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
using Spring.Context;
using Spring.Context.Support;
using TradeHub.Common.Core.Constants;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.ValueObjects.AdminMessages;
using TradeHub.DataDownloader.ApplicationCenter;

namespace TradeHub.MarketDataEngine.ConsoleInterface
{
    /// <summary>
    /// THIS CLASS IS ONLY FOR TESTING PURPOSE
    /// This is not a permanent Class. It will be replaced by our UI Layer. 
    /// </summary>
    public class ConscoleApplication
    {
        public static ApplicationControl ApplicationControl;
        static void Main()
        {
            IApplicationContext context = ContextRegistry.GetContext();
            ApplicationControl = (ApplicationControl) context.GetObject("ApplicationControl");
            ApplicationControl.OnDataArrived+=ApplicationControlOnDataArrived;
            ApplicationControl.OnLogonArrived+=ApplicationControlOnLogonArrived;
            ApplicationControl.OnLogoutArrived+=ApplicationControlOnLogOutArrived;
            ApplicationControl.SendLogonRequest(new Login { MarketDataProvider = MarketDataProvider.Blackwood });

            Console.ReadLine();
            ApplicationControl.Logout(new Logout { MarketDataProvider = MarketDataProvider.Blackwood });
        }

        private static void ApplicationControlOnLogOutArrived(string newdata)
        {
            Console.WriteLine(newdata);
        }

        private static void ApplicationControlOnLogonArrived(string newdata)
        {
            

            Console.WriteLine(newdata);
        }

        private static void ApplicationControlOnDataArrived(MarketDataEvent newdata)
        {
            Console.WriteLine(newdata);
        }
    }
}
