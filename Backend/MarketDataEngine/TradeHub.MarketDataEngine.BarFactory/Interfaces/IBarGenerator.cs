using System;
using TradeHub.Common.Core.DomainModels;

namespace TradeHub.MarketDataEngine.BarFactory.Interfaces
{
    /// <summary>
    /// Interface to be implemented by different Bar Generators
    /// </summary>
    public interface IBarGenerator
    {
        /// <summary>
        /// Price type to build bar: Bid, Ask, Last
        /// </summary>
        string BarPriceType { get; set; }

        /// <summary>
        /// Unique Key to Identify the Bars Produced
        /// </summary>
        string BarGeneratorKey { get; set; }

        /// <summary>
        /// Security used.
        /// </summary>
        Security Security { get; }

        #region Events

        /// <summary>
        /// Fired each time when bar is created
        /// </summary>
        event Action<Bar, string> BarArrived;
        
        #endregion

        #region Methods
        
        /// <summary>
        /// Updates OHLC values given new tick.
        /// </summary>
        /// <param name="tick"></param>
        void Update(Tick tick);
        
        #endregion

    }
}
