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
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Unity;
using TraceSourceLogger;
using TradeHub.StrategyRunner.Infrastructure.ValueObjects;
using TradeHub.StrategyRunner.UserInterface.Common;
using TradeHub.StrategyRunner.UserInterface.SearchModule.Utility;

namespace TradeHub.StrategyRunner.UserInterface.SearchModule.ViewModel
{
    /// <summary>
    /// Contains Strategy Constructor View Functionality
    /// </summary>
    public class ConstructorViewModel : ViewModelBase
    {
        private Type _type = typeof(ConstructorViewModel);

        /// <summary>
        /// Command to Browse Strategy Constructor parameters
        /// </summary>
        public ICommand BrowseStrategyParametersCommand { get; set; }

        /// <summary>
        /// Default Constructor
        /// </summary>
        public ConstructorViewModel()
        {
            BrowseStrategyParametersCommand = new DelegateCommand(BrowseStrategyParameters);
        }

        /// <summary>
        /// Tries to initialize the User Selected Strategy
        /// </summary>
        /// <param name="index">Selected Index</param>
        public void RunStrategy(int index)
        {
            try
            {
                // Create new object to pass to event aggregator
                VerfiyParameters verfiyParameters= new VerfiyParameters(index);
                
                // Publish event to notify listeners
                EventSystem.Publish<VerfiyParameters>(verfiyParameters);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "RunStrategy");
            }
        }

        /// <summary>
        /// Tries to initialize the User Selected Strategy
        /// </summary>
        /// <param name="ctorArgs">Constructor Arguments to be verified</param>
        public void RunStrategy(object[] ctorArgs)
        {
            try
            {
                // Create new object to pass to event aggregator
                VerfiyParameters verfiyParameters = new VerfiyParameters(-1);

                // Add arguments to be verified
                verfiyParameters.CtrArgs = ctorArgs;

                // Publish event to notify listeners
                EventSystem.Publish<VerfiyParameters>(verfiyParameters);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "RunStrategy");
            }
        }

        /// <summary>
        /// Tries to run optimizations on the User Selected Strategy using Brute Force
        /// </summary>
        /// <param name="index">Selected Index</param>
        public void OptimizeStrategyBruteForce(int index)
        {
            try
            {
                // Create new object to pass to event aggregator
                VerfiyParameters verfiyParameters = new VerfiyParameters(index);

                // Publish event to notify listeners
                EventSystem.Publish<VerfiyParameters>(verfiyParameters);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "OptimizeStrategyBruteForce");
            }
        }

        /// <summary>
        /// Tries to run optimizations on the User Selected Strategy using Genetic Algorithm
        /// </summary>
        /// <param name="ctorArgs">Constructor Arguments to be verified</param>
        public void OptimizeStrategyGeneticAlgorithm(object[] ctorArgs)
        {
            try
            {
                // Create new object to pass to event aggregator
                VerfiyParameters verfiyParameters = new VerfiyParameters(-2);

                // Add arguments to be verified
                verfiyParameters.CtrArgs = ctorArgs;

                // Publish event to notify listeners
                EventSystem.Publish<VerfiyParameters>(verfiyParameters);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "OptimizeStrategyGeneticAlgorithm");
            }
        }

        /// <summary>
        /// Open dialog to select file containing custom strategy constuctor parameters
        /// </summary>
        private void BrowseStrategyParameters()
        {
            try
            {
                IUnityContainer container = new UnityContainer();

                // Open File Dialog Service to browse file
                var fileDialogService = container.Resolve<FileDialogService>();

                if (fileDialogService.OpenFileDialog(".csv", "Comma Separated Values File") == true)
                {
                    if (Logger.IsDebugEnabled)
                    {
                        Logger.Debug("File select browser opened.", _type.FullName, "BrowseStrategyParameters");
                    }

                    var parametersList = FileReader.ReadParameters(fileDialogService.FileName);

                    if (parametersList.Count > 0)
                    {
                        // Publish Event to notify Listeners.
                        EventSystem.Publish<List<string[]>>(parametersList);
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "BrowseStrategyParameters");
            } 
        }
    }
}
