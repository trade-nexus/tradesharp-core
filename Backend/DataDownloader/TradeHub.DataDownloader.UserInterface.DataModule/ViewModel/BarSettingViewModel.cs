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


﻿
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;
using TraceSourceLogger;
using TradeHub.Common.Core.Constants;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.ValueObjects.MarketData;
using TradeHub.DataDownloader.UserInterface.Common;

namespace TradeHub.DataDownloader.UserInterface.DataModule.ViewModel
{
    public class BarSettingViewModel:ViewModelBase
    {
        public ObservableCollection<string> BarTypes { get; set; }
        public ObservableCollection<string> BarFormates { get; set; }
        public DataViewModel DataViewModel { get; set; }
        private Type _oType = typeof(BarSettingViewModel);

        public SecurityStatisticsViewModel StatisticsViewModel { get; set; }

        /// <summary>
        /// Enable or Disable Controls 
        /// </summary>
        private bool _isControlsEnabled=true;
        public bool IsControlsEnabled
        {
            get { return _isControlsEnabled; }
            set
            {
                _isControlsEnabled = value;
                RaisePropertyChanged("IsControlsEnabled");
            }
        }

        /// <summary>
        /// Current Selected Type
        /// </summary>
        private string _selectedType = BarPriceType.LAST;
        public string SelectedType
        {
            get { return _selectedType; }
            set
            {
                _selectedType = value;
                RaisePropertyChanged("SelectedType");
            }
        }

        /// <summary>
        /// Current Selected Type
        /// </summary>
        private string _selectedFormate = BarFormat.TIME;
        public string SelectedFormate
        {
            get { return _selectedFormate; }
            set
            {
                _selectedFormate = value;
                RaisePropertyChanged("SelectedFormate");
            }
        }

        /// <summary>
        /// Saves The Setting of 
        /// </summary>
        public ICommand SaveSettings { get; set; } 

        /// <summary>
        /// Length Of Bar
        /// </summary>
        private decimal _barLength;
        public decimal BarLength
        {
            get { return _barLength; }
            set
            {
                _barLength = value;
                RaisePropertyChanged("BarLength");
            }
        }

        /// <summary>
        /// Pip Size
        /// </summary>
        private decimal _pipSize;
        public decimal PipSize
        {
            get { return _pipSize; }
            set
            {
                _pipSize = value;
                RaisePropertyChanged("BarLength");
            }
        }

        
        public BarSettingViewModel()
        {
            BarTypes=new ObservableCollection<string>();
            BarTypes.Add(BarPriceType.ASK);
            BarTypes.Add(BarPriceType.BID);
            BarTypes.Add(BarPriceType.LAST);
            BarTypes.Add(BarPriceType.MEAN);
            BarFormates = new ObservableCollection<string>();
            BarFormates.Add(BarFormat.DISPLACEMENT);
            BarFormates.Add(BarFormat.EQUAL_ENGINEERED);
            BarFormates.Add(BarFormat.TIME);
            BarFormates.Add(BarFormat.UNEQUAL_ENGINEERED);
            SaveSettings=new DelegateCommand(SaveCurrentSettingForBar);
        }

        /// <summary>
        /// Methord Fired When User Click On Save Command.
        /// </summary>
        private void SaveCurrentSettingForBar()
        {
            try
            {
                if (IsControlsEnabled)
                {
                    BarDataRequest newBarDataRequest = new BarDataRequest
                        {
                            BarFormat = SelectedFormate,
                            Security = new Security {Symbol = StatisticsViewModel.Symbol},
                            Id = StatisticsViewModel.Id,
                            MarketDataProvider = StatisticsViewModel.ProviderName,
                            BarLength = BarLength,
                            BarPriceType = SelectedType,
                            BarSeed = 0,
                            PipSize = PipSize
                        };
                    if (Logger.IsInfoEnabled)
                    {
                        Logger.Info("Creating Bar Request Object" + newBarDataRequest, _oType.FullName,
                                                      "SaveCurrentSettingForBar");
                    }

                    EventSystem.Publish<BarDataRequest>(newBarDataRequest);
                    IsControlsEnabled = false;
                }
            }
            catch (Exception exception)
            {
                TraceSourceLogger.Logger.Error(exception, _oType.FullName, "SaveCurrentSettingForBar");
            }
        }
    }
}
