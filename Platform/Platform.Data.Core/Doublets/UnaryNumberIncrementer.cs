using Platform.Interfaces;
using Platform.Helpers.Numbers;

namespace Platform.Data.Core.Doublets
{
    public class UnaryNumberIncrementer<TLink> : LinksOperatorBase<TLink>, IIncrementer<TLink>
    {
        private readonly TLink _unaryOne;

        public UnaryNumberIncrementer(ILinks<TLink> links, TLink unaryOne)
            : base(links)
            => _unaryOne = unaryOne;

        public TLink Increment(TLink unaryNumber)
        {
            if (MathHelpers<TLink>.IsEquals(unaryNumber, _unaryOne))
                return Links.GetOrCreate(_unaryOne, _unaryOne);

            var source = Links.GetSource(unaryNumber);
            var target = Links.GetTarget(unaryNumber);

            if (MathHelpers<TLink>.IsEquals(source, target))
                return Links.GetOrCreate(unaryNumber, _unaryOne);
            else
                return Links.GetOrCreate(source, Increment(target));
        }
    }
}
