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
using System.Windows;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Prism.UnityExtensions;
using Spring.Context;
using Spring.Context.Support;
using TradeHub.SimulatedExchange.UserInterface.Shell;

namespace TradeHub.SimulatedExchange.UserInterface.Simulator
{
    public class Bootstrapper : UnityBootstrapper
    {

        private readonly Type _oType = typeof (Bootstrapper);

        /// <summary>
        /// Initializing Application Shell.
        /// Need to run initialization steps to ensure that 
        /// the shell is ready to be displayed.
        /// </summary>
        protected override void InitializeShell()
        {
            base.InitializeShell();
            Application.Current.MainWindow = (Window) Shell;
            Application.Current.MainWindow.Show();
        }


        /// <summary>
        /// Creating Application Shell.
        /// Resolving Shell from Container 
        /// and returning it to parent class
        /// </summary>
        /// <returns></returns>
        protected override DependencyObject CreateShell()
        {
            IApplicationContext context = ContextRegistry.GetContext();
            return (ApplicationShell)context.GetObject("ApplicationShell");
        }

        /// <summary>
        /// In UnityBootstrapper class the Run method calls the 
        /// CreateModuleCatalog method and then sets the class's 
        /// ModuleCatalog property using the returned value
        /// </summary>
        protected override void ConfigureModuleCatalog()
        {
            base.ConfigureModuleCatalog();
            var moduleCatalog = (ModuleCatalog) ModuleCatalog;
            /*Populate a module catalog from another data
              source by calling the AddModule method or by deriving 
              from ModuleCatalog to create a module catalog with customized behavior.*/
            /*  moduleCatalog.AddModule(typeof(ProviderModule.ProviderModule));
                moduleCatalog.AddModule(typeof(DataModule.DataModule));*/
        }
    }
}
