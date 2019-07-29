namespace Platform.Data.Constants
{
    public interface ILinksCombinedConstants<TDecision, TAddress, TPartIndex, TLinksConstants> :
        ILinksDecisionConstants<TDecision>,
        ILinksAddressConstants<TAddress>,
        ILinksPartConstants<TPartIndex>
        where TLinksConstants : ILinksDecisionConstants<TDecision>, ILinksAddressConstants<TAddress>, ILinksPartConstants<TPartIndex>
    {
    }
}
