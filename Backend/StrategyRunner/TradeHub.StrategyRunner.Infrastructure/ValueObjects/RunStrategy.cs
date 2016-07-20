using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeHub.StrategyRunner.Infrastructure.ValueObjects
{
    /// <summary>
    /// Contains info the execute the selected specified strategy
    /// </summary>
    public class RunStrategy
    {
        /// <summary>
        /// Unique key identifing the saved strategy instance
        /// </summary>
        private string _key;

        /// <summary>
        /// Unique key identifing the saved strategy instance
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
        public RunStrategy(string key)
        {
            _key = key;
        }
    }
}
