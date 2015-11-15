using System;

// ReSharper disable StaticFieldInGenericType

namespace Platform.Helpers
{
    public class TypeHelpers<T>
    {
        public static readonly Type Cached = typeof(T);

        public static readonly bool IsNumeric;
        public static readonly bool CanBeNumeric;
        public static readonly bool IsSigned;

        public static bool IsValueType { get { return Cached.IsValueType; } }
        public static bool IsSignedNumeric { get { return IsNumeric && IsSigned; } }

        static TypeHelpers()
        {
            switch (Type.GetTypeCode(Cached))
            {
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    IsSigned = true;
                    IsNumeric = true;
                    CanBeNumeric = true;
                    break;
                case TypeCode.Byte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    IsNumeric = true;
                    CanBeNumeric = true;
                    break;
                case TypeCode.Boolean:
                case TypeCode.Char:
                case TypeCode.DateTime:
                    CanBeNumeric = true;
                    break;
            }
        }
    }
}
