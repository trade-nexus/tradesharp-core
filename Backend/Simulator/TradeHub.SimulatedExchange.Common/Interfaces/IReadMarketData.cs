using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.ValueObjects.MarketData;

namespace TradeHub.SimulatedExchange.Common.Interfaces
{
    public interface IReadMarketData
    {
        IEnumerable<Bar> ReadBars(DateTime startTime, DateTime endTime, string providerName, BarDataRequest request);

        IEnumerable<Bar> ReadBars(DateTime startTime, DateTime endTime, string providerName, Subscribe subscribe);

        IEnumerable<Bar> ReadBars(string providerName, HistoricDataRequest historicDataRequest);
    }
}
