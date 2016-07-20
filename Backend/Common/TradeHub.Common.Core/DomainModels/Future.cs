

namespace TradeHub.Common.Core.DomainModels
{
    /// <summary>
    /// A tradeable future.
    /// </summary>
    public class Future : Security
    {
        // Indentifies Security as Future
        public override string SecurityType { get { return Constants.MarketData.SecurityTypes.Forex; } }
    }
}
