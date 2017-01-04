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


using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Timers;
using System.Windows.Threading;
using TraceSourceLogger;
using TradeSharp.UI.Common.Constants;
using TradeSharp.UI.Common.Models;
using TradeSharp.UI.Common.Utility;

namespace TradeSharp.ServiceControllers.Managers
{
    /// <summary>
    /// Responsible for TradeHub Application Services functionality
    /// </summary>
    internal class TradeHubServicesManager
    {
        private Type _type = typeof (TradeHubServicesManager);

        /// <summary>
        /// Holds UI thread reference
        /// </summary>
        private Dispatcher _currentDispatcher;

        /// <summary>
        /// Responsible for updating Service Status
        /// </summary>
        private Timer _statusUpdateTimer;

        /// <summary>
        /// Used with TradeHub Application Services
        /// </summary>
        private int _timeout = 4000;

        /// <summary>
        /// Contains all available services information
        /// </summary>
        private readonly List<ServiceDetails> _serviceDetailsCollection;

        /// <summary>
        /// Default Constructor
        /// </summary>
        public TradeHubServicesManager()
        {
            // Save UI thread reference
            _currentDispatcher = Dispatcher.CurrentDispatcher;

            
            _serviceDetailsCollection = new List<ServiceDetails>();

            PopulateServiceDetails();
        }

        /// <summary>
        /// Contains all available services information
        /// </summary>
        public List<ServiceDetails> ServiceDetailsCollection
        {
            get { return _serviceDetailsCollection; }
        }

        /// <summary>
        /// Populate Intial Services information
        /// </summary>
        private void PopulateServiceDetails()
        {
            // Create Service Details
            ServiceDetails marketServiceDetails = new ServiceDetails(GetEnumDescription.GetValue(TradeSharp.UI.Common.Constants.Services.MarketDataService), ServiceStatus.Disabled);
            ServiceDetails orderServiceDetails = new ServiceDetails(GetEnumDescription.GetValue(TradeSharp.UI.Common.Constants.Services.OrderExecutionService), ServiceStatus.Disabled);
            ServiceDetails positionServiceDetails = new ServiceDetails(GetEnumDescription.GetValue(TradeSharp.UI.Common.Constants.Services.PositionService), ServiceStatus.Disabled);

            // Set Display names
            marketServiceDetails.ServiceDisplayName = marketServiceDetails.ServiceName.Replace("TradeHub", "");
            marketServiceDetails.ServiceDisplayName = marketServiceDetails.ServiceDisplayName.Replace("Service", "");

            orderServiceDetails.ServiceDisplayName = orderServiceDetails.ServiceName.Replace("TradeHub", "");
            orderServiceDetails.ServiceDisplayName = orderServiceDetails.ServiceDisplayName.Replace("Service", "");

            positionServiceDetails.ServiceDisplayName = positionServiceDetails.ServiceName.Replace("TradeHub", "");
            positionServiceDetails.ServiceDisplayName = positionServiceDetails.ServiceDisplayName.Replace("Service", "");

            // Add details to collection
            _serviceDetailsCollection.Add(marketServiceDetails);
            _serviceDetailsCollection.Add(orderServiceDetails);
            _serviceDetailsCollection.Add(positionServiceDetails);

            // Get Actual Service Status
            marketServiceDetails.Status = GetServiceStatus(marketServiceDetails.ServiceName);
            orderServiceDetails.Status = GetServiceStatus(orderServiceDetails.ServiceName);
        }

        /// <summary>
        /// Initialize all available services
        /// </summary>
        public void InitializeServices()
        {
            if (_statusUpdateTimer!=null)
            {
                _statusUpdateTimer.Enabled = false;
                _statusUpdateTimer.Elapsed -= new ElapsedEventHandler(UpdateServiceStatus);
            }

            _statusUpdateTimer = new Timer();
            _statusUpdateTimer.Elapsed += new ElapsedEventHandler(UpdateServiceStatus);
            _statusUpdateTimer.Interval = 60000;
            _statusUpdateTimer.Enabled = true;

            StartServices();
        }

        /// <summary>
        /// Start Available Services
        /// </summary>
        public void StartServices()
        {
            // Travers collection
            foreach (var serviceDetails in _serviceDetailsCollection)
            {
                // Request Starting of available service
                if (!serviceDetails.Status.Equals(ServiceStatus.Disabled))
                {
                    StartService(serviceDetails);
                }
            }

            UpdateServiceStatus(null, null);
        }

        /// <summary>
        /// Start given service
        /// </summary>
        /// <param name="serviceDetails"></param>
        public async void StartService(ServiceDetails serviceDetails)
        {
            var controller = new ServiceController(serviceDetails.ServiceName);

            try
            {
                if (!controller.Status.ToString().Equals(ServiceStatus.Running.ToString()))
                {
                    TimeSpan timeout = TimeSpan.FromMilliseconds(_timeout);
                    controller.Start();
                    controller.WaitForStatus(ServiceControllerStatus.Running, timeout);

                    Logger.Info("Starting service " + serviceDetails.ServiceName, _type.FullName, "StartService");

                    UpdateServiceStatus(null, null);
                }
                else
                {
                    Logger.Info("Service " + serviceDetails.ServiceName + " :" + controller.Status.ToString(),
                        _type.FullName, "StartService");
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "StartService");
            }
        }

        /// <summary>
        /// Stops given service
        /// </summary>
        /// <param name="serviceDetails"></param>
        public async void StopService(ServiceDetails serviceDetails)
        {
            var controller = new ServiceController(serviceDetails.ServiceName);
            try
            {
                if (!controller.Status.ToString().Equals(ServiceStatus.Stopped.ToString()))
                {
                    TimeSpan timeout = TimeSpan.FromMilliseconds(_timeout);
                    controller.Stop();
                    controller.WaitForStatus(ServiceControllerStatus.Stopped, timeout);

                    Logger.Info("Stoping service " + serviceDetails.ServiceName, _type.FullName, "StopService");

                    UpdateServiceStatus(null, null);
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "StopService");
            }
        }

        /// <summary>
        /// Restarts Given Service
        /// </summary>
        /// <param name="serviceDetails"></param>
        public void RestartService(ServiceDetails serviceDetails)
        {
            ServiceController controller = new ServiceController(serviceDetails.ServiceName);
            try
            {
                int millisec1 = Environment.TickCount;
                TimeSpan timeout = TimeSpan.FromMilliseconds(_timeout);
                if (!controller.Status.ToString().Equals(ServiceStatus.Stopped.ToString()))
                {
                    controller.Stop();
                    controller.WaitForStatus(ServiceControllerStatus.Stopped, timeout);
                    // count the rest of the timeout
                    int millisec2 = Environment.TickCount;
                    timeout = TimeSpan.FromMilliseconds(_timeout - (millisec2 - millisec1));
                }

                controller.Start();
                controller.WaitForStatus(ServiceControllerStatus.Running, timeout);

                Logger.Info("Restarting service " + serviceDetails.ServiceName, _type.FullName, "StopService");
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "RestartService");
            }
        }

        /// <summary>
        /// Returns actual service status in the system
        /// </summary>
        /// <param name="serviceName"></param>
        /// <returns></returns>
        private ServiceStatus GetServiceStatus(string serviceName)
        {
            try
            {
                var controller = new ServiceController(serviceName);

                switch (controller.Status.ToString())
                {
                    case "Running":
                        return ServiceStatus.Running;
                    case "Starting":
                        return ServiceStatus.Starting;
                    case "Stopped":
                        return ServiceStatus.Stopped;
                    case "Stopping":
                        return ServiceStatus.Stopping;
                    default:
                        return ServiceStatus.Disabled;
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "GetServiceStatus");
                return ServiceStatus.Disabled;
            }
        }

        /// <summary>
        /// Update available tradehub services status
        /// </summary>
        private void UpdateServiceStatus(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            _currentDispatcher.Invoke(DispatcherPriority.Background, (Action) (() =>
            {
                foreach (var serviceDetails in _serviceDetailsCollection.ToList())
                {
                    if (!serviceDetails.Status.Equals(ServiceStatus.Disabled))
                    {
                        serviceDetails.Status = GetServiceStatus(serviceDetails.ServiceName);
                    }
                }
            }));
        }
    }
}
