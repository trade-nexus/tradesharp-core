using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeHub.StrategyRunner.Infrastructure.ValueObjects
{
    /// <summary>
    /// Contains info to stop the specfied strategy execution
    /// </summary>
    public class StopStrategy
    {
        /// <summary>
        /// Unique Key to identify the strategy instance
        /// </summary>
        private string _key;

        /// <summary>
        /// Unique Key to identify the strategy instance
        /// </summary>
        public string Key
        {
            get { return _key; }
            set { _key = value; }
        }

        /// <summary>
        /// Argument Constructor
        /// </summary>
        /// <param name="key">Unique key to identify the strategy instance</param>
        public StopStrategy(string key)
        {
            _key = key;
        }
    }
}
