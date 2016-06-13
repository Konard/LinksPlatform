using System;

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

        static CachedTypeInfo()
        {
            Type = typeof(T);

            if (IsNullable = Type.IsNullable())
                UnderlyingType = Nullable.GetUnderlyingType(Type);
            else
                UnderlyingType = Type;

            if (UnderlyingType == typeof(Decimal) ||
                UnderlyingType == typeof(Double) ||
                UnderlyingType == typeof(Single))
                CanBeNumeric = IsNumeric = IsSigned = IsFloatPoint = true;

            if (UnderlyingType == typeof(SByte) ||
                UnderlyingType == typeof(Int16) ||
                UnderlyingType == typeof(Int32) ||
                UnderlyingType == typeof(Int64))
                CanBeNumeric = IsNumeric = IsSigned = true;

            if (UnderlyingType == typeof(Byte) ||
                UnderlyingType == typeof(UInt16) ||
                UnderlyingType == typeof(UInt32) ||
                UnderlyingType == typeof(UInt64))
                CanBeNumeric = IsNumeric = true;

            if (UnderlyingType == typeof(Boolean) ||
                UnderlyingType == typeof(Char) ||
                UnderlyingType == typeof(DateTime) ||
                UnderlyingType == typeof(TimeSpan))
                CanBeNumeric = true;


            if (IsSigned)
            {
                SignedVersion = UnderlyingType;
                UnsignedVersion = GetUnsignedFor(UnderlyingType);
            }
            else
            {
                SignedVersion = GetSignedFor(UnderlyingType);
                UnsignedVersion = UnderlyingType;
            }
        }

        private static Type GetUnsignedFor(Type signedType)
        {
            if (signedType == typeof(SByte))
                return typeof(Byte);
            if (signedType == typeof(Int16))
                return typeof(UInt16);
            if (signedType == typeof(Int32))
                return typeof(UInt32);
            if (signedType == typeof(Int64))
                return typeof(UInt64);
            return null;
        }

        private static Type GetSignedFor(Type unsignedType)
        {
            if (unsignedType == typeof(Byte))
                return typeof(SByte);
            if (unsignedType == typeof(UInt16))
                return typeof(Int16);
            if (unsignedType == typeof(UInt32))
                return typeof(Int32);
            if (unsignedType == typeof(UInt64))
                return typeof(Int64);
            return null;
        }
    }
}
