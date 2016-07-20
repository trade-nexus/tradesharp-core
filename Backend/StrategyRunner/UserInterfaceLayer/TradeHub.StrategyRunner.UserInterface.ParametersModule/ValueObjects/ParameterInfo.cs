using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeHub.StrategyRunner.UserInterface.ParametersModule.ValueObjects
{
    /// <summary>
    /// Contains info for individual ctor arguments parameters
    /// </summary>
    public class ParameterInfo
    {
        /// <summary>
        /// Name of the parameter
        /// </summary>
        private string _parameter;

        /// <summary>
        /// Parameter value
        /// </summary>
        private string _value;

        /// <summary>
        /// Location in Ctor arguments array
        /// </summary>
        private int _index;

        /// <summary>
        /// End Point for the range defined if the parameter is to be used in optimization iterations
        /// </summary>
        private string _endPoint;

        /// <summary>
        /// Increment value to be used to get the to end point starting from the actual value
        /// </summary>
        private string _increment;

        /// <summary>
        /// Default Constructor
        /// </summary>
        public ParameterInfo()
        {
            _parameter = string.Empty;
            _value = string.Empty;
            _index = 0;
            _endPoint = string.Empty;
            _increment = string.Empty;
        }

        /// <summary>
        /// Name of the parameter
        /// </summary>
        public string Parameter
        {
            get { return _parameter; }
            set { _parameter = value; }
        }

        /// <summary>
        /// Parameter value
        /// </summary>
        public string Value
        {
            get { return _value; }
            set { _value = value; }
        }

        /// <summary>
        /// Location in Ctor arguments array
        /// </summary>
        public int Index
        {
            get { return _index; }
            set { _index = value; }
        }

        /// <summary>
        /// End Point for the range defined if the parameter is to be used in optimization iterations
        /// </summary>
        public string EndPoint
        {
            get { return _endPoint; }
            set { _endPoint = value; }
        }

        /// <summary>
        /// Increment value to be used to get the to end point starting from the actual value
        /// </summary>
        public string Increment
        {
            get { return _increment; }
            set { _increment = value; }
        }
    }
}
