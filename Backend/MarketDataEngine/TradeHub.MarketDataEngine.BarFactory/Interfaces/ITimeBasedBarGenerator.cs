using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeHub.MarketDataEngine.BarFactory.Interfaces
{
    /// <summary>
    /// Interface to be implemented by Time Based Bar Generators
    /// </summary>
    public interface ITimeBasedBarGenerator : IBarGenerator
    {
        #region Methods
        
        /// <summary>
        /// To dispose the timer which posts bar.
        /// </summary>
        void DisposeTimer();
        
        #endregion

        #region Properties
        
        /// <summary>
        /// Get bar window length in seconds.
        /// </summary>
        int BarWindowLengthInSeconds { get; }

        #endregion
    }
}
