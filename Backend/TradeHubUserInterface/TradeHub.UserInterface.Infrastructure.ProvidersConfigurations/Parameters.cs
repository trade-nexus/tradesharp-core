using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeHub.UserInterface.Infrastructure.ProvidersConfigurations
{
    /// <summary>
    /// Value Object for passing provider parameters from XML files
    /// </summary>
    public class Parameters
    {
        private string _parameterName;


        /// <summary>
        /// Read only properties
        /// </summary>
        public string ParameterValue
        {
            get { return _parameterValue; }
            set { _parameterValue = value; }
        }

        public string ParameterName
        {
            get { return _parameterName; }
            set { _parameterName = value; }
        }

        private string _parameterValue;

        /// <summary>
        /// Parameterized Constructor
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public Parameters(string name, string value)
        {
            _parameterName = name;
            _parameterValue = value;

        }

    }
}
