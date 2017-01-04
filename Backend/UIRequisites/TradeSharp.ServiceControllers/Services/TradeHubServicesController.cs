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
using System.Threading.Tasks;
using TradeSharp.ServiceControllers.Managers;
using TradeSharp.UI.Common;
using TradeSharp.UI.Common.Constants;
using TradeSharp.UI.Common.Models;

namespace TradeSharp.ServiceControllers.Services
{
    /// <summary>
    /// Handles TradeHub Application services related calls
    /// </summary>
    public class TradeHubServicesController
    {
        private Type _type = typeof (TradeHubServicesController);

        /// <summary>
        /// Handles all service related functionality
        /// </summary>
        private TradeHubServicesManager _servicesManager;

        /// <summary>
        /// Defautl Constructor
        /// </summary>
        public TradeHubServicesController()
        {
            // Initialize Manager
            _servicesManager = new TradeHubServicesManager();
        }

        /// <summary>
        /// Returns available services information
        /// </summary>
        /// <returns></returns>
        public List<ServiceDetails> GetAvailableServices()
        {
            return _servicesManager.ServiceDetailsCollection;
        }

        /// <summary>
        /// Initialize all available services
        /// </summary>
        public void InitializeServices()
        {
            _servicesManager.InitializeServices();


            // TEST CODE
            foreach (var service in GetAvailableServices())
            {
                if (service.Status.Equals(ServiceStatus.Running))
                {
                    EventSystem.Publish<ServiceDetails>(service);
                }
            }
        }

        /// <summary>
        /// Starts given service
        /// </summary>
        /// <param name="serviceDetails">Contains service information</param>
        public async void StartService(ServiceDetails serviceDetails)
        {
            if (serviceDetails.Status.Equals(ServiceStatus.Stopped))
            {
                await Task.Run(() =>
                {
                    _servicesManager.StartService(serviceDetails);

                });
            }

            // Notify listeners if the service is running
            if (serviceDetails.Status == ServiceStatus.Running)
            {
                EventSystem.Publish<ServiceDetails>(serviceDetails);
            }
        }

        /// <summary>
        /// Stop given service
        /// </summary>
        /// <param name="serviceDetails">Contains service information</param>
        public async void StopService(ServiceDetails serviceDetails)
        {
            if (serviceDetails.Status.Equals(ServiceStatus.Running))
            {
                await Task.Run(() =>
                {
                    _servicesManager.StopService(serviceDetails);

                });
            }

            // Notify listeners if the service is running
            if (serviceDetails.Status == ServiceStatus.Stopped)
            {
                EventSystem.Publish<ServiceDetails>(serviceDetails);
            }
        }
    }
}
