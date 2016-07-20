
using TradeHub.Common.Core.ValueObjects.MarketData;

namespace TradeHub.DataDownloader.UserInterface.Common.Messages
{
    public class UnsubscribeBars
    {
        public BarDataRequest UnSubscribeBarDataRequest { get; set; }
    }
}
