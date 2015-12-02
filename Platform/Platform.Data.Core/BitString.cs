using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Platform.Data.Core
{
    /// <remarks>
    ///     А что если хранить карту значений, где каждый бит будет означать присутствует ли блок в 64 бит в массиве значений.
    ///     64 бита по 0 бит, будут означать отсутствие 64-х блоков по 64 бита. Т.е. упаковка 512 байт в 8 байт.
    ///     Подобный принцип можно применять и к 64-ём блокам и т.п. По сути это карта значений. С помощью которой можно быстро
    ///     проверять есть ли значения непосредственно далее (ниже по уровню).
    ///     Или как таблица виртуальной памяти где номер блока означает его присутствие и адрес.
    /// 
    ///     TODO: Compare what is faster to store BitSetsIn16Bits or to calculate it
    ///     TODO: Avoid int usage (replace to long)
    /// </remarks>
    public sealed class BitString : ICloneable
    {
        private static readonly byte[][] BitSetsIn16Bits;
        private long[] _array;
        private long _length;
        //private int _version;

        static BitString()
        {
            BitSetsIn16Bits = new byte[65536][];

            for (int i = 0; i < 65536; i++)
            {
                // Calculating size of array (number of positive bits)
                int c = 0;
                for (int k = 1; k <= 65536; k = k << 1)
                    if ((i & k) == k) c++;

                var array = new byte[c];

                // Adding positive bits indices into array
                byte j = 0;
                c = 0;
                for (int k = 1; k <= 65536; k = k << 1)
                {
                    if ((i & k) == k)
                        array[c++] = j;
                    j++;
                }

                BitSetsIn16Bits[i] = array;
            }
        }

        #region Constructors

        public BitString(BitString bits)
        {
            if (bits == null)
            {
                throw new ArgumentNullException("bits");
            }

            _length = bits._length;
            _array = new long[(_length + 63) / 64];

            _minPositiveWord = bits._minPositiveWord;
            _maxPositiveWord = bits._maxPositiveWord;

            if (_array.Length == 1)
            {
                _array[0] = bits._array[0];
            }
            else
            {
                Array.Copy(bits._array, _array, _array.Length);
            }
        }

        public BitString(long length)
        {
            if (length < 0)
            {
                throw new ArgumentOutOfRangeException("length");
            }

            _length = length;
            _array = new long[(_length + 63) / 64];
            _minPositiveWord = _array.Length - 1;
            _maxPositiveWord = 0;
        }

        public BitString(int length, bool defaultValue)
            : this(length)
        {
            if (defaultValue)
            {
                const int fillValue = unchecked(((int)0xffffffff));
                for (int i = 0; i < _array.Length; i++)
                {
                    _array[i] = fillValue;
                }

                _minPositiveWord = 0;
                _maxPositiveWord = _array.Length - 1;
            }
        }

        #endregion

        #region Utility Methods

        private void EnsureArgumentIsValid(BitString other)
        {
            if (other == null)
                throw new ArgumentNullException();
            if (other._length != _length)
                throw new ArgumentException();
        }

        #endregion

        private long _minPositiveWord;
        private long _maxPositiveWord;

        public long Count
        {
            get { return _length; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool IsSynchronized
        {
            get { return false; }
        }

        public bool this[int index]
        {
            get { return Get(index); }
            set { Set(index, value); }
        }

        public long Length
        {
            get { return _length; }
            set
            {
                if (_length == value)
                {
                    return;
                }

                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException();
                }

                // Currently we never shrink the array
                if (value > _length)
                {
                    long numints = (value + 63) / 64;
                    long oldNumints = (_length + 63) / 64;
                    if (numints > _array.Length)
                    {
                        var newArr = new long[numints];
                        Array.Copy(_array, newArr, _array.Length);
                        _array = newArr;
                    }
                    else
                    {
                        Array.Clear(_array, (int)oldNumints, (int)(numints - oldNumints));
                    }

                    int mask = (int)_length % 64;
                    if (mask > 0)
                    {
                        _array[oldNumints - 1] &= ((long)1 << mask) - 1;
                    }
                }

                // set the internal state
                _length = value;
                //_version++;
            }
        }

        public object SyncRoot
        {
            get { return this; }
        }

        public object Clone()
        {
            // LAMESPEC: docs say shallow, MS makes deep.
            return new BitString(this);
        }

        public BitString Not()
        {
            long ints = (_length + 63) / 64;
            for (long i = 0; i < ints; i++)
            {
                _array[i] = ~_array[i];
                RefreshBordersByWord(i);
            }

            //_version++;
            return this;
        }

        public BitString And(BitString other)
        {
#if DEBUG
            var sw = Stopwatch.StartNew();
#endif

            EnsureArgumentIsValid(other);

            long ints = (_length + 63) / 64;
            var otherArray = other._array;
            for (long i = 0; i < ints; i++)
            {
                _array[i] &= otherArray[i];
                RefreshBordersByWord(i);
            }

            //_version++;

#if DEBUG
            Console.WriteLine("AND → Len: {0}, Time: {1}", this._array.Length, sw.Elapsed.TotalMilliseconds);
#endif

            return this;
        }

        public BitString Or(BitString value)
        {
#if DEBUG
            var sw = Stopwatch.StartNew();
#endif

            EnsureArgumentIsValid(value);

            long ints = (_length + 63) / 64;
            for (long i = 0; i < ints; i++)
            {
                _array[i] |= value._array[i];
                RefreshBordersByWord(i);
            }

            //_version++;

#if DEBUG
            Console.WriteLine("OR → Len: {0}, Time: {1}", this._array.Length, sw.Elapsed.TotalMilliseconds);
#endif

            return this;
        }

        public BitString Xor(BitString value)
        {
            EnsureArgumentIsValid(value);

            long ints = (_length + 63) / 64;
            for (long i = 0; i < ints; i++)
            {
                _array[i] ^= value._array[i];
                RefreshBordersByWord(i);
            }

            //_version++;
            return this;
        }
        
        private void RefreshBordersByWord(long wordIndex)
        {
            if (_array[wordIndex] != 0)
            {
                if (wordIndex < _minPositiveWord)
                    _minPositiveWord = wordIndex;
                if (wordIndex > _maxPositiveWord)
                    _maxPositiveWord = wordIndex;
            }
            else
            {
                if (wordIndex == _minPositiveWord && wordIndex != (_array.Length - 1))
                    _minPositiveWord++;
                if (wordIndex == _maxPositiveWord && wordIndex != 0)
                    _maxPositiveWord--;
            }
        }

        /*
        private void RefreshBordersByBit(int bitIndex)
        {
            var wordIndex = GetWordIndex(bitIndex);
            var value = _array[wordIndex];
            if (value != 0)
            {
                RefreshBordersByWord(wordIndex);
            }
        }*/

        public bool Get(long index)
        {
            if (index < 0 || index >= _length)
                throw new ArgumentOutOfRangeException();

            return GetCore(index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool GetCore(long index)
        {
            long wordIndex = index >> 6;
            return (_array[wordIndex] & ((long)1 << (int)(index & 63))) != 0;
        }

        public void Set(long index)
        {
            if (index < 0 || index >= _length)
                throw new ArgumentOutOfRangeException();

            SetCore(index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetCore(long index)
        {
            long wordIndex = index >> 6;
            long mask = ((long)1 << (int)(index & 63));

            _array[wordIndex] |= mask;
            RefreshBordersByWord(wordIndex);

            //_version++;
        }

        public void Reset(long index)
        {
            if (index < 0 || index >= _length)
                throw new ArgumentOutOfRangeException();

            ResetCore(index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ResetCore(long index)
        {
            long wordIndex = index >> 6;
            long mask = ((long)1 << (int)(index & 63));

            _array[wordIndex] &= ~mask;

            //_version++;
        }

        public void Set(long index, bool value)
        {
            if (index < 0 || index >= _length)
                throw new ArgumentOutOfRangeException();
            
            if (value)
                SetCore(index);
            else
                ResetCore(index);
        }

        public void SetAll(bool value)
        {
            long fillValue = value ? unchecked(((long)0xffffffffffffffff)) : 0;
            long ints = (_length + 63) / 64;
            for (long i = 0; i < ints; i++)
            {
                _array[i] = fillValue;
            }
            if (value)
            {
                _minPositiveWord = 0;
                _maxPositiveWord = _array.Length - 1;
            }
            else
            {
                _minPositiveWord = _array.Length - 1;
                _maxPositiveWord = 0;
            }

            //_version++;
        }

        public int CountSet()
        {
            int result = 0;
            for (int i = 0; i < _array.Length; i++)
            {
                long n = _array[i];
                if (n != 0)
                {
                    result += BitSetsIn16Bits[(int)(n & 0xffffu)].Length +
                              BitSetsIn16Bits[(int)((n >> 16) & 0xffffu)].Length;
                    result += BitSetsIn16Bits[(int)((n >> 32) & 0xffffu)].Length +
                              BitSetsIn16Bits[(int)((n >> 48) & 0xffffu)].Length;
                }
            }
            return result;
        }

        public List<int> GetSetIndeces()
        {
#if DEBUG
            Stopwatch sw = Stopwatch.StartNew();
#endif
            var result = new List<int>();
            for (int i = 0; i < _array.Length; i++)
            {
                long n = _array[i];
                if (n != 0)
                {
                    var bits1 = BitSetsIn16Bits[(int)(n & 0xffffu)];
                    var bits2 = BitSetsIn16Bits[(int)((n >> 16) & 0xffffu)];
                    var bits3 = BitSetsIn16Bits[(int)((n >> 32) & 0xffffu)];
                    var bits4 = BitSetsIn16Bits[(int)((n >> 48) & 0xffffu)];

                    for (int j = 0; j < bits1.Length; j++)
                        result.Add(bits1[j] + i * 64);

                    for (int j = 0; j < bits2.Length; j++)
                        result.Add(bits2[j] + 16 + i * 64);

                    for (int j = 0; j < bits3.Length; j++)
                        result.Add(bits3[j] + 32 + i * 64);

                    for (int j = 0; j < bits4.Length; j++)
                        result.Add(bits4[j] + 48 + i * 64);
                }
            }
#if DEBUG
            Console.WriteLine("Get-Set-Indices: {0} ms. ({1} elements)", sw.Elapsed.TotalMilliseconds, _array.Length);
#endif

            return result;
        }

        public List<ulong> GetSetUInt64Indices()
        {
#if DEBUG
            Stopwatch sw = Stopwatch.StartNew();
#endif
            // TODO: Возможно нужно считать общее число установленных бит, тогда здесь можно будет создавать сразу массив
            var result = new List<ulong>();
            for (long i = 0; i < _array.Length; i++)
            {
                var n = _array[i];

                if (n == 0) continue;

                var bits1 = BitSetsIn16Bits[(int)(n & 0xffffu)];
                var bits2 = BitSetsIn16Bits[(int)((n >> 16) & 0xffffu)];
                var bits3 = BitSetsIn16Bits[(int)((n >> 32) & 0xffffu)];
                var bits4 = BitSetsIn16Bits[(int)((n >> 48) & 0xffffu)];

                for (var j = 0; j < bits1.Length; j++)
                    result.Add(bits1[j] + (ulong)i * 64);

                for (var j = 0; j < bits2.Length; j++)
                    result.Add(bits2[j] + 16UL + (ulong)i * 64);

                for (var j = 0; j < bits3.Length; j++)
                    result.Add(bits3[j] + 32UL + (ulong)i * 64);

                for (var j = 0; j < bits4.Length; j++)
                    result.Add(bits4[j] + 48UL + (ulong)i * 64);
            }
#if DEBUG
            Console.WriteLine("Get-Set-Indices: {0} ms. ({1} elements)", sw.Elapsed.TotalMilliseconds, _array.Length);
#endif

            return result;
        }

        public int GetFirstSetBitIndex()
        {
            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < _array.Length; i++)
            {
                long n = _array[i];
                if (n != 0)
                {
                    var bits1 = BitSetsIn16Bits[(int)(n & 0xffffu)];
                    var bits2 = BitSetsIn16Bits[(int)((n >> 16) & 0xffffu)];
                    var bits3 = BitSetsIn16Bits[(int)((n >> 32) & 0xffffu)];
                    var bits4 = BitSetsIn16Bits[(int)((n >> 48) & 0xffffu)];

#if DEBUG
                    Console.WriteLine("Get-First-Set-Index: {0} ms. ({1} elements)", sw.Elapsed.TotalMilliseconds, _array.Length);
#endif

                    if (bits1.Length > 0)
                        return bits1[0] + i * 64;
                    if (bits2.Length > 0)
                        return bits2[0] + 16 + i * 64;
                    if (bits3.Length > 0)
                        return bits3[0] + 32 + i * 64;
                    return bits4[0] + 48 + i * 64;
                }
            }

            return -1;
        }

        public int GetLastSetBitIndex()
        {
#if DEBUG
            Stopwatch sw = Stopwatch.StartNew();
#endif

            for (int i = _array.Length - 1; i >= 0; i--)
            {
                long n = _array[i];
                if (n != 0)
                {
                    var bits1 = BitSetsIn16Bits[(int)(n & 0xffffu)];
                    var bits2 = BitSetsIn16Bits[(int)((n >> 16) & 0xffffu)];
                    var bits3 = BitSetsIn16Bits[(int)((n >> 32) & 0xffffu)];
                    var bits4 = BitSetsIn16Bits[(int)((n >> 48) & 0xffffu)];

#if DEBUG
                    Console.WriteLine("Get-Last-Set-Index: {0} ms. ({1} elements)", sw.Elapsed.TotalMilliseconds, _array.Length);
#endif

                    if (bits4.Length > 0)
                        return bits4[bits4.Length - 1] + 48 + i * 64;
                    if (bits3.Length > 0)
                        return bits3[bits3.Length - 1] + 32 + i * 64;
                    if (bits2.Length > 0)
                        return bits2[bits2.Length - 1] + 16 + i * 64;
                    return bits1[bits1.Length - 1] + i * 64;
                }
            }

            return -1;
        }

        public int GetSetIndecesCount()
        {
            int result = 0;
            for (int i = 0; i < _array.Length; i++)
            {
                long n = _array[i];
                if (n != 0)
                {
                    var bits1 = BitSetsIn16Bits[(int)(n & 0xffffu)];
                    var bits2 = BitSetsIn16Bits[(int)((n >> 16) & 0xffffu)];
                    var bits3 = BitSetsIn16Bits[(int)((n >> 32) & 0xffffu)];
                    var bits4 = BitSetsIn16Bits[(int)((n >> 48) & 0xffffu)];

                    result += bits1.Length + bits2.Length + bits3.Length + bits4.Length;
                }
            }
            return result;
        }

        public bool HaveCommonBits(BitString other)
        {
            if (Length != other.Length)
            {
                throw new ArgumentException("Bit strings must have same size", "other");
            }

#if DEBUG
            var sw = Stopwatch.StartNew();
#endif

            long from = Math.Max(_minPositiveWord, other._minPositiveWord);
            long to = Math.Min(_maxPositiveWord, other._maxPositiveWord);

            bool result = false;

#if DEBUG
            int steps = 0;
#endif
            long[] otherArray = other._array;

            for (long i = from; i <= to; i++)
            {
#if DEBUG
                steps++;
#endif
                long v1 = _array[i];
                long v2 = otherArray[i];
                if (v1 != 0 && v2 != 0 && ((v1 & v2) != 0))
                {
                    result = true;
                    break;
                }
            }

#if DEBUG
            if (sw.Elapsed.TotalMilliseconds > 0.1)
            {
                Console.WriteLine("HCB → Min: {0}, Max: {1}, Delta: {2}, Steps: {3}, Time: {4}", from, to, to - from, steps, sw.Elapsed.TotalMilliseconds);
            }
#endif

            return result;
        }

        public int CountCommonBits(BitString other)
        {
            if (Length != other.Length)
            {
                throw new ArgumentException("Bit strings must have same size", "other");
            }

            long from = Math.Max(_minPositiveWord, other._minPositiveWord);
            long to = Math.Min(_maxPositiveWord, other._maxPositiveWord);

            int result = 0;

            long[] otherArray = other._array;

            for (long i = from; i <= to; i++)
            {
                long v1 = _array[i];
                long v2 = otherArray[i];
                long n = v1 & v2;
                if (n != 0)
                {
                    var bits1 = BitSetsIn16Bits[(int)(n & 0xffffu)];
                    var bits2 = BitSetsIn16Bits[(int)((n >> 16) & 0xffffu)];
                    var bits3 = BitSetsIn16Bits[(int)((n >> 32) & 0xffffu)];
                    var bits4 = BitSetsIn16Bits[(int)((n >> 48) & 0xffffu)];

                    result += bits1.Length + bits2.Length + bits3.Length + bits4.Length;
                }
            }

            return result;
        }

        public List<int> GetCommonIndices(BitString other)
        {
            if (Length != other.Length)
            {
                throw new ArgumentException("Bit strings must have same size", "other");
            }

            long from = Math.Max(_minPositiveWord, other._minPositiveWord);
            long to = Math.Min(_maxPositiveWord, other._maxPositiveWord);

            var result = new List<int>();

            long[] otherArray = other._array;

            for (long i = from; i <= to; i++)
            {
                long v1 = _array[i];
                long v2 = otherArray[i];
                long n = v1 & v2;
                if (n != 0)
                {
                    var bits1 = BitSetsIn16Bits[(int)(n & 0xffffu)];
                    var bits2 = BitSetsIn16Bits[(int)((n >> 16) & 0xffffu)];
                    var bits3 = BitSetsIn16Bits[(int)((n >> 32) & 0xffffu)];
                    var bits4 = BitSetsIn16Bits[(int)((n >> 48) & 0xffffu)];

                    if (bits1.Length > 0)
                        result.Add(bits1[0] + (int)i * 64);
                    else if (bits2.Length > 0)
                        result.Add(bits2[0] + 16 + (int)i * 64);
                    else if (bits3.Length > 0)
                        result.Add(bits3[0] + 32 + (int)i * 64);
                    else
                        result.Add(bits4[0] + 48 + (int)i * 64);
                }
            }

            return result;
        }

        public int GetLastCommonBitIndex(BitString other)
        {
            if (Length != other.Length)
            {
                throw new ArgumentException("Bit strings must have same size", "other");
            }

            long from = Math.Max(_minPositiveWord, other._minPositiveWord);
            long to = Math.Min(_maxPositiveWord, other._maxPositiveWord);

            long[] otherArray = other._array;

            for (long i = from; i <= to; i++)
            {
                long v1 = _array[i];
                long v2 = otherArray[i];
                long n = v1 & v2;
                if (n != 0)
                {
                    var bits1 = BitSetsIn16Bits[(int)(n & 0xffffu)];
                    var bits2 = BitSetsIn16Bits[(int)((n >> 16) & 0xffffu)];
                    var bits3 = BitSetsIn16Bits[(int)((n >> 32) & 0xffffu)];
                    var bits4 = BitSetsIn16Bits[(int)((n >> 48) & 0xffffu)];

                    if (bits4.Length > 0)
                        return bits4[bits4.Length - 1] + 48 + (int)i * 64;
                    if (bits3.Length > 0)
                        return bits3[bits3.Length - 1] + 32 + (int)i * 64;
                    if (bits2.Length > 0)
                        return bits2[bits2.Length - 1] + 16 + (int)i * 64;
                    return bits1[bits1.Length - 1] + (int)i * 64;
                }
            }

            return -1;
        }
    }
}