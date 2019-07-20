using System.Collections.Generic;
using Platform.Interfaces;
using Platform.Reflection;
using Platform.Numbers;

namespace Platform.Data.Core.Doublets
{
    public class AddressToUnaryNumberConverter<TLink> : LinksOperatorBase<TLink>, IConverter<TLink>
    {
        private static readonly EqualityComparer<TLink> EqualityComparer = EqualityComparer<TLink>.Default;

        private readonly IConverter<int, TLink> _powerOf2ToUnaryNumberConverter;

        public AddressToUnaryNumberConverter(ILinks<TLink> links, IConverter<int, TLink> powerOf2ToUnaryNumberConverter) : base(links)
        {
            _powerOf2ToUnaryNumberConverter = powerOf2ToUnaryNumberConverter;
        }

        public TLink Convert(TLink sourceAddress)
        {
            var number = sourceAddress;
            var target = Links.Constants.Null;
            for (int i = 0; i < CachedTypeInfo<TLink>.BitsLength; i++)
            {
                if (EqualityComparer.Equals(ArithmeticHelpers.And(number, Integer<TLink>.One), Integer<TLink>.One))
                {
                    if (EqualityComparer.Equals(target, Links.Constants.Null))
                        target = _powerOf2ToUnaryNumberConverter.Convert(i);
                    else
                        target = Links.GetOrCreate(_powerOf2ToUnaryNumberConverter.Convert(i), target);
                }
                number = (Integer<TLink>)(((ulong)(Integer<TLink>)number) >> 1); // MathHelpers.ShiftRight(number, 1);
                if (EqualityComparer.Equals(number, default))
                    break;
            }
            return target;
        }
    }
}
