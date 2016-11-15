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
using System.ServiceProcess;
using TraceSourceLogger;
using TradeHub.UserInterface.Common;
using TradeHub.UserInterface.Common.Interfaces;

namespace TradeHub.UserInterface.Infrastructure
{
    public class ServiceControl:IServiceControl<ServiceParams>
    {
        private Type _type = typeof(ServiceControl);
        private int _timeout = 4000;
        public void StartService(ServiceParams service)
        {
            ServiceController controller = new ServiceController(service.ServiceName);
            try
            {
                if (!ServiceStatus(service.ServiceName).Equals("Running"))
                {
                    TimeSpan timeout = TimeSpan.FromMilliseconds(_timeout);
                    controller.Start();
                    controller.WaitForStatus(ServiceControllerStatus.Running, timeout);
                    Logger.Info("Starting service " + service.ServiceName, _type.FullName, "StartService");
                }
                else
                {
                    RestartService(service);
                }
            }
            catch(Exception exception)
            {
                Logger.Error(exception, _type.FullName, "StartService");
            }
        }

        public void StopService(ServiceParams service)
        {
            ServiceController controller = new ServiceController(service.ServiceName);
            try
            {
                if (!ServiceStatus(service.ServiceName).Equals("Stopped"))
                {
                    TimeSpan timeout = TimeSpan.FromMilliseconds(_timeout);
                    controller.Stop();
                    controller.WaitForStatus(ServiceControllerStatus.Stopped, timeout);
                    Logger.Info("Stoping service " + service.ServiceName, _type.FullName, "StopService");
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "StopService");
            }
        }

        public void RestartService(ServiceParams service)
        {
            ServiceController controller = new ServiceController(service.ServiceName);
            try
            {
                int millisec1 = Environment.TickCount;
                TimeSpan timeout = TimeSpan.FromMilliseconds(_timeout);
                if (!ServiceStatus(service.ServiceName).Equals("Stopped"))
                {
                    controller.Stop();
                    controller.WaitForStatus(ServiceControllerStatus.Stopped, timeout);
                    // count the rest of the timeout
                    int millisec2 = Environment.TickCount;
                    timeout = TimeSpan.FromMilliseconds(_timeout - (millisec2 - millisec1));
                }

                controller.Start();
                controller.WaitForStatus(ServiceControllerStatus.Running, timeout);
                Logger.Info("Restarting service " + service.ServiceName, _type.FullName, "StopService");
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "RestartService");
            }
        }

        public string ServiceStatus(string serviceName)
        {
            ServiceController controller = new ServiceController(serviceName);
            try
            {
                return controller.Status.ToString();
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "RestartService");
            }
            return "Not Responding";
        }
    }
}
