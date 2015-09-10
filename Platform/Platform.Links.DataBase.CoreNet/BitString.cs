using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Platform.Links.DataBase.CoreNet
{
    /// <remarks>
    /// А что если хранить карту значений, где каждый бит будет означать присутствует ли блок в 64 бит в массиве значений.
    /// 64 бита по 0 бит, будут означать отсутствие 64-х блоков по 64 бита. Т.е. упаковка 512 байт в 8 байт.
    /// Подобный принцип можно применять и к 64-ём блокам и т.п. По сути это карта значений. С помощью которой можно быстро проверять есть ли значения непосредственно далее (ниже по уровню).
    /// 
    /// Или как таблица виртуальной памяти где номер блока означает его присутствие и адрес.
    /// </remarks>
    public sealed class BitString : ICloneable
    {
        static readonly Dictionary<int, List<byte>> BitSetsIn16Bits;
        private long[] _array;
        private int _length;
        private int _version;
        private int MinPositiveWord { get; set; }
        private int MaxPositiveWord { get; set; }

        static BitString()
        {
            BitSetsIn16Bits = new Dictionary<int, List<byte>>();

            for (var i = 0; i < 65536; i++)
            {
                BitSetsIn16Bits.Add(i, new List<byte>());
                var k = 1;
                byte j = 0;
                while (k <= 65536)
                {
                    if ((i & k) == k)
                    {
                        BitSetsIn16Bits[i].Add(j);
                    }
                    j++;
                    k = k << 1;
                }
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

            MinPositiveWord = bits.MinPositiveWord;
            MaxPositiveWord = bits.MaxPositiveWord;

            if (_array.Length == 1)
            {
                _array[0] = bits._array[0];
            }
            else
            {
                Array.Copy(bits._array, _array, _array.Length);
            }
        }

        public BitString(int length)
        {
            if (length < 0)
            {
                throw new ArgumentOutOfRangeException("length");
            }

            _length = length;
            _array = new long[(_length + 63) / 64];
            MinPositiveWord = _array.Length - 1;
            MaxPositiveWord = 0;
        }

        public BitString(int length, bool defaultValue)
            : this(length)
        {
            if (defaultValue)
            {
                const int fillValue = unchecked(((int)0xffffffff));
                for (var i = 0; i < _array.Length; i++)
                {
                    _array[i] = fillValue;
                }

                MinPositiveWord = 0;
                MaxPositiveWord = _array.Length - 1;
            }
        }

        #endregion

        #region Utility Methods        

        void checkOperand(BitString operand)
        {
            if (operand == null)
            {
                throw new ArgumentNullException();
            }

            if (operand._length != _length)
            {
                throw new ArgumentException();
            }
        }
        #endregion

        public int Count
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

        public int Length
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
                    var numints = (value + 63) / 64;
                    var oldNumints = (_length + 63) / 64;
                    if (numints > _array.Length)
                    {
                        var newArr = new long[numints];
                        Array.Copy(_array, newArr, _array.Length);
                        _array = newArr;
                    }
                    else
                    {
                        Array.Clear(_array, oldNumints, numints - oldNumints);
                    }

                    var mask = _length % 64;
                    if (mask > 0)
                    {
                        _array[oldNumints - 1] &= ((long)1 << mask) - 1;
                    }
                }

                // set the internal state
                _length = value;
                _version++;
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
            var ints = (_length + 63) / 64;
            for (var i = 0; i < ints; i++)
            {
                _array[i] = ~_array[i];
                RefreshBordersByWord(i);
            }

            _version++;
            return this;
        }

        public BitString And(BitString value)
        {
            //var sw = Stopwatch.StartNew();

            checkOperand(value);

            var ints = (_length + 63) / 64;
            for (var i = 0; i < ints; i++)
            {
                _array[i] &= value._array[i];
                RefreshBordersByWord(i);
            }

            _version++;

            //Console.WriteLine("AND -> Len: {0}, Time: {1}", this._array.Length, sw.Elapsed.TotalMilliseconds);

            return this;
        }

        public BitString Or(BitString value)
        {
            //var sw = Stopwatch.StartNew();

            checkOperand(value);

            var ints = (_length + 63) / 64;
            for (var i = 0; i < ints; i++)
            {
                _array[i] |= value._array[i];
                RefreshBordersByWord(i);
            }

            _version++;

            //Console.WriteLine("OR -> Len: {0}, Time: {1}", this._array.Length, sw.Elapsed.TotalMilliseconds);

            return this;
        }

        public BitString Xor(BitString value)
        {
            checkOperand(value);

            var ints = (_length + 63) / 64;
            for (var i = 0; i < ints; i++)
            {
                _array[i] ^= value._array[i];
                RefreshBordersByWord(i);
            }

            _version++;
            return this;
        }

        private int GetWordIndex(int bitIndex)
        {
            return bitIndex >> 6;
        }

        private void RefreshBordersByWord(int wordIndex)
        {
            if (_array[wordIndex] != 0)
            {
                if (wordIndex < MinPositiveWord)
                {
                    MinPositiveWord = wordIndex;
                }
                if (wordIndex > MaxPositiveWord)
                {
                    MaxPositiveWord = wordIndex;
                }
            }
            else
            {
                if (wordIndex == MinPositiveWord && wordIndex != (_array.Length - 1))
                {
                    MinPositiveWord++;
                }
                if (wordIndex == MaxPositiveWord && wordIndex != 0)
                {
                    MaxPositiveWord--;
                }
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

        public bool Get(int index)
        {
            if (index < 0 || index >= _length)
            {
                throw new ArgumentOutOfRangeException();
            }

            var wordIndex = GetWordIndex(index);

            return (_array[wordIndex] & (1 << (index & 63))) != 0;
        }

        public void Set(int index, bool value)
        {
            if (index < 0 || index >= _length)
            {
                throw new ArgumentOutOfRangeException();
            }

            var wordIndex = GetWordIndex(index);

            if (value)
            {
                _array[wordIndex] |= ((long)1 << (index & 63));
                RefreshBordersByWord(wordIndex);
            }
            else
            {
                _array[wordIndex] &= ~((long)1 << (index & 63));
            }

            _version++;
        }

        public void SetAll(bool value)
        {
            var fillValue = value ? unchecked(((long)0xffffffffffffffff)) : 0;
            var ints = (_length + 63) / 64;
            for (var i = 0; i < ints; i++)
            {
                _array[i] = fillValue;
            }
            if (value)
            {
                MinPositiveWord = 0;
                MaxPositiveWord = _array.Length - 1;
            }
            else
            {
                MinPositiveWord = _array.Length - 1;
                MaxPositiveWord = 0;
            }

            _version++;
        }

        public int CountSet()
        {
            var result = 0;
            for (var i = 0; i < _array.Length; i++)
            {
                var n = _array[i];
                if (n != 0)
                {
                    result += BitSetsIn16Bits[(int)(n & 0xffffu)].Count + BitSetsIn16Bits[(int)((n >> 16) & 0xffffu)].Count;
                    result += BitSetsIn16Bits[(int)((n >> 32) & 0xffffu)].Count + BitSetsIn16Bits[(int)((n >> 48) & 0xffffu)].Count;
                }
            }
            return result;
        }

        public List<int> GetSetIndeces()
        {
            var sw = Stopwatch.StartNew();
            var result = new List<int>();
            for (var i = 0; i < _array.Length; i++)
            {
                var n = _array[i];
                if (n != 0)
                {
                    var bits1 = BitSetsIn16Bits[(int)(n & 0xffffu)];
                    var bits2 = BitSetsIn16Bits[(int)((n >> 16) & 0xffffu)];
                    var bits3 = BitSetsIn16Bits[(int)((n >> 32) & 0xffffu)];
                    var bits4 = BitSetsIn16Bits[(int)((n >> 48) & 0xffffu)];

                    for (var j = 0; j < bits1.Count; j++)
                    {
                        result.Add(bits1[j] + i * 64);
                    }

                    for (var j = 0; j < bits2.Count; j++)
                    {
                        result.Add(bits2[j] + 16 + i * 64);
                    }

                    for (var j = 0; j < bits3.Count; j++)
                    {
                        result.Add(bits3[j] + 32 + i * 64);
                    }

                    for (var j = 0; j < bits4.Count; j++)
                    {
                        result.Add(bits4[j] + 48 + i * 64);
                    }
                }
            }

            Console.WriteLine("Get-Set-Indices: {0} ms. ({1} elements)", sw.Elapsed.TotalMilliseconds, _array.Length);

            return result;
        }

        public int GetFirstSetBitIndex()
        {
            var sw = Stopwatch.StartNew();            
            for (var i = 0; i < _array.Length; i++)
            {
                var n = _array[i];
                if (n != 0)
                {
                    var bits1 = BitSetsIn16Bits[(int)(n & 0xffffu)];
                    var bits2 = BitSetsIn16Bits[(int)((n >> 16) & 0xffffu)];
                    var bits3 = BitSetsIn16Bits[(int)((n >> 32) & 0xffffu)];
                    var bits4 = BitSetsIn16Bits[(int)((n >> 48) & 0xffffu)];

                    Console.WriteLine("Get-First-Set-Index: {0} ms. ({1} elements)", sw.Elapsed.TotalMilliseconds, _array.Length);

                    if (bits1.Count > 0)
                    {
                        return bits1[0] + i * 64;
                    }
                    else if (bits2.Count > 0)
                    {
                        return bits2[0] + 16 + i * 64;
                    }
                    else if (bits3.Count > 0)
                    {
                        return bits3[0] + 32 + i * 64;
                    }
                    else
                    {
                        return bits4[0] + 48 + i * 64;
                    }
                }
            }

            return -1;
        }

        public int GetLastSetBitIndex()
        {
            var sw = Stopwatch.StartNew();
            for (var i = _array.Length - 1 ; i >= 0; i--)
            {
                var n = _array[i];
                if (n != 0)
                {
                    var bits1 = BitSetsIn16Bits[(int)(n & 0xffffu)];
                    var bits2 = BitSetsIn16Bits[(int)((n >> 16) & 0xffffu)];
                    var bits3 = BitSetsIn16Bits[(int)((n >> 32) & 0xffffu)];
                    var bits4 = BitSetsIn16Bits[(int)((n >> 48) & 0xffffu)];

                    Console.WriteLine("Get-Last-Set-Index: {0} ms. ({1} elements)", sw.Elapsed.TotalMilliseconds, _array.Length);

                    if (bits4.Count > 0)
                    {
                        return bits4[bits4.Count - 1] + 48 + i * 64;                        
                    }
                    else if (bits3.Count > 0)
                    {
                        return bits3[bits3.Count - 1] + 32 + i * 64;                        
                    }
                    else if (bits2.Count > 0)
                    {
                        return bits2[bits2.Count - 1] + 16 + i * 64;
                    }
                    else
                    {
                        return bits1[bits1.Count - 1] + i * 64;
                    }
                }
            }

            return -1;
        }

        public int GetSetIndecesCount()
        {
            var result = 0;
            for (var i = 0; i < _array.Length; i++)
            {
                var n = _array[i];
                if (n != 0)
                {
                    var bits1 = BitSetsIn16Bits[(int)(n & 0xffffu)];
                    var bits2 = BitSetsIn16Bits[(int)((n >> 16) & 0xffffu)];
                    var bits3 = BitSetsIn16Bits[(int)((n >> 32) & 0xffffu)];
                    var bits4 = BitSetsIn16Bits[(int)((n >> 48) & 0xffffu)];

                    result += bits1.Count + bits2.Count + bits3.Count + bits4.Count;
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

            //var sw = Stopwatch.StartNew();

            var from = Math.Max(MinPositiveWord, other.MinPositiveWord);
            var to = Math.Min(MaxPositiveWord, other.MaxPositiveWord);

            var result = false;

            var steps = 0;

            var otherArray = other._array;

            for (var i = from; i <= to; i++)
            {
                steps++;
                var v1 = _array[i];
                var v2 = otherArray[i];
                if (v1 != 0 && v2 != 0 && ((v1 & v2) != 0))
                {
                    result = true;
                    break;
                }
            }

            /*
            if (sw.Elapsed.TotalMilliseconds > 0.1)
            {
                Console.WriteLine("HCB -> Min: {0}, Max: {1}, Delta: {2}, Steps: {3}, Time: {4}", from, to, to - from, steps, sw.Elapsed.TotalMilliseconds);
            }
            */

            return result;
        }

        public int CountCommonBits(BitString other)
        {
            if (Length != other.Length)
            {
                throw new ArgumentException("Bit strings must have same size", "other");
            }

            var from = Math.Max(MinPositiveWord, other.MinPositiveWord);
            var to = Math.Min(MaxPositiveWord, other.MaxPositiveWord);

            var result = 0;

            var otherArray = other._array;

            for (var i = from; i <= to; i++)
            {
                var v1 = _array[i];
                var v2 = otherArray[i];
                var n = v1 & v2;
                if (n != 0)
                {
                    var bits1 = BitSetsIn16Bits[(int)(n & 0xffffu)];
                    var bits2 = BitSetsIn16Bits[(int)((n >> 16) & 0xffffu)];
                    var bits3 = BitSetsIn16Bits[(int)((n >> 32) & 0xffffu)];
                    var bits4 = BitSetsIn16Bits[(int)((n >> 48) & 0xffffu)];

                    result += bits1.Count + bits2.Count + bits3.Count + bits4.Count;
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

            var from = Math.Max(MinPositiveWord, other.MinPositiveWord);
            var to = Math.Min(MaxPositiveWord, other.MaxPositiveWord);

            var result = new List<int>();

            var otherArray = other._array;

            for (var i = from; i <= to; i++)
            {
                var v1 = _array[i];
                var v2 = otherArray[i];
                var n = v1 & v2;
                if (n != 0)
                {
                    var bits1 = BitSetsIn16Bits[(int)(n & 0xffffu)];
                    var bits2 = BitSetsIn16Bits[(int)((n >> 16) & 0xffffu)];
                    var bits3 = BitSetsIn16Bits[(int)((n >> 32) & 0xffffu)];
                    var bits4 = BitSetsIn16Bits[(int)((n >> 48) & 0xffffu)];

                    if (bits1.Count > 0)
                    {
                        result.Add(bits1[0] + i * 64);
                    }
                    else if (bits2.Count > 0)
                    {
                        result.Add(bits2[0] + 16 + i * 64);
                    }
                    else if (bits3.Count > 0)
                    {
                        result.Add(bits3[0] + 32 + i * 64);
                    }
                    else
                    {
                        result.Add(bits4[0] + 48 + i * 64);
                    }
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

            var from = Math.Max(MinPositiveWord, other.MinPositiveWord);
            var to = Math.Min(MaxPositiveWord, other.MaxPositiveWord);

            var otherArray = other._array;

            for (var i = from; i <= to; i++)
            {
                var v1 = _array[i];
                var v2 = otherArray[i];
                var n = v1 & v2;
                if (n != 0)
                {
                    var bits1 = BitSetsIn16Bits[(int)(n & 0xffffu)];
                    var bits2 = BitSetsIn16Bits[(int)((n >> 16) & 0xffffu)];
                    var bits3 = BitSetsIn16Bits[(int)((n >> 32) & 0xffffu)];
                    var bits4 = BitSetsIn16Bits[(int)((n >> 48) & 0xffffu)];

                    if (bits4.Count > 0)
                    {
                        return bits4[bits4.Count - 1] + 48 + i * 64;
                    }
                    else if (bits3.Count > 0)
                    {
                        return bits3[bits3.Count - 1] + 32 + i * 64;
                    }
                    else if (bits2.Count > 0)
                    {
                        return bits2[bits2.Count - 1] + 16 + i * 64;
                    }
                    else
                    {
                        return bits1[bits1.Count - 1] + i * 64;
                    }
                }
            }

            return -1;
        }        
    }
}