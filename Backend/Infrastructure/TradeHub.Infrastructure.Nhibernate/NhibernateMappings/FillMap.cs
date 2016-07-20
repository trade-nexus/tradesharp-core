using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using TradeHub.Common.Core.Constants;
using TradeHub.Common.Core.DomainModels.OrderDomain;

namespace TradeHub.Infrastructure.Nhibernate.NhibernateMappings
{
    /// <summary>
    /// Mapping class for Order Fill
    /// </summary>
    public class FillMap:ClassMapping<Fill>
    {
        public FillMap()
        {
            Table("Fill");
            Lazy(false);
            Id(x=>x.ExecutionId,m=>m.Generator(Generators.Assigned));
            Property(x=>x.ExecutionSize);
            Property(x=>x.ExecutionPrice);
            Property(x=>x.ExecutionDateTime);
            Property(x=>x.ExecutionSide);
            //mapping Enum as a string.
            Property(x => x.ExecutionType, attr => attr.Type<NHibernate.Type.EnumStringType<ExecutionType>>());
            Property(x=>x.LeavesQuantity);
            Property(x=>x.CummalativeQuantity);
            Property(x=>x.Currency);
            Property(x=>x.AverageExecutionPrice);
            Property(x=>x.ExecutionAccount);
            Property(x=>x.ExecutionExchange);
            Property(x => x.OrderId);
            
        }

    }
}
