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


﻿using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;
using TradeHub.Common.Core.Constants;
using TradeHub.DataDownloader.Common.ConcreteImplementation;
using TradeHub.DataDownloader.UserInterface.Common;
using TradeHub.DataDownloader.UserInterface.Common.Messages;

namespace TradeHub.DataDownloader.UserInterface.ProviderModule.ViewModel
{
    public class WritePermissionViewModel:ViewModelBase
    {
        private string _selectedProvider;
        /// <summary>
        /// Binds to Csv Check Box
        /// </summary>
        private bool _writeToCsv;
        public bool WriteToCsv
        {
            get { return _writeToCsv; }
            set
            {
                _writeToCsv = value;
                RaisePropertyChanged("WriteToCsv");
            }
        }

        /// <summary>
        /// Binds to Csv Check Box
        /// </summary>
        private bool _writeToBinary;
        public bool WriteToBinary
        {
            get { return _writeToBinary; }
            set
            {
                _writeToBinary = value;
                RaisePropertyChanged("WriteToBinary");
            }
        }

        /// <summary>
        /// Binds to Csv Check Box
        /// </summary>
        private bool _writeToDatabase;
        public bool WriteToDatabase
        {
            get { return _writeToDatabase; }
            set
            {
                _writeToDatabase = value;
                RaisePropertyChanged("WriteToDatabase");
            }
        }

        /// <summary>
        /// Command To provide Save Action
        /// </summary>
        public ICommand Save { get; set; }
        public WritePermissionViewModel()
        {
            Save=new DelegateCommand(SaveAction);
            EventSystem.Subscribe<SelectedProviderFromList>(SelectedProvider);
        }

        /// <summary>
        /// Methord fired then user Click Save button
        /// It Over Writes the Broker Permissions
        /// </summary>
        private void SaveAction()
        {
            EventSystem.Publish<ProviderPermission>(new ProviderPermission
                {
                    MarketDataProvider = _selectedProvider,
                    WriteCsv = WriteToCsv,
                    WriteBinary = WriteToBinary,
                    WriteDatabase = WriteToDatabase
                });
        }

        /// <summary>
        /// Current Selected Provider
        /// </summary>
        /// <param name="providerFromList"></param>
        private void SelectedProvider(SelectedProviderFromList providerFromList)
        {
            _selectedProvider = providerFromList.ProviderName;
        }

    }
}
