using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeHub.Common.Core.CustomAttributes
{
    /// <summary>
    /// Contains custom attributes to be used in User Strategies
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
    public class TradeHubAttributes : Attribute
    {
        private int _index;
        private string _description;
        private Type _value;

        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }

        public Type Value
        {
            get { return _value; }
            set { _value = value; }
        }

        public int Index
        {
            get { return _index; }
            set { _index = value; }
        }

        /// <summary>
        /// Argument Constructor
        /// </summary>
        /// <param name="description">Property definition</param>
        /// <param name="value">Property value to be used</param>
        /// <param name="index">Index to be used for properties</param>
        public TradeHubAttributes(string description, Type value, int index=0)
        {
            _description = description;
            _value = value;
            _index = index;
        }
    }
}
