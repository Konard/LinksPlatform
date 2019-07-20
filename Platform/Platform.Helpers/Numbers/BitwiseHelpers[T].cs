using System;
using Platform.Reflection;
using Platform.Reflection.Sigil;

// ReSharper disable StaticFieldInGenericType

namespace Platform.Helpers.Numbers
{
    public static class BitwiseHelpers<T>
    {
        public static readonly Func<T, T, int, int, T> PartialWrite;
        public static readonly Func<T, int, int, T> PartialRead;

        static BitwiseHelpers()
        {
            DelegateHelpers.Compile(out PartialWrite, emiter =>
            {
                if (!CachedTypeInfo<T>.IsNumeric)
                    throw new NotSupportedException();

                var constants = GetConstants<T>();
                var bitsNumber = constants.Item1;
                var numberFilledWithOnes = constants.Item2;

                ushort shiftArgument = 2;
                ushort limitArgument = 3;

                var checkLimit = emiter.DefineLabel();
                var calculateSourceMask = emiter.DefineLabel();

                // Check shift
                emiter.LoadArgument(shiftArgument);
                emiter.LoadConstant(0);
                emiter.BranchIfGreaterOrEqual(checkLimit); // Skip fix

                // Fix shift
                emiter.LoadConstant(bitsNumber);
                emiter.LoadArgument(shiftArgument);
                emiter.Add();
                emiter.StoreArgument(shiftArgument);

                emiter.MarkLabel(checkLimit);
                // Check limit
                emiter.LoadArgument(limitArgument);
                emiter.LoadConstant(0);
                emiter.BranchIfGreaterOrEqual(calculateSourceMask); // Skip fix

                // Fix limit
                emiter.LoadConstant(bitsNumber);
                emiter.LoadArgument(limitArgument);
                emiter.Add();
                emiter.StoreArgument(limitArgument);

                emiter.MarkLabel(calculateSourceMask);

                using (var sourceMask = emiter.DeclareLocal<T>())
                using (var targetMask = emiter.DeclareLocal<T>())
                {
                    emiter.LoadConstant(typeof(T), numberFilledWithOnes);
                    emiter.LoadArgument(limitArgument);
                    emiter.ShiftLeft();
                    emiter.Not();
                    emiter.LoadConstant(typeof(T), numberFilledWithOnes);
                    emiter.And();
                    emiter.StoreLocal(sourceMask);

                    emiter.LoadLocal(sourceMask);
                    emiter.LoadArgument(shiftArgument);
                    emiter.ShiftLeft();
                    emiter.Not();
                    emiter.StoreLocal(targetMask);

                    emiter.LoadArgument(0); // target
                    emiter.LoadLocal(targetMask);
                    emiter.And();
                    emiter.LoadArgument(1); // source
                    emiter.LoadLocal(sourceMask);
                    emiter.And();
                    emiter.LoadArgument(shiftArgument);
                    emiter.ShiftLeft();
                    emiter.Or();
                }

                emiter.Return();
            });

            DelegateHelpers.Compile(out PartialRead, emiter =>
            {
                if (!CachedTypeInfo<T>.IsNumeric)
                    throw new NotSupportedException();

                var constants = GetConstants<T>();
                var bitsNumber = constants.Item1;
                var numberFilledWithOnes = constants.Item2;

                ushort shiftArgument = 1;
                ushort limitArgument = 2;

                var checkLimit = emiter.DefineLabel();
                var calculateSourceMask = emiter.DefineLabel();

                // Check shift
                emiter.LoadArgument(shiftArgument);
                emiter.LoadConstant(0);
                emiter.BranchIfGreaterOrEqual(checkLimit); // Skip fix

                // Fix shift
                emiter.LoadConstant(bitsNumber);
                emiter.LoadArgument(shiftArgument);
                emiter.Add();
                emiter.StoreArgument(shiftArgument);

                emiter.MarkLabel(checkLimit);
                // Check limit
                emiter.LoadArgument(limitArgument);
                emiter.LoadConstant(0);
                emiter.BranchIfGreaterOrEqual(calculateSourceMask); // Skip fix

                // Fix limit
                emiter.LoadConstant(bitsNumber);
                emiter.LoadArgument(limitArgument);
                emiter.Add();
                emiter.StoreArgument(limitArgument);

                emiter.MarkLabel(calculateSourceMask);

                using (var sourceMask = emiter.DeclareLocal<T>())
                using (var targetMask = emiter.DeclareLocal<T>())
                {
                    emiter.LoadConstant(typeof(T), numberFilledWithOnes);
                    emiter.LoadArgument(limitArgument); // limit
                    emiter.ShiftLeft();
                    emiter.Not();
                    emiter.LoadConstant(typeof(T), numberFilledWithOnes);
                    emiter.And();
                    emiter.StoreLocal(sourceMask);

                    emiter.LoadLocal(sourceMask);
                    emiter.LoadArgument(shiftArgument);
                    emiter.ShiftLeft();
                    emiter.StoreLocal(targetMask);

                    emiter.LoadArgument(0); // target
                    emiter.LoadLocal(targetMask);
                    emiter.And();
                    emiter.LoadArgument(shiftArgument);
                    emiter.ShiftRight();
                }

                emiter.Return();
            });
        }

        private static Tuple<int, TElement> GetConstants<TElement>()
        {
            var type = typeof(T);
            if (type == typeof(ulong))
                return new Tuple<int, TElement>(64, (TElement)(object)ulong.MaxValue);
            if (type == typeof(uint))
                return new Tuple<int, TElement>(32, (TElement)(object)uint.MaxValue);
            if (type == typeof(ushort))
                return new Tuple<int, TElement>(16, (TElement)(object)ushort.MaxValue);
            if (type == typeof(byte))
                return new Tuple<int, TElement>(8, (TElement)(object)byte.MaxValue);
            throw new NotSupportedException();
        }
    }
}
