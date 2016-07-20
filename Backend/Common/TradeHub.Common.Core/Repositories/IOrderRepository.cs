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
    /// Interface specific to order with additional order method required
    /// </summary>
    public interface IOrderRepository:IPersistRepository<Order>,IReadOnlyRepository<Order,string>
    {
        //NOTE: Place all the searching criterias regarding order in this interface.
        IList<Order> FilterByExecutionProvider(string executionProvider);
        IList<Order> FilterByOrderSide(string orderSide);
        IList<Order> FilterBySecurity(Security security);
        IList<object[]> Find(Dictionary<OrderParameters, string> parameters);
    }
}
