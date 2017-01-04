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
using System.Collections.ObjectModel;
using System.ComponentModel;
using TradeHub.StrategyEngine.Utlility.Services;
using TradeSharp.UI.Common.Constants;
using TradeSharp.UI.Common.ValueObjects;

namespace TradeSharp.UI.Common.Models
{
    public class BruteForceParameters : INotifyPropertyChanged
    {
        /// <summary>
        /// Total number of possible iterations for given parameters
        /// </summary>
        private int _totalIterations = 0;

        /// <summary>
        /// Number of completed iterations
        /// </summary>
        private int _completedIterations = 0;

        /// <summary>
        /// Number of remaining iterations
        /// </summary>
        private int _remainingIterations = 0;

        /// <summary>
        /// Indicates brute force working status
        /// </summary>
        private OptimizationStatus _status;

        /// <summary>
        /// Strategy Type containing TradeHubStrategy
        /// </summary>
        private Type _strategyType;

        /// <summary>
        /// Contains detailed information to be used while running Brute Force optimization
        /// </summary>
        private ObservableCollection<BruteForceParameterDetail> _parameterDetails;

        #region Properties

        /// <summary>
        /// Strategy Type containing TradeHubStrategy
        /// </summary>
        public Type StrategyType
        {
            get { return _strategyType; }
            set
            {
                if (_strategyType != value)
                {
                    _strategyType = value;
                    OnPropertyChanged("StrategyType");
                }
            }
        }

        /// <summary>
        /// Contains detailed information to be used while running Brute Force optimization
        /// </summary>
        public ObservableCollection<BruteForceParameterDetail> ParameterDetails
        {
            get { return _parameterDetails; }
            set
            {
                _parameterDetails = value;
                OnPropertyChanged("ParameterDetails");
            }
        }

        /// <summary>
        /// Indicates brute force working status
        /// </summary>
        public OptimizationStatus Status
        {
            get { return _status; }
            set
            {
                _status = value;
                OnPropertyChanged("Status");
            }
        }

        /// <summary>
        /// Total number of possible iterations for given parameters
        /// </summary>
        public int TotalIterations
        {
            get { return _totalIterations; }
            set
            {
                _totalIterations = value;
                OnPropertyChanged("TotalIterations");
            }
        }

        /// <summary>
        /// Number of completed iterations
        /// </summary>
        public int CompletedIterations
        {
            get { return _completedIterations; }
            set
            {
                _completedIterations = value;
                OnPropertyChanged("CompletedIterations");
            }
        }

        /// <summary>
        /// Number of remaining iterations
        /// </summary>
        public int RemainingIterations
        {
            get { return _remainingIterations; }
            set
            {
                _remainingIterations = value;
                OnPropertyChanged("RemainingIterations");
            }
        }

        #endregion

        /// <summary>
        /// Argument Constructor
        /// </summary>
        /// <param name="strategyType">Strategy Type containing TradeHubStrategy</param>
        public BruteForceParameters(Type strategyType)
        {
            _status = OptimizationStatus.None;
            _strategyType = strategyType;
            _parameterDetails = new ObservableCollection<BruteForceParameterDetail>();
        }

        /// <summary>
        /// Returns Initial Parameter values
        /// </summary>
        /// <returns></returns>
        public object[] GetParameterValues()
        {
            int entryCount = 0;

            object[] parameterValues = new object[ParameterDetails.Count];

            // Traverse all parameter
            foreach (BruteForceParameterDetail iteratorVariable in ParameterDetails)
            {
                // Makes sure all parameters are in right format
                var input = StrategyHelper.GetParametereValue(iteratorVariable.ParameterValue.ToString(), iteratorVariable.ParameterType.Name);

                // Add actual parameter values to the new object list
                parameterValues[entryCount++] = input;
            }

            return parameterValues;
        }

        /// <summary>
        /// Returns array of parameter which will be used to make different iterations
        /// </summary>
        /// <returns></returns>
        public Tuple<int, object, double>[] GetConditionalParameters()
        {
            int index = 0;

            // Create a list to hold all optimization parameters
            var optimizationParameters = new List<Tuple<int, object, double>>();

            // Read info from all parameters
            foreach (var parameterDetail in ParameterDetails)
            {
                // Check if both End Point and Increment values are added
                if (parameterDetail.ParameterValue != null && !(parameterDetail.ParameterValue.ToString().Equals(parameterDetail.EndValue.ToString())))
                {
                    if (parameterDetail.Increment > 0)
                    {
                        // Add parameter info
                        optimizationParameters.Add(new Tuple<int, object, double>(index,
                                                                                  parameterDetail.EndValue,
                                                                                  parameterDetail.Increment));
                    }
                }

                index++;
            }

            return optimizationParameters.ToArray();
        }

        #region INotifyPropertyChanged members

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }
}
