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
using TraceSourceLogger;
using TradeHub.UserInterface.Common;
using TradeHub.UserInterface.Common.Value_Objects;
using TradeHub.UserInterface.Infrastructure.ProvidersConfigurations;

namespace TradeHub.UserInterface.Infrastructure
{
    public class ApplicationController
    {
        private Type _type = typeof (ApplicationController);
        private ProvidersController _providersController;
        public ApplicationController()
        {
            EventSystem.Subscribe<ServiceParams>(ListenServiceEvents);
            EventSystem.Subscribe<LaunchComponent>(LaunchTradeHubComponent);
            EventSystem.Subscribe<string>(ListenStringCommands);
            Logger.Info("Subscribed the events", _type.FullName, "ApplicationController");
            _providersController = new ProvidersController();
        }

        /// <summary>
        /// Event listener for the services controls
        /// </summary>
        /// <param name="service"></param>
        private void ListenServiceEvents(ServiceParams service)
        {
            Logger.Info("Recieved event to control service" + service.ServiceName, _type.FullName,
               "ListenServiceEvents");
           ServiceControl serviceControl=new ServiceControl();
            switch (service.CommandType)
            {
                case ServiceCommand.Start:
                    serviceControl.StartService(service);
                    break;

                case ServiceCommand.Stop:
                    serviceControl.StopService(service);
                    break;

                case ServiceCommand.Restart:
                    serviceControl.RestartService(service);
                    break;
            }
            //send the service status to update UI
            ServiceStatus serviceStatus=new ServiceStatus();
            serviceStatus.ServiceName = service.ServiceName;
            serviceStatus.Status = serviceControl.ServiceStatus(service.ServiceName);
            EventSystem.Publish(serviceStatus);

        }

        /// <summary>
        /// Event Listener for launching the components
        /// </summary>
        /// <param name="component"></param>
        private void LaunchTradeHubComponent(LaunchComponent component)
        {
            Logger.Info("Recieved event to launch component:" + component.Component, _type.FullName,
                "LaunchTradeHubComponent");
            string path = "";
            switch (component.Component)
            {
                case TradeHubComponent.StrategyRunner:
                    path = @"..\Strategy Runner\TradeHub.StrategyRunner.UserInterface.exe";
                    RunTradeHubComponent.RunComponent(path);
                    break;

                case TradeHubComponent.Clerk:
                    path = @"..\Clerk\ClerkUI.exe";
                    RunTradeHubComponent.RunComponent(path);
                    break;
            }
        }

        /// <summary>
        /// Listen to string events
        /// </summary>
        /// <param name="command"></param>
        private void ListenStringCommands(string command)
        {
            if (command.Equals("ShutDown"))
            {
                ShutDownSystem();
            }
            
        }

        /// <summary>
        /// Shut down system, stop all services and components running
        /// </summary>
        private void ShutDownSystem()
        {
            try
            {
                //shutdown any process running
                RunTradeHubComponent.ShutdownProcess();

                //shut down all the services running
                CloseAllServices();
            }
            catch (Exception exception)
            {

                Logger.Error(exception, _type.FullName, "ShutDownSystem");
            }
            
        }

        /// <summary>
        /// Close all services
        /// </summary>
        private void CloseAllServices()
        {
            ServiceControl serviceControl = new ServiceControl();
            ServiceParams service = new ServiceParams() { CommandType = ServiceCommand.Stop, ServiceName = "TradeHub MarketDataEngine Service" };
            serviceControl.StopService(service);
            service = new ServiceParams() { CommandType = ServiceCommand.Stop, ServiceName = "TradeHub OrderExecutionEngine Service" };
            serviceControl.StopService(service);
            service = new ServiceParams() { CommandType = ServiceCommand.Stop, ServiceName = "TradeHub PositionEngine Service" };
            serviceControl.StopService(service);
        }
    }
}
