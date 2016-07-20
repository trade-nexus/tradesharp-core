using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using NHibernate.Properties;
using TradeHub.Common.Core.DomainModels;

namespace TradeHub.Infrastructure.Nhibernate.NhibernateMappings
{
    /// <summary>
    /// Security Map for DB
    /// </summary>
    public class SecurityMap //: ClassMapping<Security>
    {
        public SecurityMap()
        {
            //Lazy(false);
            ////Id(x => x.Isin,m=>m.Generator(Generators.Assigned));
            //Id(x=>x.Id,m=>m.Generator(Generators.Native));
            //Property(x=>x.Symbol);
            //Property(x => x.SecurityType);
            //Property(x=>x.Isin);
            
        }

    }
}
