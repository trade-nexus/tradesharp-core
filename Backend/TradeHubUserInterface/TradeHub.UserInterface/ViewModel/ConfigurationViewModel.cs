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
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using TraceSourceLogger;
using TradeHub.UserInterface.Common;
using TradeHub.UserInterface.Infrastructure.ProvidersConfigurations;
using TradeHub.UserInterface.ServicesModule.Commands;

namespace TradeHub.UserInterface.ServicesModule.ViewModel
{
    public class ConfigurationViewModel:DependencyObject
    {
        public ObservableCollection<Parameters> ProviderParameterses;

        public List<string> ProviderList;

        public static readonly DependencyProperty SelectedProviderProperty =
            DependencyProperty.Register("SelectedProvider", typeof (string), typeof (ConfigurationViewModel), new PropertyMetadata(default(string)));

        public string SelectedProvider
        {
            get { return (string) GetValue(SelectedProviderProperty); }
            set { SetValue(SelectedProviderProperty, value); }
        }

        private string _serviceName;

        public ICommand SaveCommand { get; set; }

        private Dispatcher _currentDispatcher;
        public ConfigurationViewModel(string serviceName)
        {
            _currentDispatcher = Dispatcher.CurrentDispatcher;
            _serviceName = serviceName;
            SaveCommand=new SaveCommand(this);
            ProviderParameterses=new ObservableCollection<Parameters>();
            ProviderList=new List<string>();
            ProviderList.Add("Blackwood");
            SelectedProvider = "Blackwood";
            EventSystem.Subscribe<List<Parameters>>(ReceivedParameters);
            LoadProviderParameters();
            
        }

        /// <summary>
        /// Function to request the call for loading parameters paramters
        /// </summary>
        private void LoadProviderParameters()
        {
            ServiceProvider provider=new ServiceProvider(_serviceName,SelectedProvider);
            EventSystem.Publish<ServiceProvider>(provider);
            
        }

        /// <summary>
        /// Receive providers parameters
        /// </summary>
        /// <param name="parameterses"></param>
        private void ReceivedParameters(List<Parameters> parameterses)
        {
            Logger.Info("Receieved parameters List","","");
            if (parameterses != null)
            {
                _currentDispatcher.BeginInvoke(DispatcherPriority.Normal, (Action) (() =>
                {
                    ProviderParameterses.Clear();

                    foreach (var parameterse in parameterses)
                    {
                        ProviderParameterses.Add(parameterse);
                    }

                }));
            }
        }

        /// <summary>
        /// Save the parameters.
        /// </summary>
        public void SaveParameters()
        {
            ServiceParametersList parametersList=new ServiceParametersList(new ServiceProvider(_serviceName,SelectedProvider),ProviderParameterses.ToList());
            EventSystem.Publish(parametersList);
            MessageBox.Show("Please start the service to load new configuration");


        }
    }
}
