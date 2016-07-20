using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeHub.Common.Core.DomainModels;

namespace TradeHub.SimulatedExchange.DomainObjects
{
    public class HistoricBarIdentity
    {
        public HistoricBarData HistoricBarData { get; set; }
        public string Id { get; set; }
    }
}
