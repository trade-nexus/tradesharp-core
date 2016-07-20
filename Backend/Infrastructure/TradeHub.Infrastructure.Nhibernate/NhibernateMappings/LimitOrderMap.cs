using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate.Criterion;
using NHibernate.Mapping.ByCode.Conformist;
using TradeHub.Common.Core.DomainModels.OrderDomain;
using Order = TradeHub.Common.Core.DomainModels.OrderDomain.Order;

namespace TradeHub.Infrastructure.Nhibernate.NhibernateMappings
{
    /// <summary>
    /// Limit Order map for DB
    /// </summary>
    public class LimitOrderMap:SubclassMapping<LimitOrder>
    {
        public LimitOrderMap()
        {
            Lazy(false);
            DiscriminatorValue("LimitOrder");
            Property(x => x.LimitPrice);
        }
    }
}
