using System;
using Platform.Data.Core.Pairs;

namespace Platform.Data.Core.Sequences
{
    public struct SequencesOptions // TODO: To use type parameter <TLink> the ILinks<TLink> must contain GetConstants function.
    {
        public ulong SequenceMarkerLink;
        public bool UseCascadeUpdate;
        public bool UseCascadeDelete;
        public bool UseSequenceMarker;
        public bool UseCompression;
        public bool UseGarbageCollection;
        public bool EnforceSingleSequenceVersionOnWrite;
        // TODO: Реализовать компактификацию при чтении
        //public bool EnforceSingleSequenceVersionOnRead; 
        //public bool UseRequestMarker;
        //public bool StoreRequestResults;

        public void InitOptions(ILinks<ulong> links)
        {
            if (UseSequenceMarker && SequenceMarkerLink == LinksConstants.Null)
                SequenceMarkerLink = links.Create(LinksConstants.Itself, LinksConstants.Itself);
        }

        public void ValidateOptions()
        {
            if (UseGarbageCollection && !UseSequenceMarker)
                throw new NotSupportedException("To use garbage collection UseSequenceMarker option must be on.");
        }
    }
}
