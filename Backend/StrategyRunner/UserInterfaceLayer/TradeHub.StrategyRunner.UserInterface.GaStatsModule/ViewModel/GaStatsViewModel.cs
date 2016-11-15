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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Win32;
using TraceSourceLogger;
using TraceSourceLogger.Constants;
using TradeHub.StrategyRunner.Infrastructure.ValueObjects;
using TradeHub.StrategyRunner.UserInterface.Common;
using TradeHub.StrategyRunner.UserInterface.Common.ValueObjects;
using TradeHub.StrategyRunner.UserInterface.GaStatsModule.Model;

namespace TradeHub.StrategyRunner.UserInterface.GaStatsModule.ViewModel
{
    /// <summary>
    /// Contains code behind functionality for GaStatsViewModel.cs
    /// </summary>
    public class GaStatsViewModel : ViewModelBase
    {
        private readonly Type _type = typeof (GaStatsViewModel);
        
        /// <summary>
        /// Profit and Loss for the optimized strategy
        /// </summary>
        private double _pnl;

        /// <summary>
        /// Export result command
        /// </summary>
        public ICommand ExportResult { get; set; }

        /// <summary>
        /// Holds reference for the UI thread
        /// </summary>
        private Dispatcher _currentDispatcher;

        /// <summary>
        /// Collection to hold info to be dispalyed on UI
        /// </summary>
        private ObservableCollection<ParameterStats> _parametersInfo;

        /// <summary>
        /// Profit and Loss for the optimized strategy
        /// </summary>
        public double Pnl
        {
            get { return _pnl; }
            set
            {
                _pnl = value;
                RaisePropertyChanged("Pnl");
            }
        }

        /// <summary>
        /// Collection to hold info to be dispalyed on UI
        /// </summary>
        public ObservableCollection<ParameterStats> ParametersInfo
        {
            get { return _parametersInfo; }
            set { _parametersInfo = value; }
        }

        /// <summary>
        /// Default Constructor
        /// </summary>
        public GaStatsViewModel()
        {
            // Initialize Dispatcher to be used for UI modifications
            _currentDispatcher = Dispatcher.CurrentDispatcher;

            ExportResult=new DelegateCommand(ExportGeneticAlgoReuslts);

            //initilize parameters stats collection
            _parametersInfo=new ObservableCollection<ParameterStats>();
            
            // Register Event for GA Optimization results
            EventSystem.Subscribe<OptimizationResultGeneticAlgo>(DisplayOptimizationResults);
        }

        /// <summary>
        /// Displays optimization results
        /// </summary>
        private void DisplayOptimizationResults(OptimizationResultGeneticAlgo result)
        {
            try
            {
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("Parameter Results, PnL=" + result.FitnessValue, _type.FullName, "DisplayOptimizationResults");
                }
                // Update Fitness
                Pnl = Math.Round(result.FitnessValue,5);

                // Update UI Element
                _currentDispatcher.Invoke(DispatcherPriority.Normal, (Action)(() =>
                    {
                        foreach (var info in result.OptimizedParameters)
                        {
                            _parametersInfo.Add(new ParameterStats(info.Key.ToString(), info.Value));
                            if (Logger.IsInfoEnabled)
                            {
                                Logger.Info("Param=" +info.Key+" , Value="+info.Value, _type.FullName, "DisplayOptimizationResults");
                            }
                        }
                        _parametersInfo.Add(new ParameterStats("Risk",Pnl));
                    }));
                Task.Factory.StartNew(() => { EventSystem.Publish<ExecuteNext>(new ExecuteNext()); });
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "DisplayOptimizationResults");
            }
        }

        /// <summary>
        /// dump the results to file
        /// </summary>
        private void ExportGeneticAlgoReuslts()
        {
            try
            {
                string folderPath = string.Empty;
                IList<string> lines = null;
                using (FolderBrowserDialog fdb = new FolderBrowserDialog())
                {
                    if (fdb.ShowDialog() == DialogResult.OK)
                    {
                        folderPath = fdb.SelectedPath;
                    }
                }
                if (folderPath != string.Empty)
                {
                    lines = new List<string>();
                    lines.Add("Round,Property1,Property2,Property3,Property4,Risk");
                    int round = 1;
                    for (int i = 0; i < _parametersInfo.Count; i += 5)
                    {
                        string temp = string.Format("{0},{1},{2},{3},{4},{5}", round++, _parametersInfo[i].ParameterValue,
                            _parametersInfo[i + 1].ParameterValue, _parametersInfo[i + 2].ParameterValue,
                            _parametersInfo[i + 3].ParameterValue, _parametersInfo[i + 4].ParameterValue);
                        lines.Add(temp);
                    }
                    string path = folderPath + "\\" + "GA-Results.csv";
                    File.WriteAllLines(path, lines);
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "ExportGeneticAlgoReuslts");
            }
        }
    }
}
