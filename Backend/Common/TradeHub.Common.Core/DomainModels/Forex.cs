

namespace TradeHub.Common.Core.DomainModels
{
    /// <summary>
    /// A foreign exchange currency.
    /// </summary>
    public class Forex : Security
    {
        // Indentifies Security as Forex
        public override string SecurityType  { get { return Constants.MarketData.SecurityTypes.Forex; } }
    }
}
