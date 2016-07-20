

namespace TradeHub.Common.Core.DomainModels
{
    /// <summary>
    /// A tradeable option.
    /// </summary>
    public class Option : Security
    {
        // Indentifies Security as Option
        public override string SecurityType { get { return Constants.MarketData.SecurityTypes.Option; } }
    }
}
