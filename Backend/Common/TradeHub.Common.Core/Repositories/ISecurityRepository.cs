using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeHub.Common.Core.DomainModels;

namespace TradeHub.Common.Core.Repositories
{
    public interface ISecurityRepository:IRepository<Security>
    {
        Security FindyBySymbol(object symbol);
    }
}
