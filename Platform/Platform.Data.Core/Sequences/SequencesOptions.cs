using System;
using System.Collections.Generic;
using Platform.Helpers;
using Platform.Data.Core.Doublets;
using Platform.Data.Core.Sequences.FrequencyCounters;

namespace Platform.Data.Core.Sequences
{
    public struct SequencesOptions<TLink> // TODO: To use type parameter <TLink> the ILinks<TLink> must contain GetConstants function.
    {
        private static readonly LinksConstants<bool, ulong, long> Constants = Default<LinksConstants<bool, ulong, long>>.Instance;

        public TLink SequenceMarkerLink;
        public bool UseCascadeUpdate;
        public bool UseCascadeDelete;
        public bool UseIndex; // TODO: Update Index on sequence update/delete.
        public bool UseSequenceMarker;
        public bool UseCompression;
        public bool UseGarbageCollection;
        public bool EnforceSingleSequenceVersionOnWriteBasedOnExisting;
        public bool EnforceSingleSequenceVersionOnWriteBasedOnNew;

        public MarkedSequenceMatcher<TLink> MarkedSequenceMatcher;
        public IConverter<IList<TLink>, TLink> LinksToSequenceConverter;
        public SequencesIndexer<TLink> Indexer;

        // TODO: Реализовать компактификацию при чтении
        //public bool EnforceSingleSequenceVersionOnRead; 
        //public bool UseRequestMarker;
        //public bool StoreRequestResults;

        public void InitOptions(ISynchronizedLinks<TLink> links)
        {
            if (UseSequenceMarker)
            {
                if (Equals(SequenceMarkerLink, Constants.Null))
                    SequenceMarkerLink = links.CreatePoint();
                else
                {
                    if (!links.Exists(SequenceMarkerLink))
                    {
                        var link = links.CreatePoint();
                        if (!Equals(link, SequenceMarkerLink))
                            throw new Exception("Cannot recreate sequence marker link.");
                    }
                }

                if (MarkedSequenceMatcher == null)
                    MarkedSequenceMatcher = new MarkedSequenceMatcher<TLink>(links, SequenceMarkerLink);
            }

            var balancedVariantConverter = new BalancedVariantConverter<TLink>(links);

            if (UseCompression)
            {
                if (LinksToSequenceConverter == null)
                {
                    ICounter<TLink, TLink> totalSequenceSymbolFrequencyCounter;
                    if (UseSequenceMarker)
                        totalSequenceSymbolFrequencyCounter = new TotalMarkedSequenceSymbolFrequencyCounter<TLink>(links, MarkedSequenceMatcher);
                    else
                        totalSequenceSymbolFrequencyCounter = new TotalSequenceSymbolFrequencyCounter<TLink>(links);

                    var doubletFrequenciesCache = new DoubletFrequenciesCache<TLink>(links, totalSequenceSymbolFrequencyCounter);

                    var compressingConverter = new CompressingConverter<TLink>(links, balancedVariantConverter, doubletFrequenciesCache);
                    LinksToSequenceConverter = compressingConverter;
                }
            }
            else
            {
                if (LinksToSequenceConverter == null)
                    LinksToSequenceConverter = balancedVariantConverter;
            }

            if (UseIndex && Indexer == null)
                Indexer = new SequencesIndexer<TLink>(links);
        }

        public void ValidateOptions()
        {
            if (UseGarbageCollection && !UseSequenceMarker)
                throw new NotSupportedException("To use garbage collection UseSequenceMarker option must be on.");
        }
    }
}
