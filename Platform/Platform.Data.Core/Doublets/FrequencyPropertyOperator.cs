using Platform.Helpers;

namespace Platform.Data.Core.Doublets
{
    public class FrequencyPropertyOperator<TLink> : LinksOperatorBase<TLink>, ISpecificPropertyOperator<TLink, TLink>
    {
        private readonly TLink _frequencyPropertyMarker;
        private readonly TLink _frequencyMarker;

        public FrequencyPropertyOperator(ILinks<TLink> links, TLink frequencyPropertyMarker, TLink frequencyMarker) : base(links)
        {
            _frequencyPropertyMarker = frequencyPropertyMarker;
            _frequencyMarker = frequencyMarker;
        }

        public TLink GetValue(TLink link)
        {
            var property = Links.SearchOrDefault(link, _frequencyPropertyMarker);
            var container = GetContainer(property);
            var frequency = GetFrequency(container);
            return frequency;
        }

        private TLink GetContainer(TLink property)
        {
            var frequencyContainer = default(TLink);

            if (Equals(property, default(TLink)))
                return frequencyContainer;

            Links.Each(candidate =>
            {
                var candidateTarget = Links.GetTarget(candidate);
                var frequencyTarget = Links.GetTarget(candidateTarget);

                if (Equals(frequencyTarget, _frequencyMarker))
                {
                    frequencyContainer = Links.GetIndex(candidate);
                    return Links.Constants.Break;
                }

                return Links.Constants.Continue;
            }, Links.Constants.Any, property, Links.Constants.Any);

            return frequencyContainer;
        }

        private TLink GetFrequency(TLink container) => Equals(container, default(TLink)) ? default : Links.GetTarget(container);

        public void SetValue(TLink link, TLink frequency)
        {
            var property = Links.GetOrCreate(link, _frequencyPropertyMarker);
            var container = GetContainer(property);
            if (Equals(container, default(TLink)))
                Links.GetOrCreate(property, frequency);
            else
                Links.Update(container, property, frequency);
        }
    }
}
