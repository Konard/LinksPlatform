using System;
using System.Runtime.InteropServices;

// ReSharper disable AssignmentInConditionalExpression

// ReSharper disable BuiltInTypeReferenceStyle
// ReSharper disable StaticFieldInGenericType

namespace Platform.Helpers.Reflection
{
    public class CachedTypeInfo<T>
    {
        public static readonly Type Type;
        public static readonly Type UnderlyingType;

        public static readonly Type SignedVersion;
        public static readonly Type UnsignedVersion;

        public static readonly bool IsFloatPoint;
        public static readonly bool IsNumeric;
        public static readonly bool IsSigned;
        public static readonly bool CanBeNumeric;

        public static readonly bool IsNullable;

        public static readonly int BitsLength;
        public static readonly T MinValue;
        public static readonly T MaxValue;

        static CachedTypeInfo()
        {
            Type = typeof(T);

            if (IsNullable = Type.IsNullable()) //-V3055
                UnderlyingType = Nullable.GetUnderlyingType(Type);
            else
                UnderlyingType = Type;

            if (UnderlyingType == typeof(Decimal) ||
                UnderlyingType == typeof(Double) ||
                UnderlyingType == typeof(Single))
                CanBeNumeric = IsNumeric = IsSigned = IsFloatPoint = true;
            else if (UnderlyingType == typeof(SByte) ||
                UnderlyingType == typeof(Int16) ||
                UnderlyingType == typeof(Int32) ||
                UnderlyingType == typeof(Int64))
                CanBeNumeric = IsNumeric = IsSigned = true;
            else if (UnderlyingType == typeof(Byte) ||
                UnderlyingType == typeof(UInt16) ||
                UnderlyingType == typeof(UInt32) ||
                UnderlyingType == typeof(UInt64))
                CanBeNumeric = IsNumeric = true;
            else if (UnderlyingType == typeof(Boolean) ||
                UnderlyingType == typeof(Char) ||
                UnderlyingType == typeof(DateTime) ||
                UnderlyingType == typeof(TimeSpan))
                CanBeNumeric = true;

            BitsLength = Marshal.SizeOf((object)UnderlyingType) * 8;

            if (UnderlyingType == typeof(Boolean))
            {
                MinValue = (T)(object)false;
                MaxValue = (T)(object)true;
            }
            else
            {
                MinValue = UnderlyingType.GetStaticFieldValue<T>("MinValue");
                MaxValue = UnderlyingType.GetStaticFieldValue<T>("MaxValue");
            }

            if (IsSigned)
            {
                SignedVersion = UnderlyingType;
                UnsignedVersion = UnderlyingType.GetUnsignedVersionOrNull();
            }
            else
            {
                SignedVersion = UnderlyingType.GetSignedVersionOrNull();
                UnsignedVersion = UnderlyingType;
            }
        }
    }
}
