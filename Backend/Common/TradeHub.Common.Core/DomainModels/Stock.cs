

namespace TradeHub.Common.Core.DomainModels
{
    /// <summary>
    /// A single stock.
    /// </summary>
    public class Stock : Security
    {
        // Indentifies Security as Stock
        public override string SecurityType { get { return Constants.MarketData.SecurityTypes.Stock; } }
    }
}
