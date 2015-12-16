using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Threading;

namespace Platform.Helpers.Collections
{
    public class UnsafeDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IDictionary
    {
        public struct Entry
        {
            public int hashCode;    // Lower 31 bits of hash code, -1 if unused
            public int next;        // Index of next entry, -1 if last
            public TKey key;           // Key of entry
            public TValue value;         // Value of entry
        }

        private int[] buckets;
        public Entry[] entries;
        private int capacity;
        private int count;
        private int version;
        private int freeList;
        private int freeCount;
        private IEqualityComparer<TKey> comparer;
        private KeyCollection keys;
        private ValueCollection values;
        private Object _syncRoot;     

        // constants for serialization
        private const String VersionName = "Version";
        private const String HashSizeName = "HashSize";  // Must save buckets.Length
        private const String KeyValuePairsName = "KeyValuePairs";
        private const String ComparerName = "Comparer";

        public UnsafeDictionary() : this(1000, null) { }

        public UnsafeDictionary(int capacity) : this(capacity, null) { }

        public UnsafeDictionary(IEqualityComparer<TKey> comparer) : this(0, comparer) { }

        public UnsafeDictionary(int capacity, IEqualityComparer<TKey> comparer)
        {
            //if (capacity < 0) throw new ArgumentOutOfRangeException("Capacity");
            this.capacity = capacity;
            if (capacity > 0) Initialize(capacity);
            if (comparer == null) comparer = EqualityComparer<TKey>.Default;
            this.comparer = comparer;
        }

        public UnsafeDictionary(IDictionary<TKey, TValue> dictionary) : this(dictionary, null) { }

        public UnsafeDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer) :
            this(dictionary != null ? dictionary.Count : 0, comparer)
        {

            //if (dictionary == null)
            //{
            //    throw new ArgumentNullException("Dictionary");
            //}

            foreach (var pair in dictionary)
            {
                Add(pair.Key, pair.Value);
            }
        }

        public IEqualityComparer<TKey> Comparer
        {
            get
            {
                return comparer;
            }
        }

        public int Count
        {
            get { return count - freeCount; }
        }

        public KeyCollection Keys
        {
            get { return keys ?? (keys = new KeyCollection(this)); }
        }

        ICollection<TKey> IDictionary<TKey, TValue>.Keys
        {
            get { return keys ?? (keys = new KeyCollection(this)); }
        }

        public ValueCollection Values
        {
            get { return values ?? (values = new ValueCollection(this)); }
        }

        ICollection<TValue> IDictionary<TKey, TValue>.Values
        {
            get { return values ?? (values = new ValueCollection(this)); }
        }

        public TValue this[TKey key]
        {
            get
            {
                var i = FindEntry(key);
                if (i >= 0) return entries[i].value;
                //throw new KeyNotFoundException();
                return default(TValue);
            }
            set
            {
                Insert(key, value, false);
            }
        }

        public void Add(TKey key, TValue value)
        {
            Insert(key, value, true);
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> keyValuePair)
        {
            Add(keyValuePair.Key, keyValuePair.Value);
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> keyValuePair)
        {
            var i = FindEntry(keyValuePair.Key);
            if (i >= 0 && EqualityComparer<TValue>.Default.Equals(entries[i].value, keyValuePair.Value))
            {
                return true;
            }
            return false;
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> keyValuePair)
        {
            var i = FindEntry(keyValuePair.Key);
            if (i >= 0 && EqualityComparer<TValue>.Default.Equals(entries[i].value, keyValuePair.Value))
            {
                Remove(keyValuePair.Key);
                return true;
            }
            return false;
        }

        public void Clear()
        {
            if (count > 0)
            {
                for (var i = 0; i < buckets.Length; i++) buckets[i] = -1;
                Array.Clear(entries, 0, count);
                freeList = -1;
                count = 0;
                freeCount = 0;
                version++;
            }
        }

        public bool ContainsKey(TKey key)
        {
            return FindEntry(key) >= 0;
        }

        public bool ContainsValue(TValue value)
        {
            if (value == null)
            {
                for (var i = 0; i < count; i++)
                {
                    if (entries[i].hashCode >= 0 && entries[i].value == null) return true;
                }
            }
            else
            {
                var c = EqualityComparer<TValue>.Default;
                for (var i = 0; i < count; i++)
                {
                    if (entries[i].hashCode >= 0 && c.Equals(entries[i].value, value)) return true;
                }
            }
            return false;
        }

        private void CopyTo(KeyValuePair<TKey, TValue>[] array, int index)
        {
            //if (array == null)
            //{
            //    throw new ArgumentNullException("array");
            //}

            //if (index < 0 || index > array.Length)
            //{
            //    throw new ArgumentOutOfRangeException("index", "ArgumentOutOfRange_NeedNonNegNum");
            //}

            //if (array.Length - index < Count)
            //{
            //    throw new ArgumentException("Arg_ArrayPlusOffTooSmall");
            //}

            var count = this.count;
            var entries = this.entries;
            for (var i = 0; i < count; i++)
            {
                if (entries[i].hashCode >= 0)
                {
                    array[index++] = new KeyValuePair<TKey, TValue>(entries[i].key, entries[i].value);
                }
            }
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(this, Enumerator.KeyValuePair);
        }

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            return new Enumerator(this, Enumerator.KeyValuePair);
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            //if (info == null)
            //{
            //    throw new ArgumentNullException("info");
            //}
            info.AddValue(VersionName, version);
            info.AddValue(ComparerName, comparer, typeof(IEqualityComparer<TKey>));
            info.AddValue(HashSizeName, buckets == null ? 0 : buckets.Length); //This is the length of the bucket array.
            if (buckets != null)
            {
                var array = new KeyValuePair<TKey, TValue>[Count];
                CopyTo(array, 0);
                info.AddValue(KeyValuePairsName, array, typeof(KeyValuePair<TKey, TValue>[]));
            }
        }

        private int FindEntry(TKey key)
        {
            //if (key == null)
            //{
            //    throw new ArgumentNullException("key");
            //}

            if (buckets != null)
            {
                var hashCode = comparer.GetHashCode(key) & 0x7FFFFFFF;
                for (var i = buckets[hashCode % buckets.Length]; i >= 0; i = entries[i].next)
                {

                    if (entries[i].hashCode == hashCode)
                        if (comparer.Equals(entries[i].key, key))
                            return i;

                }
            }
            return -1;
        }

        private void Initialize(int capacity)
        {
            var size = HashHelpers.GetPrime(capacity);
            buckets = new int[size];
            for (var i = 0; i < buckets.Length; i++) buckets[i] = -1;
            entries = new Entry[size];
            freeList = -1;
        }

        public int InitOrGetPosition(TKey key)
        {
            return Insert(key, default(TValue), true);
        }

        public void StoreAtPosition(int pos, TValue value)
        {
            entries[pos].value = value;
            version++;
        }

        public TValue GetAtPosition(int pos)
        {
            return entries[pos].value;
        }

        private int Insert(TKey key, TValue value, bool add)
        {
            //if (key == null)
            //{
            //    throw new ArgumentNullException("key");
            //}



            if (buckets == null) Initialize(capacity);
            var hashCode = comparer.GetHashCode(key) & 0x7FFFFFFF;
            for (var i = buckets[hashCode % buckets.Length]; i >= 0; i = entries[i].next)
            {
                if (entries[i].hashCode == hashCode && comparer.Equals(entries[i].key, key))
                {
                    if (add)
                    {
                        return i;
                    }
                    entries[i].value = value;
                    version++;
                    return i;
                }
            }
            int index;
            if (freeCount > 0)
            {
                index = freeList;
                freeList = entries[index].next;
                freeCount--;
            }
            else
            {
                if (count == entries.Length) Resize();
                index = count;
                count++;
            }
            var bucket = hashCode % buckets.Length;
            entries[index].hashCode = hashCode;
            entries[index].next = buckets[bucket];
            entries[index].key = key;
            entries[index].value = value;
            buckets[bucket] = index;
            version++;

            return index;
        }

        private void Resize()
        {
            var newSize = HashHelpers.GetPrime(count * 2);
            var newBuckets = new int[newSize];
            for (var i = 0; i < newBuckets.Length; i++) newBuckets[i] = -1;
            var newEntries = new Entry[newSize];
            Array.Copy(entries, 0, newEntries, 0, count);
            for (var i = 0; i < count; i++)
            {
                var bucket = newEntries[i].hashCode % newSize;
                newEntries[i].next = newBuckets[bucket];
                newBuckets[bucket] = i;
            }
            buckets = newBuckets;
            entries = newEntries;
        }

        public bool Remove(TKey key)
        {
            //if (key == null)
            //{
            //    throw new ArgumentNullException("key");
            //}

            if (buckets != null)
            {
                var hashCode = comparer.GetHashCode(key) & 0x7FFFFFFF;
                var bucket = hashCode % buckets.Length;
                var last = -1;
                for (var i = buckets[bucket]; i >= 0; last = i, i = entries[i].next)
                {
                    if (entries[i].hashCode == hashCode && comparer.Equals(entries[i].key, key))
                    {
                        if (last < 0)
                        {
                            buckets[bucket] = entries[i].next;
                        }
                        else
                        {
                            entries[last].next = entries[i].next;
                        }
                        entries[i].hashCode = -1;
                        entries[i].next = freeList;
                        entries[i].key = default(TKey);
                        entries[i].value = default(TValue);
                        freeList = i;
                        freeCount++;
                        version++;
                        return true;
                    }
                }
            }
            return false;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            var i = FindEntry(key);
            if (i >= 0)
            {
                value = entries[i].value;
                return true;
            }
            value = default(TValue);
            return false;
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly
        {
            get { return false; }
        }

        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int index)
        {
            CopyTo(array, index);
        }

        void ICollection.CopyTo(Array array, int index)
        {
            //if (array == null)
            //{
            //    throw new ArgumentNullException("array");
            //}

            //if (array.Rank != 1)
            //{
            //    throw new ArgumentException("Arg_RankMultiDimNotSupported");
            //}

            //if (array.GetLowerBound(0) != 0)
            //{
            //    throw new ArgumentException("Arg_NonZeroLowerBound");
            //}

            //if (index < 0 || index > array.Length)
            //{
            //    throw new ArgumentOutOfRangeException("index", "ArgumentOutOfRange_NeedNonNegNum");
            //}

            //if (array.Length - index < Count)
            //{
            //    throw new ArgumentException("Arg_ArrayPlusOffTooSmall");
            //}

            var pairs = array as KeyValuePair<TKey, TValue>[];
            if (pairs != null)
            {
                CopyTo(pairs, index);
            }
            else if (array is DictionaryEntry[])
            {
                var dictEntryArray = array as DictionaryEntry[];
                var entries = this.entries;
                for (var i = 0; i < count; i++)
                {
                    if (entries[i].hashCode >= 0)
                    {
                        dictEntryArray[index++] = new DictionaryEntry(entries[i].key, entries[i].value);
                    }
                }
            }
            else
            {
                var objects = array as object[];
                //if (objects == null)
                //{
                //    throw new ArgumentException("Argument_InvalidArrayType");
                //}

                //try
                //{
                    var count = this.count;
                    var entries = this.entries;
                    for (var i = 0; i < count; i++)
                    {
                        if (entries[i].hashCode >= 0)
                        {
                            objects[index++] = new KeyValuePair<TKey, TValue>(entries[i].key, entries[i].value);
                        }
                    }
                //}
                //catch (ArrayTypeMismatchException)
                //{
                //    throw new ArgumentException("Argument_InvalidArrayType");
                //}
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(this, Enumerator.KeyValuePair);
        }

        bool ICollection.IsSynchronized
        {
            get { return false; }
        }

        object ICollection.SyncRoot
        {
            get
            {
                if (_syncRoot == null)
                {
                    Interlocked.CompareExchange(ref _syncRoot, new Object(), null);
                }
                return _syncRoot;
            }
        }

        bool IDictionary.IsFixedSize
        {
            get { return false; }
        }

        bool IDictionary.IsReadOnly
        {
            get { return false; }
        }

        ICollection IDictionary.Keys
        {
            get { return Keys; }
        }

        ICollection IDictionary.Values
        {
            get { return Values; }
        }

        object IDictionary.this[object key]
        {
            get
            {
                //if (IsCompatibleKey(key))
                //{
                    var i = FindEntry((TKey)key);
                    if (i >= 0)
                    {
                        return entries[i].value;
                    }
                //}
                return null;
            }
            set
            {
                //VerifyKey(key);
                //VerifyValueType(value);
                this[(TKey)key] = (TValue)value;
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void VerifyKey(object key)
        {
            //if (key == null)
            //{
            //    throw new ArgumentNullException("key");
            //}

            //if (!(key is TKey))
            //{
            //    throw new ArgumentException("Invalid type", "key");
            //}
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsCompatibleKey(object key)
        {
            //if (key == null)
            //{
            //    throw new ArgumentNullException("key");
            //}

            return (key is TKey);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void VerifyValueType(object value)
        {
            //if ((value is TValue) || (value == null && !typeof(TValue).IsValueType))
            //{
            //    return;
            //}
            //throw new ArgumentException("Invalid type", "value");
        }

        void IDictionary.Add(object key, object value)
        {
            //VerifyKey(key);
            //VerifyValueType(value);
            Add((TKey)key, (TValue)value);
        }

        bool IDictionary.Contains(object key)
        {
            //if (IsCompatibleKey(key))
            //{
                return ContainsKey((TKey)key);
            //}
            //return false;
        }

        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return new Enumerator(this, Enumerator.DictEntry);
        }

        void IDictionary.Remove(object key)
        {
            //if (IsCompatibleKey(key))
            //{
                Remove((TKey)key);
            //}
        }

        [Serializable()]
        public struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>,
            IDictionaryEnumerator
        {
            private UnsafeDictionary<TKey, TValue> dictionary;
            private int version;
            private int index;
            private KeyValuePair<TKey, TValue> current;
            private int getEnumeratorRetType;  // What should Enumerator.Current return?

            internal const int DictEntry = 1;
            internal const int KeyValuePair = 2;

            internal Enumerator(UnsafeDictionary<TKey, TValue> dictionary, int getEnumeratorRetType)
            {
                this.dictionary = dictionary;
                version = dictionary.version;
                index = 0;
                this.getEnumeratorRetType = getEnumeratorRetType;
                current = new KeyValuePair<TKey, TValue>();
            }

            public bool MoveNext()
            {
                //if (version != dictionary.version)
                //{
                //    throw new InvalidOperationException("InvalidOperation_EnumFailedVersion");
                //}

                // Use unsigned comparison since we set index to dictionary.count+1 when the enumeration ends.
                // dictionary.count+1 could be negative if dictionary.count is Int32.MaxValue
                var count = dictionary.count;
                var entries = dictionary.entries;
                while ((uint)index < (uint)count)
                {
                    if (entries[index].hashCode >= 0)
                    {
                        current = new KeyValuePair<TKey, TValue>(entries[index].key, entries[index].value);
                        index++;
                        return true;
                    }
                    index++;
                }

                index = count + 1;
                current = new KeyValuePair<TKey, TValue>();
                return false;
            }

            public KeyValuePair<TKey, TValue> Current
            {
                get { return current; }
            }

            public void Dispose()
            {
            }

            object IEnumerator.Current
            {
                get
                {
                    //if (index == 0 || (index == dictionary.count + 1))
                    //{
                    //    throw new InvalidOperationException("InvalidOperation_EnumOpCantHappen");
                    //}

                    if (getEnumeratorRetType == DictEntry)
                    {
                        return new DictionaryEntry(current.Key, current.Value);
                    }
                    else
                    {
                        return new KeyValuePair<TKey, TValue>(current.Key, current.Value);
                    }
                }
            }

            void IEnumerator.Reset()
            {
                //if (version != dictionary.version)
                //{
                //    throw new InvalidOperationException("InvalidOperation_EnumFailedVersion");
                //}

                index = 0;
                current = new KeyValuePair<TKey, TValue>();
            }

            DictionaryEntry IDictionaryEnumerator.Entry
            {
                get
                {
                    //if (index == 0 || (index == dictionary.count + 1))
                    //{
                    //    throw new InvalidOperationException("InvalidOperation_EnumOpCantHappen");
                    //}

                    return new DictionaryEntry(current.Key, current.Value);
                }
            }

            object IDictionaryEnumerator.Key
            {
                get
                {
                    //if (index == 0 || (index == dictionary.count + 1))
                    //{
                    //    throw new InvalidOperationException("InvalidOperation_EnumOpCantHappen");
                    //}

                    return current.Key;
                }
            }

            object IDictionaryEnumerator.Value
            {
                get
                {
                    //if (index == 0 || (index == dictionary.count + 1))
                    //{
                    //    throw new InvalidOperationException("InvalidOperation_EnumOpCantHappen");
                    //}

                    return current.Value;
                }
            }
        }

        [DebuggerDisplay("Count = {Count}")]
        [Serializable()]
        public sealed class KeyCollection : ICollection<TKey>, ICollection
        {
            private UnsafeDictionary<TKey, TValue> dictionary;

            public KeyCollection(UnsafeDictionary<TKey, TValue> dictionary)
            {
                //if (dictionary == null)
                //{
                //    throw new ArgumentNullException("dictionary");
                //}
                this.dictionary = dictionary;
            }

            public Enumerator GetEnumerator()
            {
                return new Enumerator(dictionary);
            }

            public void CopyTo(TKey[] array, int index)
            {
                //if (array == null)
                //{
                //    throw new ArgumentNullException("array");
                //}

                //if (index < 0 || index > array.Length)
                //{
                //    throw new ArgumentOutOfRangeException("index", "ArgumentOutOfRange_NeedNonNegNum");
                //}

                //if (array.Length - index < dictionary.Count)
                //{
                //    throw new ArgumentException("Arg_ArrayPlusOffTooSmall");
                //}

                var count = dictionary.count;
                var entries = dictionary.entries;
                for (var i = 0; i < count; i++)
                {
                    if (entries[i].hashCode >= 0) array[index++] = entries[i].key;
                }
            }

            public int Count
            {
                get { return dictionary.Count; }
            }

            bool ICollection<TKey>.IsReadOnly
            {
                get { return true; }
            }

            void ICollection<TKey>.Add(TKey item)
            {
                //throw new NotSupportedException("NotSupported_KeyCollectionSet");
            }

            void ICollection<TKey>.Clear()
            {
                //throw new NotSupportedException("NotSupported_KeyCollectionSet");
            }

            bool ICollection<TKey>.Contains(TKey item)
            {
                return dictionary.ContainsKey(item);
            }

            bool ICollection<TKey>.Remove(TKey item)
            {
                //throw new NotSupportedException("NotSupported_KeyCollectionSet");
                return false;
            }

            IEnumerator<TKey> IEnumerable<TKey>.GetEnumerator()
            {
                return new Enumerator(dictionary);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return new Enumerator(dictionary);
            }

            void ICollection.CopyTo(Array array, int index)
            {
                //if (array == null)
                //{
                //    throw new ArgumentNullException("array");
                //}

                //if (array.Rank != 1)
                //{
                //    throw new ArgumentException("Arg_RankMultiDimNotSupported");
                //}

                //if (array.GetLowerBound(0) != 0)
                //{
                //    throw new ArgumentException("Arg_NonZeroLowerBound");
                //}

                //if (index < 0 || index > array.Length)
                //{
                //    throw new ArgumentOutOfRangeException("index", "ArgumentOutOfRange_NeedNonNegNum");
                //}

                //if (array.Length - index < dictionary.Count)
                //{
                //    throw new ArgumentException("Arg_ArrayPlusOffTooSmall");
                //}

                var keys = array as TKey[];
                if (keys != null)
                {
                    CopyTo(keys, index);
                }
                else
                {
                    var objects = array as object[];
                    //if (objects == null)
                    //{
                    //    throw new ArgumentException("Argument_InvalidArrayType");
                    //}

                    var count = dictionary.count;
                    var entries = dictionary.entries;
                    //try
                    //{
                        for (var i = 0; i < count; i++)
                        {
                            if (entries[i].hashCode >= 0) objects[index++] = entries[i].key;
                        }
                    //}
                    //catch (ArrayTypeMismatchException)
                    //{
                    //    throw new ArgumentException("Argument_InvalidArrayType");
                    //}
                }
            }

            bool ICollection.IsSynchronized
            {
                get { return false; }
            }

            Object ICollection.SyncRoot
            {
                get { return ((ICollection)dictionary).SyncRoot; }
            }

            [Serializable()]
            public struct Enumerator : IEnumerator<TKey>
            {
                private UnsafeDictionary<TKey, TValue> dictionary;
                private int index;
                private int version;
                private TKey currentKey;

                internal Enumerator(UnsafeDictionary<TKey, TValue> dictionary)
                {
                    this.dictionary = dictionary;
                    version = dictionary.version;
                    index = 0;
                    currentKey = default(TKey);
                }

                public void Dispose()
                {
                }

                public bool MoveNext()
                {
                    //if (version != dictionary.version)
                    //{
                    //    throw new InvalidOperationException("InvalidOperation_EnumFailedVersion");
                    //}

                    while ((uint)index < (uint)dictionary.count)
                    {
                        if (dictionary.entries[index].hashCode >= 0)
                        {
                            currentKey = dictionary.entries[index].key;
                            index++;
                            return true;
                        }
                        index++;
                    }

                    index = dictionary.count + 1;
                    currentKey = default(TKey);
                    return false;
                }

                public TKey Current
                {
                    get
                    {
                        return currentKey;
                    }
                }

                Object IEnumerator.Current
                {
                    get
                    {
                        //if (index == 0 || (index == dictionary.count + 1))
                        //{
                        //    throw new InvalidOperationException("InvalidOperation_EnumOpCantHappen");
                        //}

                        return currentKey;
                    }
                }

                void IEnumerator.Reset()
                {
                    //if (version != dictionary.version)
                    //{
                    //    throw new InvalidOperationException("InvalidOperation_EnumFailedVersion");
                    //}

                    index = 0;
                    currentKey = default(TKey);
                }
            }
        }

        [DebuggerDisplay("Count = {Count}")]
        [Serializable()]
        public sealed class ValueCollection : ICollection<TValue>, ICollection
        {
            private UnsafeDictionary<TKey, TValue> dictionary;

            public ValueCollection(UnsafeDictionary<TKey, TValue> dictionary)
            {
                //if (dictionary == null)
                //{
                //    throw new ArgumentNullException("dictionary");
                //}
                this.dictionary = dictionary;
            }

            public Enumerator GetEnumerator()
            {
                return new Enumerator(dictionary);
            }

            public void CopyTo(TValue[] array, int index)
            {
                //if (array == null)
                //{
                //    throw new ArgumentNullException("array");
                //}

                //if (index < 0 || index > array.Length)
                //{
                //    throw new ArgumentOutOfRangeException("index", "ArgumentOutOfRange_NeedNonNegNum");
                //}

                //if (array.Length - index < dictionary.Count)
                //{
                //    throw new ArgumentException("Arg_ArrayPlusOffTooSmall");
                //}

                var count = dictionary.count;
                var entries = dictionary.entries;
                for (var i = 0; i < count; i++)
                {
                    if (entries[i].hashCode >= 0) array[index++] = entries[i].value;
                }
            }

            public int Count
            {
                get { return dictionary.Count; }
            }

            bool ICollection<TValue>.IsReadOnly
            {
                get { return true; }
            }

            void ICollection<TValue>.Add(TValue item)
            {
                //throw new NotSupportedException("NotSupported_ValueCollectionSet");
            }

            bool ICollection<TValue>.Remove(TValue item)
            {
                //throw new NotSupportedException("NotSupported_ValueCollectionSet");
                return false;
            }

            void ICollection<TValue>.Clear()
            {
                //throw new NotSupportedException("NotSupported_ValueCollectionSet");
            }

            bool ICollection<TValue>.Contains(TValue item)
            {
                return dictionary.ContainsValue(item);
            }

            IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator()
            {
                return new Enumerator(dictionary);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return new Enumerator(dictionary);
            }

            void ICollection.CopyTo(Array array, int index)
            {
                //if (array == null)
                //{
                //    throw new ArgumentNullException("array");
                //}

                //if (array.Rank != 1)
                //{
                //    throw new ArgumentException("Arg_RankMultiDimNotSupported");
                //}

                //if (array.GetLowerBound(0) != 0)
                //{
                //    throw new ArgumentException("Arg_NonZeroLowerBound");
                //}

                //if (index < 0 || index > array.Length)
                //{
                //    throw new ArgumentOutOfRangeException("index", "ArgumentOutOfRange_NeedNonNegNum");
                //}

                //if (array.Length - index < dictionary.Count)
                //    throw new ArgumentException("Arg_ArrayPlusOffTooSmall");

                var values = array as TValue[];
                if (values != null)
                {
                    CopyTo(values, index);
                }
                else
                {
                    var objects = array as object[];
                    //if (objects == null)
                    //{
                    //    throw new ArgumentException("Argument_InvalidArrayType");
                    //}

                    var count = dictionary.count;
                    var entries = dictionary.entries;
                    //try
                    //{
                        for (var i = 0; i < count; i++)
                        {
                            if (entries[i].hashCode >= 0) objects[index++] = entries[i].value;
                        }
                    //}
                    //catch (ArrayTypeMismatchException)
                    //{
                    //    throw new ArgumentException("Argument_InvalidArrayType");
                    //}
                }
            }

            bool ICollection.IsSynchronized
            {
                get { return false; }
            }

            Object ICollection.SyncRoot
            {
                get { return ((ICollection)dictionary).SyncRoot; }
            }

            [Serializable()]
            public struct Enumerator : IEnumerator<TValue>
            {
                private UnsafeDictionary<TKey, TValue> dictionary;
                private int index;
                private int version;
                private TValue currentValue;

                internal Enumerator(UnsafeDictionary<TKey, TValue> dictionary)
                {
                    this.dictionary = dictionary;
                    version = dictionary.version;
                    index = 0;
                    currentValue = default(TValue);
                }

                public void Dispose()
                {
                }

                public bool MoveNext()
                {
                    //if (version != dictionary.version)
                    //{
                    //    throw new InvalidOperationException("InvalidOperation_EnumFailedVersion");
                    //}

                    while ((uint)index < (uint)dictionary.count)
                    {
                        if (dictionary.entries[index].hashCode >= 0)
                        {
                            currentValue = dictionary.entries[index].value;
                            index++;
                            return true;
                        }
                        index++;
                    }
                    index = dictionary.count + 1;
                    currentValue = default(TValue);
                    return false;
                }

                public TValue Current
                {
                    get
                    {
                        return currentValue;
                    }
                }

                Object IEnumerator.Current
                {
                    get
                    {
                        //if (index == 0 || (index == dictionary.count + 1))
                        //{
                        //    throw new InvalidOperationException("InvalidOperation_EnumOpCantHappen");
                        //}

                        return currentValue;
                    }
                }

                void IEnumerator.Reset()
                {
                    //if (version != dictionary.version)
                    //{
                    //    throw new InvalidOperationException("InvalidOperation_EnumFailedVersion");
                    //}
                    index = 0;
                    currentValue = default(TValue);
                }
            }
        }
    }
}