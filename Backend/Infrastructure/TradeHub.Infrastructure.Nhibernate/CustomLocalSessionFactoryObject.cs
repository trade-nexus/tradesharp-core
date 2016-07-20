using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NHibernate.Bytecode;
using NHibernate.Mapping.ByCode;
using Spring.Data.NHibernate;
using Spring.Data.NHibernate.Bytecode;
using TradeHub.Infrastructure.Nhibernate.NhibernateMappings;

namespace TradeHub.Infrastructure.Nhibernate
{
    /// <summary>
    /// Custome overriden configurations
    /// </summary>
    public class CustomLocalSessionFactoryObject : LocalSessionFactoryObject
    {
        /// <summary>
        /// Overwritten to return Spring's bytecode provider for entity injection to work.
        /// </summary>
        public override IBytecodeProvider BytecodeProvider
        {
            get { return new BytecodeProvider(ApplicationContext); }
            set { }
        }

        public string[] ConformistMappingAssemblies { get; set; }


        public CustomLocalSessionFactoryObject()
        {
            ConformistMappingAssemblies = new string[] {};
        }

        
        /// <summary>
        /// Override to give the configuration of mapping by code assemblies
        /// </summary>
        /// <param name="config"></param>
        protected override void PostProcessConfiguration(NHibernate.Cfg.Configuration config)
        {
            base.PostProcessConfiguration(config);

            // add any conformist mappings in the listed assemblies:
            var mapper = new ModelMapper();

            //mapper.AddMappings(Assembly.GetAssembly(typeof(SecurityMap)).GetExportedTypes());
            mapper.AddMappings(Assembly.GetAssembly(typeof(OrderMap)).GetExportedTypes());
            mapper.AddMappings(Assembly.GetAssembly(typeof(FillMap)).GetExportedTypes());
            mapper.AddMappings(Assembly.GetAssembly(typeof(TradeMap)).GetExportedTypes());
            mapper.AddMappings(Assembly.GetAssembly(typeof(StrategyMap)).GetExportedTypes());
            foreach (var asm in ConformistMappingAssemblies.Select(Assembly.Load))
            {
                mapper.AddMappings(asm.GetTypes());
            }
            
            foreach (var mapping in mapper.CompileMappingForEachExplicitlyAddedEntity())
            {
                config.AddMapping(mapping);    
            }
            
        }
    }
}
