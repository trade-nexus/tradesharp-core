using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.DomainModels.OrderDomain;

namespace TradeHub.Infrastructure.Nhibernate.NhibernateMappings
{
    /// <summary>
    /// TradeHUB Trade map for DB
    /// </summary>
    public class TradeMap : ClassMapping<Trade>
    {
        /// <summary>
        /// Default Constructor
        /// </summary>
        public TradeMap()
        {
            Table("trades");
            Lazy(false);

            // Generate ID
            Id(x => x.Id, m => m.Generator(Generators.Native));

            Property(x => x.TradeSide);
            Property(x => x.TradeSize);
            Property(x => x.ProfitAndLoss);
            Property(x => x.StartTime);
            Property(x => x.CompletionTime);
            Property(x => x.ExecutionProvider);

            Map(x => x.ExecutionDetails,
                
                mapping =>
                {
                    mapping.Access(Accessor.Property);
                    mapping.Lazy(CollectionLazy.NoLazy);
                    mapping.Cascade(Cascade.All);
                    mapping.Key(k => k.Column("TradeId"));
                    mapping.Table("tradedetails");
                },
                mapping => mapping.Element(k => k.Column("ExecutionId")),
                mapping => mapping.Element(k => k.Column("ExecutionSize")));

            Component(x => x.Security, m =>
            {
                //properties mapping
                m.Property(x => x.Symbol);

                //additional info
                m.Class<Security>();
                m.Insert(true);
                m.Update(true);
                m.OptimisticLock(true);
                m.Lazy(false);
            });
        }
    }
}
