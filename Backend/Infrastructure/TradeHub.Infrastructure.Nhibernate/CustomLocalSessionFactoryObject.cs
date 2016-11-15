/***************************************************************************** 
* Copyright 2016 Aurora Solutions 
* 
*    http://www.aurorasolutions.io 
* 
* Aurora Solutions is an innovative services and product company at 
* the forefront of the software industry, with processes and practices 
* involving Domain Driven Design(DDD), Agile methodologies to build 
* scalable, secure, reliable and high performance products.
* 
* TradeSharp is a C# based data feed and broker neutral Algorithmic 
* Trading Platform that lets trading firms or individuals automate 
* any rules based trading strategies in stocks, forex and ETFs. 
* TradeSharp allows users to connect to providers like Tradier Brokerage, 
* IQFeed, FXCM, Blackwood, Forexware, Integral, HotSpot, Currenex, 
* Interactive Brokers and more. 
* Key features: Place and Manage Orders, Risk Management, 
* Generate Customized Reports etc 
* 
* Licensed under the Apache License, Version 2.0 (the "License"); 
* you may not use this file except in compliance with the License. 
* You may obtain a copy of the License at 
* 
*    http://www.apache.org/licenses/LICENSE-2.0 
* 
* Unless required by applicable law or agreed to in writing, software 
* distributed under the License is distributed on an "AS IS" BASIS, 
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
* See the License for the specific language governing permissions and 
* limitations under the License. 
*****************************************************************************/


﻿using System;
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
