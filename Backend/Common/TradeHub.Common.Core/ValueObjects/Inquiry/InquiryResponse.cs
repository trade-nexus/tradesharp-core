using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeHub.Common.Core.ValueObjects.Inquiry
{
    /// <summary>
    /// Contains the information in response to the <see cref="InquiryMessage"/>
    /// </summary>
    public class InquiryResponse
    {
        private string _type = string.Empty;
        private string _marketDataProvider = string.Empty;
        private string _orderExecutionProvider = string.Empty;
        private string _appId = string.Empty;
        private List<Type> _marketDataProviderInfo = new List<Type>();
        private List<Type> _orderExecutionProviderInfo = new List<Type>();

        /// <summary>
        /// The Type of requesting Query
        /// </summary>
        public string Type
        {
            get { return _type; }
            set { _type = value; }
        }

        /// <summary>
        /// Name of the Market Data Provider for which the query response is intended
        /// </summary>
        public string MarketDataProvider
        {
            get { return _marketDataProvider; }
            set { _marketDataProvider = value; }
        }

        /// <summary>
        /// Name of the Order Execution Provider for which the query response is intended
        /// </summary>
        public string OrderExecutionProvider
        {
            get { return _orderExecutionProvider; }
            set { _orderExecutionProvider = value; }
        }

        /// <summary>
        /// Newly generated Unique App ID
        /// </summary>
        public string AppId
        {
            get { return _appId; }
            set { _appId = value; }
        }

        /// <summary>
        /// Contains the list of Types which the requested Market Data Provider supports
        /// </summary>
        public List<Type> MarketDataProviderInfo
        {
            get { return _marketDataProviderInfo; }
            set { _marketDataProviderInfo = value; }
        }

        /// <summary>
        /// Contains the list of Types which the requested Order Execution Provider supports
        /// </summary>
        public List<Type> OrderExecutionProviderInfo
        {
            get { return _orderExecutionProviderInfo; }
            set { _orderExecutionProviderInfo = value; }
        }
    }
}
