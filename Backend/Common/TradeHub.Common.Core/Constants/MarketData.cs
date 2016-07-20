
namespace TradeHub.Common.Core.Constants
{
    public static class MarketData
    {
        public static class MarketDataRequest
        {
            public const int Subscribe = 1;
            public const int Unsubscribe = 2;
            public const int Historic = 3;
            public const int BarData = 4;
        }

        public static class SecurityTypes
        {
            public const string Base = "NONE";
            public const string Futures = "FUTURES";
            public const string Forex = "FOREX";
            public const string ForexFuture = "FOREXFUTURE";
            public const string Option = "OPTION";
            public const string Stock = "STOCK";
        }
    }
}