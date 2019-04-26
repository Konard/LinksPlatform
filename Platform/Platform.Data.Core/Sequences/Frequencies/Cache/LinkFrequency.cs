namespace Platform.Data.Core.Sequences
{
    public class FrequencyAndLink<TLink>
    {
        public TLink Frequency;
        public TLink Link;

        public FrequencyAndLink(TLink frequency, TLink link)
        {
            Frequency = frequency;
            Link = link;
        }

        public FrequencyAndLink()
        {
        }

        public override string ToString() => $"F: {Frequency}, L: {Link}";
    }
}
