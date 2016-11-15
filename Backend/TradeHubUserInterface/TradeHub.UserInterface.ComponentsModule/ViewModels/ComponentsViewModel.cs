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
using System.Windows.Input;
using TraceSourceLogger;
using TradeHub.UserInterface.Common;
using TradeHub.UserInterface.Common.Value_Objects;
using TradeHub.UserInterface.ComponentsModule.Commands;

namespace TradeHub.UserInterface.ComponentsModule.ViewModels
{
    public class ComponentsViewModel
    {
        private Type _type = typeof (ComponentsViewModel);
        public ICommand OpenStrategyRunnerCommand { get; set; }
        public ICommand OpenMddCommand { get; set; }
        public ICommand OpenClerkCommand { get; set; }

        public ComponentsViewModel()
        {
            OpenStrategyRunnerCommand=new OpenStrategyRunnerCommand(this);
            OpenMddCommand=new OpenDataDownloaderCommand(this);
            OpenClerkCommand=new OpenClerkCommand(this);
            
        }

        /// <summary>
        /// Run the Strategy Runner
        /// </summary>
        public void OpenStrategyRunner()
        {
            LaunchComponent component=new LaunchComponent(){Command = "Run",Component = TradeHubComponent.StrategyRunner};
            EventSystem.Publish<LaunchComponent>(component);
            Logger.Info("Event Published to run StrategyRunner", _type.FullName, "OpenStrategyRunner");
        }

        /// <summary>
        /// Run Market Data Downloader
        /// </summary>
        public void OpenMdd()
        {
            
        }

        /// <summary>
        /// Open Clerk
        /// </summary>
        public void OpenClerk()
        {
            LaunchComponent component = new LaunchComponent() { Command = "Run", Component = TradeHubComponent.Clerk };
            EventSystem.Publish<LaunchComponent>(component);
            Logger.Info("Event Published to run Clerk", _type.FullName, "OpenClerk");
            
        }
    }
}
