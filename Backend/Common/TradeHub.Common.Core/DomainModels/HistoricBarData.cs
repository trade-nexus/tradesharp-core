using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeHub.Common.Core.ValueObjects.MarketData;

namespace TradeHub.Common.Core.DomainModels
{
    /// <summary>
    /// Contains Historical Bar Data info
    /// </summary>
    [Serializable()]
    public class HistoricBarData : MarketDataEvent
    {
        // Historical Bars
        private Bar[] _bars;

        // Request ID for the Historical Bar Data Requests
        private string _reqId;

        /// <summary>
        /// Contains detailed information of received bars
        /// </summary>
        private HistoricDataRequest _barsInformation;

        /// <summary>
        /// Argument Constructor
        /// </summary>
        public HistoricBarData(Security security, string marketDataProvider, DateTime dateTime)
            : base(security, marketDataProvider, dateTime)
        {

        }

        /// <summary>
        /// Gets/Sets Historical Bars array
        /// </summary>
        public Bar[] Bars
        {
            get { return _bars; }
            set { _bars = value; }
        }

        /// <summary>
        /// Gets/Sets Request ID for the Historical Bar Data Requests
        /// </summary>
        public string ReqId
        {
            get { return _reqId; }
            set { _reqId = value; }
        }

        /// <summary>
        /// Contains detailed information of received bars
        /// </summary>
        public HistoricDataRequest BarsInformation
        {
            get { return _barsInformation; }
            set { _barsInformation = value; }
        }

        /// <summary>
        /// Overrides ToString Method
        /// </summary>
        public override String ToString()
        {
            return " HistoricBarData :: " +
                   " Timestamp : " + DateTime +
                   " | Market Data Provider : " + MarketDataProvider +
                   " | Request ID : " + ReqId +
                   " | " + Security;
        }
    }
}
