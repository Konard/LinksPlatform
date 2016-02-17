using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Platform.Data.Core.Pairs;
using Platform.Helpers.Collections;

namespace Platform.Data.Core.Sequences
{
    /// <remarks>
    /// TODO: Возможно будет лучше если алгоритм будет выполняться полностью изолированно от Links на этапе сжатия.
    ///     А именно будет создаваться временный список пар необходимых для выполнения сжатия, в таком случае тип значения элемента массива может быть любым, как char так и ulong.
    ///     Как только список/словарь пар был выявлен можно разом выполнить создание всех этих пар, а так же разом выполнить замену.
    /// </remarks>
    public class Compressor
    {
        private readonly Func<ulong, ulong, ulong> _createLink;
        private readonly Func<ulong[], ulong> _createSequence; 
        private Link _maxPair;
        private readonly ulong _minFrequency;
        private ulong _maxFrequency;
        private UnsafeDictionary<Link, ulong> _pairsFrequencies;

        /// <remarks>
        /// TODO: Может стоит попробовать ref во всех методах
        /// </remarks>
        public class LinkComparer : IEqualityComparer<Link>
        {
            public static readonly LinkComparer Default = new LinkComparer();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool Equals(Link x, Link y)
            {
                return x.Source == y.Source &&
                       x.Target == y.Target;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int GetHashCode(Link obj)
            {
                unchecked // Overflow is fine, just wrap
                {
                    return (int)((obj.Source << 16) ^ obj.Target);

                    //return (int)(obj.Source * 31 ^ obj.Target);

                    //return (int)(obj.Source * 31 + obj.Target); // or 33

                    //return (int)((obj.Source * 17) ^ obj.Target);

                    //return (int)((527 + obj.Source) * 31 + obj.Target);

                    //return (int)((629 + obj.Source) * 37 + obj.Target);

                    //return (int)((391 + obj.Source) * 23 + obj.Target);

                    //return (int)(obj.Source + obj.Target);

                    //ulong hash = 17;
                    //hash = hash * 31 + obj.Source;
                    //hash = hash * 31 + obj.Target;
                    //return (int)hash;

                    //var hash = 17;
                    //hash = hash * 31 + (int)(obj.Source ^ (obj.Source >> 32));
                    //hash = hash * 31 + (int)(obj.Target ^ (obj.Target >> 32));
                    //return hash;

                    //var hash = 17;
                    //hash = hash * 23 + obj.Source.GetHashCode();
                    //hash = hash * 23 + obj.Target.GetHashCode();
                    //return hash;

                    //return new { obj.Source, obj.Target }.GetHashCode();

                    //return 31 * obj.Source.GetHashCode() + obj.Target.GetHashCode();

                    //return obj.Source.GetHashCode() ^ obj.Target.GetHashCode();

                    //return (int)((obj.Source << 5) + 3 + obj.Source ^ obj.Target);

                    //return (int) (obj.Source ^ obj.Target);
                }
            }
        }

        public Compressor(Links links, Sequences sequences, ulong minFrequency = 1, bool threadSafe = true)
        {
            if (threadSafe)
            {
                _createLink = links.Create;
                _createSequence = sequences.CreateBalancedVariant;
            }
            else
            {
                _createLink = links.CreateCore;
                _createSequence = sequences.CreateBalancedVariantCore;
            }

            if (minFrequency == 0) minFrequency = 1;
            _minFrequency = minFrequency;
            ResetMaxPair();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ulong IncrementFrequency(Link pair)
        {
            ulong frequency;
            if (_pairsFrequencies.TryGetValue(pair, out frequency))
            {
                frequency++;
                _pairsFrequencies[pair] = frequency;
            }
            else
            {
                frequency = 1;
                _pairsFrequencies.Add(pair, frequency);
            }
            return frequency;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void DecrementFrequency(Link pair)
        {
            ulong frequency;
            if (_pairsFrequencies.TryGetValue(pair, out frequency))
            {
                frequency--;

                if (frequency == 0)
                    _pairsFrequencies.Remove(pair);
                else
                    _pairsFrequencies[pair] = frequency;
            }
            //return frequency;
        }

        /// <remarks>
        /// Original algorithm idea: https://en.wikipedia.org/wiki/Byte_pair_encoding .
        /// Faster version (pairs' frequencies dictionary is not recreated).
        /// </remarks>
        public ulong[] Precompress(ulong[] sequence)
        {
            if (sequence.IsNullOrEmpty())
                return null;

            if (sequence.Length == 1)
                return sequence;

            if (sequence.Length == 2)
                return new[] { _createLink(sequence[0], sequence[1]) };

            if (_pairsFrequencies != null)
                throw new InvalidOperationException("Only one sequence at a time can be precompresed using single compressor.");

            _pairsFrequencies = new UnsafeDictionary<Link, ulong>(4096, LinkComparer.Default);

            for (var i = 1; i < sequence.Length; i++)
            {
                var pair = new Link(sequence[i - 1], sequence[i]);
                UpdateMaxPair(pair, IncrementFrequency(pair));
            }

            if (_maxFrequency > _minFrequency)
            {
                // Can be faster if source sequence allowed to be changed
                var copy = new ulong[sequence.Length];
                Array.Copy(sequence, copy, sequence.Length);

                var newLength = ReplacePairs(copy);

                _pairsFrequencies = null;

                var final = new ulong[newLength];
                Array.Copy(copy, final, newLength);

                return final;
            }

            _pairsFrequencies = null;

            return sequence;
        }

        /// <remarks>
        /// Original algorithm idea: https://en.wikipedia.org/wiki/Byte_pair_encoding .
        /// Faster version (pairs' frequencies dictionary is not recreated).
        /// </remarks>
        private int ReplacePairs(ulong[] copy)
        {
            var oldLength = copy.Length;
            var newLength = copy.Length;

            while (!_maxPair.IsNull())
            {
                var maxPairSource = _maxPair.Source;
                var maxPairTarget = _maxPair.Target;
                var maxPairResult = _createLink(maxPairSource, maxPairTarget);

                oldLength--;
                var oldLengthMinusTwo = oldLength - 1;

                // Substitute all usages
                int w = 0, r = 0; // (r == read, w == write)
                for (; r < oldLength; r++)
                {
                    if (copy[r] == maxPairSource && copy[r + 1] == maxPairTarget)
                    {
                        if (r > 0)
                        {
                            var previous = copy[w - 1];
                            DecrementFrequency(new Link(previous, maxPairSource));
                            IncrementFrequency(new Link(previous, maxPairResult));
                        }
                        if (r < oldLengthMinusTwo)
                        {
                            var next = copy[r + 2];
                            DecrementFrequency(new Link(maxPairTarget, next));
                            IncrementFrequency(new Link(maxPairResult, next));
                        }

                        copy[w++] = maxPairResult;
                        r++;
                        newLength--;
                    }
                    else
                    {
                        copy[w++] = copy[r];
                    }
                }
                if (w < newLength) copy[w] = copy[r];

                _pairsFrequencies.Remove(_maxPair);

                oldLength = newLength;

                // Быстрее
                UpdateMaxPair();
                //ParallelUpdateMaxPair();
            }

            return newLength;
        }

        public ulong Compress(ulong[] sequence)
        {
            var precompressedSequence = Precompress(sequence);
            return _createSequence(precompressedSequence);
        }

        private void ResetMaxPair()
        {
            _maxPair = Link.Null;
            _maxFrequency = 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UpdateMaxPair(Link pair, ulong frequency)
        {
            if (frequency > _minFrequency)
            {
                if (_maxFrequency < frequency)
                {
                    _maxFrequency = frequency;
                    _maxPair = pair;
                }
                else if (_maxFrequency == frequency &&
                    (pair.Source + pair.Target) > (_maxPair.Source + _maxPair.Target))
                {
                    _maxPair = pair;
                }
            }
        }

        private void UpdateMaxPair()
        {
            ResetMaxPair();

            var entries = _pairsFrequencies.entries;

            //for (int i = 0; i < entries.Length; i++)
            for (var i = entries.Length - 1; i >= 0; --i)
            {
                //var frequency = entries[i].value;
                if (entries[i].hashCode >= 0 && entries[i].value > _minFrequency)
                {
                    if (_maxFrequency < entries[i].value)
                    {
                        _maxFrequency = entries[i].value;
                        _maxPair = entries[i].key;
                    }
                    else if (_maxFrequency == entries[i].value &&
                             (entries[i].key.Source + entries[i].key.Target) > (_maxPair.Source + _maxPair.Target))
                    {
                        _maxPair = entries[i].key;
                    }
                }
            }
        }

        // Замедляет выполнение
        //private void ParallelUpdateMaxPair()
        //{
        //    ResetMaxPair();

        //    var entries = _pairsFrequencies.entries;

        //    Parallel.For(0, entries.Length, (i, state) =>
        //    {
        //        if (entries[i].hashCode >= 0)
        //        {
        //            var frequency = (long)entries[i].value;
        //            if (frequency > 1)
        //            {
        //                if (_maxFrequency < frequency)
        //                {
        //                    _maxPair = entries[i].key;
        //                    Interlocked.Exchange(ref _maxFrequency, frequency);
        //                }
        //                //else if (_maxFrequency == frequency &&
        //                //            (entries[i].key.Source + entries[i].key.Target) >
        //                //            (_maxPair.Source + _maxPair.Target))
        //                //{
        //                //    _maxPair = entries[i].key;
        //                //}
        //            }
        //        }
        //    });
        //}
    }
}
