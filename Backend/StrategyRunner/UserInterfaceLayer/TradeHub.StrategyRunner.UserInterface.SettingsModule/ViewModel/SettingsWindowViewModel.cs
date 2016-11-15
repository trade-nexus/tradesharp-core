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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;
using TradeHub.StrategyRunner.UserInterface.Common;
using TradeHub.StrategyRunner.UserInterface.SettingsModule.Utility;

namespace TradeHub.StrategyRunner.UserInterface.SettingsModule.ViewModel
{
    public class SettingsWindowViewModel : ViewModelBase
    {
        private string _path = @"HistoricalDataConfiguration\HistoricalDataProvider.xml";

        /// <summary>
        /// Start Date for the Historical Data to be used
        /// </summary>
        private string _startDate;

        /// <summary>
        /// End Date for the Historical Data to be used
        /// </summary>
        private string _stopDate;

        /// <summary>
        /// Start Date for the Historical Data to be used
        /// </summary>
        public string StartDate
        {
            get { return _startDate; }
            set
            {
                _startDate = value;
                RaisePropertyChanged("StartDate");
            }
        }

        /// <summary>
        /// End Date for the Historical Data to be used
        /// </summary>
        public string StopDate
        {
            get { return _stopDate; }
            set
            {
                _stopDate = value;
                RaisePropertyChanged("StopDate");
            }
        }

        public ICommand SaveSettingsCommand { get; set; }

        /// <summary>
        /// Default Constructor
        /// </summary>
        public SettingsWindowViewModel()
        {
            SaveSettingsCommand = new DelegateCommand(SaveSettings);

            EventSystem.Subscribe<string>(UpdateCurrentValues);
        }

        /// <summary>
        /// Saves current values for Start/Stop dates
        /// </summary>
        private void SaveSettings()
        {
            if(!VerifyValues())
            {
                return;
            }

            UpdateSettingsFile();

            EventSystem.Publish<string>("CloseSettingsWindow");
        }

        /// <summary>
        /// Verifies if the correct values are present
        /// </summary>
        /// <returns></returns>
        private bool VerifyValues()
        {
            // Check Start Date value
            var tempArray = _startDate.Split(',');
            if(tempArray.Length!=3)
            {
                return false;
            }

            // Check Stop Date value
            tempArray = _stopDate.Split(',');
            if (tempArray.Length != 3)
            {
                return false;
            }
            
            return true;
        }

        /// <summary>
        /// Updates the current values according to the saved data in settings file
        /// </summary>
        /// <param name="value"></param>
        private void UpdateCurrentValues(string value)
        {
            // Get Current Directory
            var directory = System.AppDomain.CurrentDomain.BaseDirectory;

            if (value.Equals("UpdateSettingsValues"))
            {
                var values = XmlFileHandler.GetValues(directory + @"\" + _path);

                _startDate = values.Item1;
                _stopDate = values.Item2;
            }
        }

        /// <summary>
        /// Updates values in the settings file 
        /// </summary>
        private void UpdateSettingsFile()
        {
            // Get Current Directory
            var directory = System.AppDomain.CurrentDomain.BaseDirectory;

            XmlFileHandler.SaveValues(_startDate, _stopDate, directory + @"\" + _path);
        }
    }
}
