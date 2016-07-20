using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeHub.StrategyRunner.Infrastructure.ValueObjects
{
    /// <summary>
    /// Contains info for the strategy to be added
    /// </summary>
    public class AddStrategy
    {
        /// <summary>
        /// Contains info for the strategy to be added
        /// </summary>
        private SelectedStrategy _selectedStrategy;

        /// <summary>
        /// Argument Constructor
        /// </summary>
        /// <param name="selectedStrategy">Contains info for the strategy to be added</param>
        public AddStrategy(SelectedStrategy selectedStrategy)
        {
            SelectedStrategy = selectedStrategy;
        }

        /// <summary>
        /// Contains info for the strategy to be added
        /// </summary>
        public SelectedStrategy SelectedStrategy
        {
            get { return _selectedStrategy; }
            set { _selectedStrategy = value; }
        }
    }
}
