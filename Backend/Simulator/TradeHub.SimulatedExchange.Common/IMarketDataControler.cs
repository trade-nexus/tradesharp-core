using System;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.ValueObjects.MarketData;

namespace TradeHub.SimulatedExchange.Common
{
    public interface IMarketDataControler
    {
        event Action<Bar, string> BarArrived;
        bool ConnectionStatus { get; set; }
        bool Connect();
        bool Disconnect();
        void SubscribeSymbol(BarDataRequest request);
        void SubscribeSymbol(Subscribe request);
    }
}