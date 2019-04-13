using Platform.Helpers;
using Platform.Helpers.Reflection;

namespace Platform.Data.Core.Doublets
{
    public class AddressToUnaryNumberConverter<TLink> : LinksOperatorBase<TLink>, IConverter<TLink, TLink>
    {
        private static readonly TLink One = MathHelpers.Increment(default(TLink));

        private readonly IConverter<int, TLink> _powerOf2ToUnaryNumberConverter;

        public AddressToUnaryNumberConverter(ILinks<TLink> links, IConverter<int, TLink> powerOf2ToUnaryNumberConverter) : base(links)
        {
            _powerOf2ToUnaryNumberConverter = powerOf2ToUnaryNumberConverter;
        }

        public TLink Convert(TLink sourceAddress)
        {
            TLink number = sourceAddress;
            TLink target = Links.Constants.Null;
            for (int i = 0; i < CachedTypeInfo<TLink>.BitsLength; i++)
            {
                if (Equals(MathHelpers.Add(number, One), One))
                {
                    if (Equals(target, Links.Constants.Null))
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
