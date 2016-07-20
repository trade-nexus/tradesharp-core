using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using TradeHub.Common.Core.DomainModels;

namespace TradeHub.Infrastructure.Nhibernate.NhibernateMappings
{
    /// <summary>
    /// Strategy mapping for NHibernate
    /// </summary>
    public class StrategyMap : ClassMapping<Strategy>
    {
        public StrategyMap()
        {
            Table("strategy");
            Lazy(false);
            Id(x=>x.Id,m=>m.Generator(Generators.Native));
            Property(x=>x.Name);
            Property(x=>x.StartDateTime);
        }
    }
}
