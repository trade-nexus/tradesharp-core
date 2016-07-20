

namespace TradeHub.Common.Core.DomainModels
{
    /// <summary>
    /// A foreign exchange future.
    /// </summary>
    public class ForexFuture : Future
    {
        // Indentifies Security as ForexFuture
        public override string SecurityType{ get { return Constants.MarketData.SecurityTypes.ForexFuture; } }
    }
}
