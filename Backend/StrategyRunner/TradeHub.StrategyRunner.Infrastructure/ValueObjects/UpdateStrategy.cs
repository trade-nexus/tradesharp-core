using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeHub.StrategyRunner.Infrastructure.ValueObjects
{
    public class UpdateStrategy
    {
        /// <summary>
        /// Unique Key to identify strategy instance
        /// </summary>
        private string _strategyKey;

        /// <summary>
        /// Indicates whether the strategy is running/stopped
        /// </summary>
        private bool _isRunning;

        /// <summary>
        /// Unique Key to identify strategy instance
        /// </summary>
        public string StrategyKey
        {
            get { return _strategyKey; }
            set { _strategyKey = value; }
        }

        /// <summary>
        /// Indicates whether the strategy is running/stopped
        /// </summary>
        public bool IsRunning
        {
            get { return _isRunning; }
            set { _isRunning = value; }
        }

        /// <summary>
        /// Argument Constructor
        /// </summary>
        /// <param name="strategyKey">Unique Key to identify strategy instance</param>
        /// <param name="isRunning">Indicates whether the strategy is running/stopped</param>
        public UpdateStrategy(string strategyKey, bool isRunning)
        {
            _strategyKey = strategyKey;
            _isRunning = isRunning;
        }
    }
}
