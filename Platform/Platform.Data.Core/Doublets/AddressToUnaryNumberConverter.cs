using Platform.Helpers;
using Platform.Helpers.Reflection;

namespace Platform.Data.Core.Doublets
{
    public class AddressToUnaryNumberConverter<TLink> : LinksOperatorBase<TLink>, IConverter<TLink>
    {
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
                if (MathHelpers<TLink>.IsEquals(number, default)) //if (Equals(MathHelpers.Add(number, Integer<TLink>.One), Integer<TLink>.One))
                {
                    if (MathHelpers<TLink>.IsEquals(target, Links.Constants.Null))
                        target = _powerOf2ToUnaryNumberConverter.Convert(i);
                    else
                        target = Links.GetOrCreate(_powerOf2ToUnaryNumberConverter.Convert(i), target);
                }
                number = (Integer<TLink>)(((ulong)(Integer<TLink>)number) >> 1); // MathHelpers.ShiftRight(number, 1);
            }
            return target;
        }
    }
}
