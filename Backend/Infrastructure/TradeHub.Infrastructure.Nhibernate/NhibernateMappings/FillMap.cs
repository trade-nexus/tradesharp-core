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
