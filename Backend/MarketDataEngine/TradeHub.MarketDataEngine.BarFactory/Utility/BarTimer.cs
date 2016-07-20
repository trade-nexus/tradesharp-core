using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace TradeHub.MarketDataEngine.BarFactory.Utility
{
    /// <summary>
    /// Used for Bar Generation
    /// </summary>
    public class BarTimer : Timer
    {
        public BarTimer(double interval)
            : base(interval)
        {
        }

        public DateTime TimeStampSecond { get; set; }
    }
}
