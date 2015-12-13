namespace Platform.Data.Core.Sequences
{
    public struct SequencesOptions<TLink>
    {
        public TLink SequenceMarkerLink;
        public bool UseCompression;
        public bool UseGarbageCollection;
        public bool EnforceSingleSequenceVersionOnWrite;
    }
}
