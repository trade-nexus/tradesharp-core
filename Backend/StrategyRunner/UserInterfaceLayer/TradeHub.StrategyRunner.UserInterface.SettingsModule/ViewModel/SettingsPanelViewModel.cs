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
using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;
using Spring.Context.Support;
using TradeHub.StrategyRunner.UserInterface.Common;
using TradeHub.StrategyRunner.UserInterface.SettingsModule.View;

namespace TradeHub.StrategyRunner.UserInterface.SettingsModule.ViewModel
{
    public class SettingsPanelViewModel : ViewModelBase
    {
        /// <summary>
        /// Command to edit Start/Stop times for Historical Data
        /// </summary>
        public ICommand EditHistoricalDataConfig { get; set; }

        private SettingsWindow _settingsWindow;

        /// <summary>
        /// Default Constructor
        /// </summary>
        public SettingsPanelViewModel()
        {
            EditHistoricalDataConfig = new DelegateCommand(EditHistoricalDataSettings);

            EventSystem.Subscribe<string>(OnSaveSettings);
        }

        /// <summary>
        /// Called when event is raised to edit Historical Data settings
        /// </summary>
        private void EditHistoricalDataSettings()
        {
            // Get View to display details
            var context = ContextRegistry.GetContext();
            _settingsWindow = context.GetObject("SettingsWindowView") as SettingsWindow;

            EventSystem.Publish<string>("UpdateSettingsValues");

            _settingsWindow.ShowDialog();
        }

        private void OnSaveSettings(string value)
        {
            if (value.Equals("CloseSettingsWindow"))
            {
                _settingsWindow.Hide();
            }
        }
    }
}
