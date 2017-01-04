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
using TradeSharp.UI.Common.Utility;
using TradeSharp.UI.Common.ValueObjects;

namespace TradeSharp.UI.Common.Models
{
    /// <summary>
    /// Contains Strategy basic information
    /// </summary>
	public class Strategy
    {
        /// <summary>
        /// Unique to distinguish Strategy 
        /// </summary>
        private string _key;

        /// <summary>
        /// Strategy Name to display
        /// </summary>
        private string _name;

        /// <summary>
        /// Strategy Type extracted from Assembly
        /// </summary>
        private Type _strategyType;

        /// <summary>
        /// Name of the '.dll' from which the Strategy is loaded
        /// </summary>
        private string _fileName;

        /// <summary>
        /// Contains Parameter details to be used by Strategy
        /// Key = Parameter Name
        /// Value = Parameter Type (e.g. Int32, String, Decimal, etc.) , Parameter Value if entered
        /// </summary>
        private Dictionary<string, ParameterDetail> _parameterDetails; 

        /// <summary>
        /// Contains all strategy instances for the current strategy
        /// </summary>
        private IDictionary<string, StrategyInstance> _strategyInstances;

        /// <summary>
        /// Contains strategy statistics for all the instances
        /// </summary>
        private ObservableCollection<StrategyStatistics> _strategyStatistics; 

        #region Properties

        /// <summary>
        /// Unique to distinguish Strategy 
        /// </summary>
        public string Key
        {
            get { return _key; }
            set { _key = value; }
        }

        /// <summary>
        /// Strategy Name to display
        /// </summary>
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        /// <summary>
        /// Contains all strategy instances for the current strategy
        /// </summary>
        public IDictionary<string, StrategyInstance> StrategyInstances
        {
            get { return _strategyInstances; }
            set { _strategyInstances = value; }
        }

        /// <summary>
        /// Strategy Type extracted from Assembly
        /// </summary>
        public Type StrategyType
        {
            get { return _strategyType; }
            set { _strategyType = value; }
        }

        /// <summary>
        /// Contains Parameter details to be used by Strategy
        /// Key = Parameter Name
        /// Value = Parameter Type (e.g. Int32, String, Decimal, etc.)
        /// </summary>
        public Dictionary<string, ParameterDetail> ParameterDetails
        {
            get { return _parameterDetails; }
            set { _parameterDetails = value; }
        }

        /// <summary>
        /// Name of the '.dll' from which the Strategy is loaded
        /// </summary>
        public string FileName
        {
            get { return _fileName; }
            set { _fileName = value; }
        }

        /// <summary>
        /// Contains strategy statistics for all the instances
        /// </summary>
        public ObservableCollection<StrategyStatistics> Statistics
        {
            get { return _strategyStatistics; }
            set { _strategyStatistics = value; }
        }

        #endregion

        /// <summary>
        /// Argument Constructor
        /// </summary>
        /// <param name="name">Strategy Name</param>
        /// <param name="strategyType">Strategy Assembly Type</param>
        /// <param name="fileName">Name of the file from which the Straegy is loaded</param>
        public Strategy(string name, Type strategyType, string fileName)
        {
            // Get new strategy ID
            _key = StrategyIdGenerator.GetStrategyKey();

            // Save information
            _name = name;
            _fileName = fileName;
            _strategyType = strategyType;

            // Initialize fields
            _parameterDetails = new Dictionary<string, ParameterDetail>();
            _strategyInstances = new Dictionary<string, StrategyInstance>();
            _strategyStatistics = new ObservableCollection<StrategyStatistics>();

            // Subscribe Domain Events
            EventSystem.Subscribe<StrategyStatistics>(UpdateStrategyStatistics);
        }

        /// <summary>
        /// Creates a new Strategy Instance object
        /// </summary>
        /// <param name="parameters">Parameter list to be used by the instance for execution</param>
        /// <param name="description">Instance description</param>
        public StrategyInstance CreateInstance(Dictionary<string, ParameterDetail> parameters, string description)
        {
            // Get new Instance Key
            string instanceKey = StrategyIdGenerator.GetInstanceKey(_key);

            // Create new Strategy Instance Object
            var strategyInstance = new StrategyInstance(instanceKey, parameters, _strategyType)
            {
                Description = description
            };

            // Add to local MAP
            _strategyInstances.Add(instanceKey, strategyInstance);

            // Return Instance
            return strategyInstance;
        }

        /// <summary>
        /// Update strategy statistics collection
        /// </summary>
        /// <param name="strategyStatistics"></param>
        private void UpdateStrategyStatistics(StrategyStatistics strategyStatistics)
        {
            if (strategyStatistics.InstanceId.Split('-')[0].Contains(Key))
            {
                Statistics.Insert(0, strategyStatistics);
            }
        }


        /// <summary>
        /// Removes existing Strategy Instance from the local Map
        /// </summary>
        /// <param name="instanceKey">Unique key of Strategy Instance object to be removed</param>
        public void RemoveInstance(string instanceKey)
        {
            // Add to local MAP
            _strategyInstances.Remove(instanceKey);
        }

        /// <summary>
        /// Clears strategy statistics collection
        /// </summary>
        public void ClearStatistics()
        {
            Statistics.Clear();
        }
    }
}
