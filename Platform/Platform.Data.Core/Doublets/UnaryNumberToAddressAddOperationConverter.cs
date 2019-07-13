using System.Collections.Generic;
using Platform.Interfaces;
using Platform.Helpers.Numbers;

namespace Platform.Data.Core.Doublets
{
    public class UnaryNumberToAddressAddOperationConverter<TLink> : LinksOperatorBase<TLink>, IConverter<TLink>
    {
        private Dictionary<TLink, TLink> _unaryToUInt64;
        private readonly TLink _unaryOne;

        public UnaryNumberToAddressAddOperationConverter(ILinks<TLink> links, TLink unaryOne)
            : base(links)
        {
            _unaryOne = unaryOne;
            InitUnaryToUInt64();
        }

        private void InitUnaryToUInt64()
        {
            _unaryToUInt64 = new Dictionary<TLink, TLink>();
            _unaryToUInt64.Add(_unaryOne, Integer<TLink>.One);

            var unary = _unaryOne;
            var number = Integer<TLink>.One;

            for (var i = 1; i < 64; i++)
                _unaryToUInt64.Add(unary = Links.GetOrCreate(unary, unary), number = (Integer<TLink>)(((ulong)(Integer<TLink>)number) * 2UL));
        }

        public TLink Convert(TLink unaryNumber)
        {
            if (MathHelpers<TLink>.IsEquals(unaryNumber, default))
                return default;
            if (MathHelpers<TLink>.IsEquals(unaryNumber, _unaryOne))
                return Integer<TLink>.One;

            var source = Links.GetSource(unaryNumber);
            var target = Links.GetTarget(unaryNumber);

            if (MathHelpers<TLink>.IsEquals(source, target))
                return _unaryToUInt64[unaryNumber];
            else
            {
                var result = _unaryToUInt64[source];
                TLink lastValue;
                while (!_unaryToUInt64.TryGetValue(target, out lastValue))
                {
                    source = Links.GetSource(target);
                    result = MathHelpers.Add(result, _unaryToUInt64[source]);
                    target = Links.GetTarget(target);
                }
                result = MathHelpers.Add(result, lastValue);
                return result;
            }
        }
    }
}
