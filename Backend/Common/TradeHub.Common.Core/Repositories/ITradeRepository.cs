using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeHub.Common.Core.Constants;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.DomainModels.OrderDomain;
using TradeHub.Common.Core.Repositories.Parameters;

namespace TradeHub.Common.Core.Repositories
{
    /// <summary>
    /// Contains necessary elements related to Trade Persisence
    /// </summary>
    public interface ITradeRepository : IPersistRepository<Trade>, IReadOnlyRepository<Trade, string>
    {
        //NOTE: Place all the searching criterias regarding Trade in this interface.
        IList<Trade> FilterByExecutionProvider(string executionProvider);
        IList<Trade> FilterByTradeSide(TradeSide tradeSide);
        IList<Trade> FilterBySecurity(Security security);
        IList<Trade> Filter(Dictionary<TradeParameters, string> parameters);
    }
}
