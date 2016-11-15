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
using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;
using TradeHub.Common.Core.Constants;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.ValueObjects.MarketData;
using TradeHub.DataDownloader.UserInterface.Common;

namespace TradeHub.DataDownloader.UserInterface.DataModule.ViewModel
{
    public class HistoricBarViewModel:ViewModelBase
    {
        private Type _oType = typeof(HistoricBarViewModel);
        public SecurityStatisticsViewModel StatisticsViewModel { get; set; }
        public ObservableCollection<string> BarTypes { get; set; }
        private static int _uniqueId = 0 ;

        /// <summary>
        /// Command To Submit Request For Historic Bar To Provider.
        /// </summary>
        public ICommand SubmitHistoricBarRequest { get; set; }

        /// <summary>
        /// Property To save Start DateTime
        /// </summary>
        private DateTime _startDateTime=DateTime.Now;
        public DateTime StartDateTime
        {
            get { return _startDateTime; }
            set
            {
                _startDateTime = value;
                RaisePropertyChanged("StartDateTime");
            }
        }

        /// <summary>
        /// Selected BarType From ComboBox.
        /// </summary>
        private string _selectedBarType;
        public string SelectedBarType
        {
            get { return _selectedBarType; }
            set
            {
                _selectedBarType = value;
                RaisePropertyChanged("SelectedBarType");
            }
        }

        /// <summary>
        /// Property To save End Data Time
        /// </summary>
        private DateTime _endDateTime = DateTime.Now;
        public DateTime EndDateTime
        {
            get { return _endDateTime; }
            set
            {
                _endDateTime = value;
                RaisePropertyChanged("StartDateTime");
            }
        }

        public HistoricBarViewModel()
        {
            SubmitHistoricBarRequest=new DelegateCommand(HistoricBarRequest);
            BarTypes=new ObservableCollection<string>();
            BarTypes.Add(BarType.DAILY);
            BarTypes.Add(BarType.INTRADAY);
            BarTypes.Add(BarType.MIDPOINT);
            BarTypes.Add(BarType.MONTHLY);
            BarTypes.Add(BarType.TICK);
            BarTypes.Add(BarType.TRADE);
            BarTypes.Add(BarType.WEEKLY);
        }

        /// <summary>
        /// Methord Fired when user Ask for historic Bars.
        /// </summary>
        private void HistoricBarRequest()
        {
            try
            {
                EventSystem.Publish<HistoricDataRequest>(new HistoricDataRequest
                    {
                        BarType = SelectedBarType,
                        Id = Convert.ToString(++_uniqueId),
                        StartTime = StartDateTime.Date,
                        EndTime = EndDateTime.Date,
                        MarketDataProvider = StatisticsViewModel.ProviderName,
                        Interval = 60,
                        Security = new Security{Symbol = StatisticsViewModel.Symbol}
                    });
            }
            catch (Exception exception)
            {
                TraceSourceLogger.Logger.Error(exception, _oType.FullName, "HistoricBarRequest");
            }
        }
    }
}
