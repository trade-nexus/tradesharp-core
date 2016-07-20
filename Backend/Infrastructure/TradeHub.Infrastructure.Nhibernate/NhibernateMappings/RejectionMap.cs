using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate.Mapping.ByCode.Conformist;
using TradeHub.Common.Core.DomainModels.OrderDomain;

namespace TradeHub.Infrastructure.Nhibernate.NhibernateMappings
{
    public class RejectionMap:ClassMapping<Rejection>
    {
        public RejectionMap()
        {
            Lazy(false);
        }
        
    }
}
