using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeHub.Common.Core.ValueObjects.Inquiry
{
    /// <summary>
    /// Contains information regarding which functionality is provider by Market Data Provider
    /// </summary>
    public class MarketDataProviderInfo
    {
        private string _dataProviderName = string.Empty;
        private bool _providesTickData = false;
        private bool _providesLiveBarData = false;
        private bool _providesHistoricalBarData = false;

        public bool ProvidesTickData
        {
            get { return _providesTickData; }
            set { _providesTickData = value; }
        }

        public bool ProvidesLiveBarData
        {
            get { return _providesLiveBarData; }
            set { _providesLiveBarData = value; }
        }

        public bool ProvidesHistoricalBarData
        {
            get { return _providesHistoricalBarData; }
            set { _providesHistoricalBarData = value; }
        }

        public string DataProviderName
        {
            get { return _dataProviderName; }
            set { _dataProviderName = value; }
        }

        /// <summary>
        /// ToString overrride for Market Data Provdider Info
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append("MarketDataProviderInfo :: ");
            stringBuilder.Append(" Name: " + _dataProviderName);
            stringBuilder.Append(" | Ticks: " + _providesTickData);
            stringBuilder.Append(" | Live Bars: " + _providesLiveBarData);
            stringBuilder.Append(" | Historical Bars: " + _providesHistoricalBarData);

            return stringBuilder.ToString();
        }
    }
}
