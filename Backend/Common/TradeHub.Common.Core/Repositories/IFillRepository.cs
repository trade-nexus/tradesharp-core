using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeHub.Common.Core.DomainModels.OrderDomain;
using TradeHub.Common.Core.Repositories.Parameters;

namespace TradeHub.Common.Core.Repositories
{
    public interface IFillRepository:IPersistRepository<Fill>,IReadOnlyRepository<Fill,string>
    {
        IList<object> Find(Dictionary<FillParameters, string> parameters);
    }
}
