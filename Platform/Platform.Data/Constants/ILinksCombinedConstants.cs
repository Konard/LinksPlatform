namespace Platform.Data.Constants
{
    public interface ILinksCombinedConstants<TDecision, TAddress, TPartIndex> :
        ILinksDecisionConstants<TDecision>,
        ILinksAddressConstants<TAddress>,
        ILinksPartConstants<TPartIndex>
    {
    }
}
