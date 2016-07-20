namespace TradeHub.Common.Core.ValueObjects.Inquiry
{
    /// <summary>
    /// Class provides info for the requesting query
    /// </summary>
    public class InquiryMessage
    {
        private string _type = string.Empty;
        private string _marketDataProvider = string.Empty;
        private string _orderExecutionProvider = string.Empty;

        /// <summary>
        /// The Type of requesting Query
        /// </summary>
        public string Type
        {
            get { return _type; }
            set { _type = value; }
        }

        /// <summary>
        /// Name of the Market Data Provider for which the query is intended
        /// </summary>
        public string MarketDataProvider
        {
            get { return _marketDataProvider; }
            set { _marketDataProvider = value; }
        }

        /// <summary>
        /// Name of the Order Execution Provider for which the query is intended
        /// </summary>
        public string OrderExecutionProvider
        {
            get { return _orderExecutionProvider; }
            set { _orderExecutionProvider = value; }
        }
    }
}
