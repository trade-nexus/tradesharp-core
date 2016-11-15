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


﻿using System.Windows;
using System.Windows.Input;
using TradeHub.UserInterface.Common;
using TradeHub.UserInterface.Common.Value_Objects;
using TradeHub.UserInterface.ServicesModule.Commands;
using TradeHub.UserInterface.ServicesModule.View;

namespace TradeHub.UserInterface.ServicesModule.ViewModel
{
   public class TradeHubServicesViewModel:DependencyObject
   {
       #region Services status dependencies properties region
       public static readonly DependencyProperty MarketDataEngineServicesStatusProperty =
           DependencyProperty.Register("MarketDataEngineServicesStatus", typeof (string), typeof (TradeHubServicesViewModel), new PropertyMetadata(default(string)));

       public string MarketDataEngineServicesStatus
       {
           get { return (string) GetValue(MarketDataEngineServicesStatusProperty); }
           set { SetValue(MarketDataEngineServicesStatusProperty, value); }
       }

       public static readonly DependencyProperty OrderExecutionEngineServiceStatusProperty =
           DependencyProperty.Register("OrderExecutionEngineServiceStatus", typeof (string), typeof (TradeHubServicesViewModel), new PropertyMetadata(default(string)));

       public string OrderExecutionEngineServiceStatus
       {
           get { return (string) GetValue(OrderExecutionEngineServiceStatusProperty); }
           set { SetValue(OrderExecutionEngineServiceStatusProperty, value); }
       }

       public static readonly DependencyProperty PositionEngineServiceStatusProperty =
           DependencyProperty.Register("PositionEngineServiceStatus", typeof (string), typeof (TradeHubServicesViewModel), new PropertyMetadata(default(string)));

       public string PositionEngineServiceStatus
       {
           get { return (string) GetValue(PositionEngineServiceStatusProperty); }
           set { SetValue(PositionEngineServiceStatusProperty, value); }
       }
       #endregion
       #region Commands Area

       public ICommand RestartMdeCommand { get; set; }
       public ICommand RestartOeeCommand { get; set; }
       public ICommand RestartPeCommand { get; set; }
       public ICommand StartMdeCommand { get; set; }
       public ICommand StartOeeCommand { get; set; }
       public ICommand StartPeCommand { get; set; }
       public ICommand StopMdeCommand { get; set; }
       public ICommand StopOeeCommand { get; set; }
       public ICommand StopPeCommand { get; set; }


       #endregion

       public TradeHubServicesViewModel()
       {
           //subscribe to service status events
           EventSystem.Subscribe<ServiceStatus>(ServiceStatusListener);

           //start the services
           StartAllServices();
           
           //initialize commands
           RestartMdeCommand = new RestartMdeCommand(this);
           RestartOeeCommand=new RestartOeeCommand(this);
           RestartPeCommand=new RestartPeCommand(this);
           StartMdeCommand=new StartMdeCommand(this);
           StartOeeCommand=new StartOeeCommand(this);
           StartPeCommand=new StartPeCommand(this);
           StopMdeCommand=new StopMdeCommand(this);
           StopOeeCommand=new StopOeeCommand(this);
           StopPeCommand=new StopPeCommand(this);

       }

       /// <summary>
       /// Start Mde Service
       /// </summary>
       public void StartMdeService()
       {
           ServiceParams service=new ServiceParams(){CommandType = ServiceCommand.Start,ServiceName = "TradeHub MarketDataEngine Service"};
           EventSystem.Publish<ServiceParams>(service);
       }

       /// <summary>
       /// Start Oee Service
       /// </summary>
       public void StartOeeService()
       {
           ServiceParams service = new ServiceParams() { CommandType = ServiceCommand.Start, ServiceName = "TradeHub OrderExecutionEngine Service" };
           EventSystem.Publish<ServiceParams>(service);

       }

       /// <summary>
       /// Start Pe service
       /// </summary>
       public void StartPeService()
       {
           ServiceParams service = new ServiceParams() { CommandType = ServiceCommand.Start, ServiceName = "TradeHub PositionEngine Service" };
           EventSystem.Publish<ServiceParams>(service);
       }

       /// <summary>
       /// Stop Mde Service
       /// </summary>
       public void StopMdeService()
       {
           ServiceParams service = new ServiceParams() { CommandType = ServiceCommand.Stop, ServiceName = "TradeHub MarketDataEngine Service" };
           EventSystem.Publish<ServiceParams>(service);
       }

       /// <summary>
       /// Stop Oee Service
       /// </summary>
       public void StopOeeService()
       {
           ServiceParams service = new ServiceParams() { CommandType = ServiceCommand.Stop, ServiceName = "TradeHub OrderExecutionEngine Service" };
           EventSystem.Publish<ServiceParams>(service);
       }

       /// <summary>
       /// Stop Pe Service
       /// </summary>
       public void StopPeService()
       {
           ServiceParams service = new ServiceParams() { CommandType = ServiceCommand.Stop, ServiceName = "TradeHub PositionEngine Service" };
           EventSystem.Publish<ServiceParams>(service);
       }

       /// <summary>
       /// Restart Mde Service
       /// </summary>
       public void RestartMdeService()
       {
           ConfigurationView view = new ConfigurationView("Market Data Engine");
           view.ShowDialog();
       }

       /// <summary>
       /// Restart Oee Service
       /// </summary>
       public void RestartOeeService()
       {
           ConfigurationView view=new ConfigurationView("Order Execution Engine");
           view.ShowDialog();
           //ServiceParams service = new ServiceParams() { CommandType = ServiceCommand.Restart, ServiceName = "TradeHub OrderExecutionEngine Service" };
           //EventSystem.Publish<ServiceParams>(service);
       }

       /// <summary>
       /// Restart Pe Service
       /// </summary>
       public void RestartPeService()
       {
           ServiceParams service = new ServiceParams() { CommandType = ServiceCommand.Restart, ServiceName = "TradeHub PositionEngine Service" };
           EventSystem.Publish<ServiceParams>(service);
       }

       /// <summary>
       /// Listen for the service status events.
       /// </summary>
       /// <param name="serviceStatus"></param>
       private void ServiceStatusListener(ServiceStatus serviceStatus)
       {
           if (serviceStatus.ServiceName.Equals("TradeHub MarketDataEngine Service"))
           {
               MarketDataEngineServicesStatus = serviceStatus.Status;
           }
           else if (serviceStatus.ServiceName.Equals("TradeHub OrderExecutionEngine Service"))
           {
               OrderExecutionEngineServiceStatus = serviceStatus.Status;
           }
           else if (serviceStatus.ServiceName.Equals("TradeHub PositionEngine Service"))
           {
               PositionEngineServiceStatus = serviceStatus.Status;
           }
           
       }


       /// <summary>
       /// Method to start all services status at the start up
       /// </summary>
       private void StartAllServices()
       {
           ServiceParams service = new ServiceParams() { ServiceName = "TradeHub MarketDataEngine Service",CommandType = ServiceCommand.Start};
           EventSystem.Publish<ServiceParams>(service);
           service.ServiceName = "TradeHub OrderExecutionEngine Service";
           EventSystem.Publish<ServiceParams>(service);
           service.ServiceName = "TradeHub PositionEngine Service";
           EventSystem.Publish<ServiceParams>(service);
       }
   }

}
