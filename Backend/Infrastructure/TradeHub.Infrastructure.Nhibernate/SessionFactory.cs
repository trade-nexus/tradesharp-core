using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Dialect;
using NHibernate.Driver;
using NHibernate.Mapping.ByCode;
using NHibernate.Tool.hbm2ddl;
using TradeHub.Infrastructure.Nhibernate.NhibernateMappings;

namespace TradeHub.Infrastructure.Nhibernate
{
    /// <summary>
    /// session factory class without spring configurations
    /// </summary>
    public abstract class SessionFactory
    {
        //TODO: Have to replace session and transactions through spring
        public static ISessionFactory GetSessionFactory()
        {
            var cfg = new Configuration();
            cfg.DataBaseIntegration(x =>
            {
                x.ConnectionString = "Server=localhost;Database=TradeHub;User ID=root;Password=;";
                x.Driver<MySqlDataDriver>();
                x.Dialect<MySQL5Dialect>();

            });
            cfg.AddAssembly(Assembly.GetExecutingAssembly());
            var mappings = GetMappings();
            cfg.AddDeserializedMapping(mappings, "NHSchemaTest");
            SchemaMetadataUpdater.QuoteTableAndColumns(cfg);
            return cfg.BuildSessionFactory();
        }

        public static HbmMapping GetMappings()
        {
            var mapper = new ModelMapper();
            mapper.AddMappings(Assembly.GetAssembly(typeof(SecurityMap)).GetExportedTypes());
            mapper.AddMappings(Assembly.GetAssembly(typeof(OrderMap)).GetExportedTypes());
            mapper.AddMappings(Assembly.GetAssembly(typeof(FillMap)).GetExportedTypes());
            var mapping = mapper.CompileMappingForAllExplicitlyAddedEntities();
            return mapping;
        }
    }
}
