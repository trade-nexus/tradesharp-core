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
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.Practices.Prism.Commands;
using TraceSourceLogger;
using TradeHub.StrategyRunner.ApplicationController.Domain;
using TradeHub.StrategyRunner.Infrastructure.Service;
using TradeHub.StrategyRunner.Infrastructure.ValueObjects;
using TradeHub.StrategyRunner.UserInterface.Common;
using TradeHub.StrategyRunner.UserInterface.Common.ValueObjects;

namespace TradeHub.StrategyRunner.UserInterface.GaParametersModule.ViewModel
{
    /// <summary>
    /// Contains backend functionality for GaParametersView.xaml
    /// </summary>
    public class GaParametersViewModel : ViewModelBase
    {
        private Type _type = typeof (GaParametersViewModel);

        /// <summary>
        /// Holds reference to UI dispatcher
        /// </summary>
        private readonly Dispatcher _currentDispatcher;

        /// <summary>
        /// Contains info to identify genetic algorithm strategy functionality
        /// </summary>
        private string _gaStrategyInfo;

        /// <summary>
        /// Saves orignal set of ctor arguments
        /// </summary>
        private object[] _ctorArguments;

        /// <summary>
        /// Holds reference of user selected custom strategy
        /// </summary>
        private Type _strategyType;

        /// <summary>
        /// Contains parameters to be used for genetic optimization
        /// </summary>
        private ObservableCollection<GeneticAlgoParameters> _parameters;

        /// <summary>
        /// Command to start User Strategy optimization.
        /// </summary>
        public ICommand ExecuteGeneticAlgo { get; set; }
        
        /// <summary>
        /// Iterations of GA
        /// </summary>
        public int Iterations { get; set; }

        /// <summary>
        /// Population Size
        /// </summary>
        public int PopulationSize { get; set; }

        public int Rounds { get; set; }

        private int _currentRound = 0;

        private string _roundsCompleted;

        private Stopwatch _stopwatch;

        public string RoundsCompleted
        {
            get { return _roundsCompleted; }
            set
            {
                _roundsCompleted = value;
                RaisePropertyChanged("RoundsCompleted");
            }
        }

        /// <summary>
        /// Contains info to identify strategy functionality
        /// </summary>
        public string GaStrategyInfo
        {
            get { return _gaStrategyInfo; }
            set
            {
                _gaStrategyInfo = value;
                RaisePropertyChanged("GaStrategyInfo");
            }
        }

        /// <summary>
        /// Contains parameters to be used for genetic optimization
        /// </summary>
        public ObservableCollection<GeneticAlgoParameters> Parameters
        {
            get { return _parameters; }
            set { _parameters = value; }
        }

        /// <summary>
        /// Default Constructor
        /// </summary>
        public GaParametersViewModel()
        {
            // Initialize Dispatcher to be used for UI modifications
            _currentDispatcher = Dispatcher.CurrentDispatcher;

            // Initialize Collection to contains GA Parameters Info
            _parameters = new ObservableCollection<GeneticAlgoParameters>();

            // Initialzie Command
            ExecuteGeneticAlgo= new DelegateCommand(ExectueGeneticAlgorithm);

            //initiliaze default value for Iterations
            Iterations = 20;

            //initialize default value for Population Size
            PopulationSize = 45;

            //set round default value to 1
            Rounds = 1;
            //RoundsCompleted = "Status : Stopped";
            
            EventSystem.Subscribe<OptimizationParametersGeneticAlgo>(DisplayGeneticParameters);
            EventSystem.Subscribe<ExecuteNext>(ExecuteNextRound);
        }

        /// <summary>
        /// Execute next round of GA
        /// </summary>
        /// <param name="next"></param>
        private void ExecuteNextRound(ExecuteNext next)
        {
            //_stopwatch.Stop();
            //Logger.Info("Executed in " + _stopwatch.ElapsedMilliseconds + "ms", "", "");
            //ManualResetEvent resetEvent = new ManualResetEvent(false);
            //resetEvent.WaitOne(2000);
            _currentRound++;
            //Update rounds on UI
            _currentDispatcher.Invoke(DispatcherPriority.Normal, (Action)(() =>
            {
                //RoundsCompleted = "Rounds Completed:" + _currentRound + "/" + Rounds;
                RoundsCompleted = "Status : Running, " + _currentRound + "Rounds Completed";
            }));
            if (_currentRound < Rounds)
            {
                ExectueGeneticAlgorithm();
            }
            else
            {
                _currentRound = -1;
                _currentDispatcher.Invoke(DispatcherPriority.Normal, (Action)(() =>
                {
                    RoundsCompleted = "Status : Completed " + Rounds + " Rounds";
                }));
            }
        }

        /// <summary>
        /// Displays available parameters for genetic optimization
        /// </summary>
        private void DisplayGeneticParameters(OptimizationParametersGeneticAlgo optimizeStrategyGeneticAlgo)
        {
            try
            {
                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug("Displaying the strategy parameters", _type.FullName, "DisplaySelectedStrategy");
                }

                _parameters.Clear();

                // Get Strategy Info
                GaStrategyInfo = LoadCustomStrategy.GetCustomClassSummary(optimizeStrategyGeneticAlgo.StrategyType);

                // Save Ctor Arguments
                _ctorArguments = optimizeStrategyGeneticAlgo.CtorArguments;

                // Save Custom Strategy Type
                _strategyType = optimizeStrategyGeneticAlgo.StrategyType;
                int i=0;
                // Get all parameters
                foreach (var parameters in optimizeStrategyGeneticAlgo.GeneticAlgoParameters)
                {
                    double start = 0;
                    double end = 0;
                    
                    if (i == 0)
                    {
                        start = 1;
                        end = 5;
                    }

                    else if (i == 1)
                    {
                        start = 0.0001;
                        end = 0.011;
                    }

                    else if (i == 2)
                    {
                        start = 0.2 ;
                        end = 10;
                    }

                    else if (i == 3)
                    {
                        start = 0.002;
                        end = 0.01;
                    }
                    GeneticAlgoParameters parameterInfo = new GeneticAlgoParameters
                    {
                        Index = parameters.Key,
                        Description = parameters.Value.Item1,
                        StartValue = start,
                        EndValue = end,
                    };
                    i++;

                    // Update UI Element
                    _currentDispatcher.Invoke(DispatcherPriority.Normal, (Action)(() =>
                    {
                        // Add to collection
                        _parameters.Add(parameterInfo);
                    }));
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "DisplayGeneticParameters");
            }   
        }

        /// <summary>
        /// Executes genetic algorithm to optimize the strategy
        /// </summary>
        private void ExectueGeneticAlgorithm()
        {
            //_stopwatch = new Stopwatch();
            try
            {
                // Verfiy parameters before initiating optimization
                if (VerfiyOptimizationParameters())
                {
                    // Create new sorted dictionary
                    var optimzationParameters = new SortedDictionary<int, GeneticAlgoParameters>();

                    // Add available values to sorted dictionary
                    foreach (var parameter in _parameters)
                    {
                        optimzationParameters.Add(parameter.Index, parameter);
                        if (Logger.IsInfoEnabled)
                        {
                            Logger.Info(
                                "Input param, Discription=" + parameter.Description + ", StartValue=" +
                                parameter.StartValue + ",EndValue=" + parameter.EndValue, _type.FullName, "ExectueGeneticAlgorithm");
                        }
                    }
                    //_stopwatch.Start();
                    // Create new value object to be published
                    OptimizeStrategyGeneticAlgo optimizeStrategy = new OptimizeStrategyGeneticAlgo(_strategyType,
                        _ctorArguments,
                        optimzationParameters, Iterations, PopulationSize);
                    // Notify Listener to start execution
                    //Task.Factory.StartNew(()=>EventSystem.Publish<OptimizeStrategyGeneticAlgo>(optimizeStrategy));
                    EventSystem.Publish<OptimizeStrategyGeneticAlgo>(optimizeStrategy);
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "ExecuteGeneticAlgorithm");
            }
        }

        /// <summary>
        /// Verfifies the parameters provided for Genetic Optimization
        /// </summary>
        private bool VerfiyOptimizationParameters()
        {
            try
            {
                // Traverse all available parameters
                foreach (var parameter in _parameters)
                {
                    if (parameter.StartValue >= parameter.EndValue)
                    {
                        Logger.Info("Start and End values were not significantly appart.", _type.FullName, "VerfiyOptimizationParameters");
                        return false;
                    }
                    // Indicates that the parameters are verified
                    return true;
                }

                return false;
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "VerfiyOptimizationParameters");
                return false;
            }
        }
    }
}
