using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.ValueObjects.MarketData;

namespace TradeHub.Common.Core.MarketDataProvider
{
    /// <summary>
    /// Interface to be implemented by Market Data Provider Gateways providing Historic Bar Data
    /// </summary>
    public interface IHistoricBarDataProvider
    {
        #region Events

        /// <summary>
        /// Fired when requested Historic Bar Data arrives
        /// </summary>
        event Action<HistoricBarData> HistoricBarDataArrived;

        #endregion

        #region Methods

        /// <summary>
        /// Historic Bar Data Request Message
        /// </summary>
        bool HistoricBarDataRequest(HistoricDataRequest historicDataRequest);

        #endregion

    }
}
