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
