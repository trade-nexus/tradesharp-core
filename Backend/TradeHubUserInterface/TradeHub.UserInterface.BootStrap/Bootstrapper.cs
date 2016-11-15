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
using System.Windows;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Prism.UnityExtensions;
using Spring.Context;
using Spring.Context.Support;
using TradeHub.UserInterface.Infrastructure.ProvidersConfigurations;

namespace TradeHub.UserInterface.BootStrap
{
    public class Bootstrapper:UnityBootstrapper
    {
       // private ApplicationController _applicationController;
        protected override void InitializeShell()
        {
            base.InitializeShell();
            Application.Current.MainWindow = (Window)Shell;
            Application.Current.MainWindow.Show();
        }

        protected override System.Windows.DependencyObject CreateShell()
        {
            ApplicationShell.Shell.ApplicationShell shell=new ApplicationShell.Shell.ApplicationShell();
            return shell;
        }

        protected override void ConfigureModuleCatalog()
        {
           // _applicationController=new ApplicationController();
            //set logging path
            string path = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) +
                              "\\TradeHub Logs\\UserInterface";
            TraceSourceLogger.Logger.LogDirectory(path);
            IApplicationContext context = ContextRegistry.GetContext();
            //ProvidersController controller=new ProvidersController();
            base.ConfigureModuleCatalog();
            var moduleCatalog = (ModuleCatalog)ModuleCatalog;
            /*Populate a module catalog from another data
              source by calling the AddModule method or by deriving 
              from ModuleCatalog to create a module catalog with customized behavior.*/
            moduleCatalog.AddModule(typeof(ServicesModule.ServicesModule));
            moduleCatalog.AddModule(typeof (ComponentsModule.ComponentsModule));



        }
    }
}
