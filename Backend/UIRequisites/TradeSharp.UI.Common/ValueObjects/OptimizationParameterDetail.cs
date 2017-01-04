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
using System.ComponentModel;

namespace TradeSharp.UI.Common.ValueObjects
{
    /// <summary>
    /// Contains Parameter details to be used for Genetic Algorithm Optimization
    /// </summary>
    public class OptimizationParameterDetail : INotifyPropertyChanged
    {
        /// <summary>
        /// Index of the parameter in discussion
        /// </summary>
        private int _index;

        /// <summary>
        /// Description of the Parameter
        /// </summary>
        private string _description;

        /// <summary>
        /// Start point of parameter range
        /// </summary>
        private double _startValue;

        /// <summary>
        /// End point of parameter range
        /// </summary>
        private double _endValue;

        /// <summary>
        /// System Type of the parameter e.g. Int32, Double, etc
        /// </summary>
        private Type _parameterType;

        /// <summary>
        /// Optimized Parameter Value after Optimization Process
        /// </summary>
        private double _optimizedValue;

        /// <summary>
        /// Default Constructor
        /// </summary>
        public OptimizationParameterDetail()
        {
            _index = default(int);
            _description = string.Empty;
            _startValue = default(double);
            _endValue = default(double);
            _optimizedValue = default(double);
            _parameterType = null;
        }

        /// <summary>
        /// Index of the parameter in discussion
        /// </summary>
        public int Index
        {
            get { return _index; }
            set
            {
                _index = value;
                OnPropertyChanged("Index");
            }
        }

        /// <summary>
        /// Description of the Parameter
        /// </summary>
        public string Description
        {
            get { return _description; }
            set
            {
                _description = value;
                OnPropertyChanged("Description");
            }
        }

        /// <summary>
        /// Start point of parameter range
        /// </summary>
        public double StartValue
        {
            get { return _startValue; }
            set
            {
                _startValue = value;
                OnPropertyChanged("StartValue");
            }
        }

        /// <summary>
        /// End point of parameter range
        /// </summary>
        public double EndValue
        {
            get { return _endValue; }
            set
            {
                _endValue = value;
                OnPropertyChanged("EndValue");
            }
        }

        /// <summary>
        /// System Type of the parameter e.g. Int32, Double, etc
        /// </summary>
        public Type ParameterType
        {
            get { return _parameterType; }
            set { _parameterType = value; }
        }

        /// <summary>
        /// Optimized Parameter Value after Optimization Process
        /// </summary>
        public double OptimizedValue
        {
            get { return _optimizedValue; }
            set
            {
                _optimizedValue = value;
                OnPropertyChanged("OptimizedValue");
            }
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
