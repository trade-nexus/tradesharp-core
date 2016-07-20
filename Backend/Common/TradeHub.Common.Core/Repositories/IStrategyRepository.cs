using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeHub.Common.Core.DomainModels;

namespace TradeHub.Common.Core.Repositories
{
    /// <summary>
    /// Strategy repository interface
    /// </summary>
    public interface IStrategyRepository:IPersistRepository<Strategy>
    {
    }
}
