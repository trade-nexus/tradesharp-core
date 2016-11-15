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
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spring.Context.Support;
using TraceSourceLogger;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.ValueObjects.AdminMessages;
using TradeHub.Common.Core.ValueObjects.MarketData;
using TradeHub.MarketDataEngine.Client.Service;

namespace TradeHub.MarketDataEngine.Client.Console
{
    class Client
    {
        private MarketDataEngineClient _marketDataEngineClient;
        private Type _type = typeof (Client);

        private int count = 1;
        private Stopwatch stopwatch;

        public void start()
        {
            _marketDataEngineClient=new MarketDataEngineClient();
            stopwatch=new Stopwatch();
            _marketDataEngineClient.ServerConnected += _marketDataEngineClient_ServerConnected;
            _marketDataEngineClient.LogonArrived += _marketDataEngineClient_LogonArrived;
            _marketDataEngineClient.InquiryResponseArrived += _marketDataEngineClient_InquiryResponseArrived;
            _marketDataEngineClient.TickArrived += TickArrived;
            _marketDataEngineClient.Start();
           

            

           
        }

        void _marketDataEngineClient_ServerConnected()
        {
            _marketDataEngineClient.SendLoginRequest(new Login() { MarketDataProvider = Common.Core.Constants.MarketDataProvider.Simulated });
        }

        void _marketDataEngineClient_InquiryResponseArrived(Common.Core.ValueObjects.Inquiry.MarketDataProviderInfo obj)
        {
            Subscribe subscribe = new Subscribe();
            subscribe.MarketDataProvider = Common.Core.Constants.MarketDataProvider.Simulated;
            subscribe.Security = new Security() { Symbol = "IBM" };
            _marketDataEngineClient.SendTickSubscriptionRequest(subscribe);
        }

        void _marketDataEngineClient_LogonArrived(string obj)
        {
            _marketDataEngineClient.SendInquiryRequest(Common.Core.Constants.MarketDataProvider.Simulated);
        }

        void TickArrived(Common.Core.DomainModels.Tick obj)
        {  
            if (count == 1)
                {
                    stopwatch.Start();
                    Logger.Info("First tick arrived " + obj, _type.FullName, "TickArrived");
                }
            if (count == 1000000)
            {
                stopwatch.Stop();
                Logger.Info("Last tick arrived " + obj, _type.FullName, "TickArrived");
                Logger.Info("1000000 Ticks recevied in " + stopwatch.ElapsedMilliseconds + " ms", _type.FullName, "TickArrived");
                Logger.Info(1000000 / stopwatch.ElapsedMilliseconds * 1000 + "msg/sec", _type.FullName, "TickArrived");
                _marketDataEngineClient.SendLogoutRequest(new Logout { MarketDataProvider = Common.Core.Constants.MarketDataProvider.Simulated });
                Close();
               
            }

            count++;
            


        }
        public void Close()
        {
            _marketDataEngineClient.Shutdown();
            
        }
    }
}
