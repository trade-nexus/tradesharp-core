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
using TradeHub.Common.Core.DomainModels;
using TradeHub.StrategyEngine.Utlility.Services;
using TradeSharp.UI.Common.ValueObjects;

namespace TradeSharp.UI.Common.Models
{
    /// <summary>
    /// Contains Individual Strategy Instance information
    /// </summary>
    public class StrategyInstance : INotifyPropertyChanged
    {
        #region Fields

        /// <summary>
        /// Unique Key to identify instance
        /// </summary>
        private string _instanceKey;

        /// <summary>
        /// Symbol of instrument
        /// </summary>
        private string _symbol;

        /// <summary>
        /// Brief Strategy Description
        /// </summary>
        private string _description;

        /// <summary>
        /// Summary of events/information as instance executes (depends on USER)
        /// </summary>
        private ObservableCollection<string> _instanceSummary;

        /// <summary>
        /// Contains Parameter details to be used by Strategy
        /// Key = Parameter Name
        /// Value = Parameter Type (e.g. Int32, String, Decimal, etc.) , Parameter Value if entered
        /// </summary>
        private Dictionary<string, ParameterDetail> _parameters; 

        /// <summary>
        /// Strategy Type containing TradeHubStrategy
        /// </summary>
        private Type _strategyType;

        /// <summary>
        /// Current Execution Status of Stratgey Instance i.e. 'None' | 'Executing' | 'Executed'
        /// </summary>
        private StrategyStatus _status = StrategyStatus.None;

        /// <summary>
        /// Holds basic execution information for the current instance to be used for UI
        /// </summary>
        private StrategyExecutionDetails _executionDetails;

        /// <summary>
        /// Indicates if the instance is selected by user
        /// </summary>
        private bool _isSelected;

        #endregion

        #region Properties

        /// <summary>
        /// Unique Key to identify instance
        /// </summary>
        public string InstanceKey
        {
            get { return _instanceKey; }
            set
            {
                if (_instanceKey != value)
                {
                    _instanceKey = value;
                    OnPropertyChanged("InstanceKey");
                }
            }
        }

        /// <summary>
        /// Symbol of instrument
        /// </summary>
        public string Symbol
        {
            get { return _symbol; }
            set
            {
                if (_symbol != value)
                {
                    _symbol = value;
                    OnPropertyChanged("Symbol");
                }
            }
        }

        /// <summary>
        /// Brief Strategy Description
        /// </summary>
        public string Description
        {
            get { return _description; }
            set
            {
                if (_description != value)
                {
                    _description = value;
                    OnPropertyChanged("Description");
                }
            }
        }

        /// <summary>
        /// Summary of events/information as instance executes (depends on USER)
        /// </summary>
        public ObservableCollection<string> InstanceSummary
        {
            get { return _instanceSummary; }
            set
            {
                if (InstanceSummary != value)
                {
                    _instanceSummary = value;
                    OnPropertyChanged("InstanceSummary");
                }
            }
        }

        /// <summary>
        /// Contains Parameter details to be used by Strategy
        /// Key = Parameter Name
        /// Value = Parameter Type (e.g. Int32, String, Decimal, etc.) , Parameter Value if entered
        /// </summary>
        public Dictionary<string, ParameterDetail> Parameters
        {
            get { return _parameters; }
            set
            {
                if (_parameters != value)
                {
                    _parameters = value;
                    OnPropertyChanged("Parameters");
                }
            }
        }

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
        /// Current Execution Status of Stratgey Instance i.e. 'None' | 'Executing' | 'Executed'
        /// </summary>
        public StrategyStatus Status
        {
            get { return _status; }
            set
            {
                if (_status != value)
                {
                    _status = value;
                    OnPropertyChanged("Status");
                }
            }
        }

        /// <summary>
        /// Holds basic execution information for the current instance to be used for UI
        /// </summary>
        public StrategyExecutionDetails ExecutionDetails
        {
            get { return _executionDetails; }
            set
            {
                if (_executionDetails != value)
                {
                    _executionDetails = value;
                    OnPropertyChanged("ExecutionDetails");
                }
            }
        }

        /// <summary>
        /// Indicates if the instance is selected by user
        /// </summary>
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged("IsSelected");
                }
            }
        }

        #endregion

        public StrategyInstance()
        {
            // Initialize
            _instanceSummary = new ObservableCollection<string>();
            _executionDetails = new StrategyExecutionDetails();
        }

        /// <summary>
        /// Argument Constructor
        /// </summary>
        /// <param name="instanceKey">Unique Key to identify instance</param>
        /// <param name="parameters">Contains Parameter details to be used by Strategy</param>
        /// <param name="strategyType">Strategy Type containing TradeHubStrategy</param>
        public StrategyInstance(string instanceKey, Dictionary<string, ParameterDetail> parameters, Type strategyType)
        {
            // Initialize
            _instanceSummary = new ObservableCollection<string>();
            _executionDetails = new StrategyExecutionDetails();

            // Save information
            _instanceKey = instanceKey;
            _parameters = parameters;
            _strategyType = strategyType;

            // Use Instance Key to identify its execution information
            _executionDetails.Key = _instanceKey;
        }

        /// <summary>
        /// Adds new order details to the local map
        /// </summary>
        /// <param name="orderDetails"></param>
        public void AddOrderDetails(OrderDetails orderDetails)
        {
            _executionDetails.AddOrderDetails(orderDetails);
        }

        /// <summary>
        /// Returns IList of actual parameter values from the Parameter Details object
        /// </summary>
        /// <returns></returns>
        public object[] GetParameterValues()
        {
            int entryCount = 0;

            object[] parameterValues = new object[Parameters.Count];

            // Traverse all parameter
            foreach (KeyValuePair<string, ParameterDetail> keyValuePair in Parameters)
            {
                // Makes sure all parameters are in right format
                var input = StrategyHelper.GetParametereValue(keyValuePair.Value.ParameterValue.ToString(), keyValuePair.Value.ParameterType.Name);

                // Add actual parameter values to the new object list
                parameterValues[entryCount++] = input;
            }

            return parameterValues;
        }

        /// <summary>
        /// Verifies if the incoming parameters arrays contains any parameter which has different from the existing parameters
        /// </summary>
        /// <param name="parametersArray">Array of parameters to check for difference</param>
        /// <returns></returns>
        public bool ParametersChanged(object[] parametersArray)
        {
            var existingParameters = GetParameterValues();

            for (int i = 0; i < existingParameters.Length; i++)
            {
                if (! existingParameters[i].GetHashCode().Equals(parametersArray[i].GetHashCode()))
                {
                    // A different Parameter value is found
                    return true;
                }
            }

            return false;
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
