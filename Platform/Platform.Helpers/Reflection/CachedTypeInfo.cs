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
        }
    }
}
