using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeHub.Common.Core.DomainModels.OrderDomain
{
    /// <summary>
    /// Contains PnL information
    /// </summary>
    public class ProfitLossStats
    {
        /// <summary>
        /// Overall Profit and Loss value
        /// </summary>
        private decimal _profitAndLoss = default(decimal);

        /// <summary>
        /// Trades responsible for the PnL value
        /// </summary>
        private IList<Trade> _trades;

        /// <summary>
        /// Overall Profit and Loss value
        /// </summary>
        public decimal ProfitAndLoss
        {
            get { return _profitAndLoss; }
        }

        /// <summary>
        /// Trades responsible for the PnL value
        /// </summary>
        public IList<Trade> Trades
        {
            get { return _trades; }
        }

        /// <summary>
        /// Argument Constructor
        /// </summary>
        /// <param name="trades">Trades responsible for the PnL value</param>
        public ProfitLossStats(IList<Trade> trades)
        {
            // Save Instance
            _trades = trades;

            // Do calculation on initialization
            CalculatePnl();
        }

        /// <summary>
        /// Calculates Profit and Loss value from the given Trades list
        /// </summary>
        private void CalculatePnl()
        {
            // Traverse each Trade
            foreach (var trade in _trades)
            {
                // Sum PnL for all the individual Trades
                _profitAndLoss += trade.ProfitAndLoss;
            }
        }
    }
}
