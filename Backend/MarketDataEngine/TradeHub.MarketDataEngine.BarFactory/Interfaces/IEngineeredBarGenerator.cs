using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeHub.MarketDataEngine.BarFactory.Interfaces
{
    /// <summary>
    /// Interface to be implemented by Engineered Bar Generators
    /// </summary>
    public interface IEngineeredBarGenerator : IBarGenerator
    {
        #region Properties
        
        /// <summary>
        /// Get pip size
        /// </summary>
        decimal PipSize { get; }

        /// <summary>
        /// Get number of pips
        /// </summary>
        decimal NumberOfPips { get; }

        #endregion
    }
}
