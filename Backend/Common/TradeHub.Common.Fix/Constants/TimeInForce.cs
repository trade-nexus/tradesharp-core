using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeHub.Common.Fix.Constants
{
    /// <summary>
    /// Time in force / Order instruction type
    /// </summary>
    public static class TimeInForce
    {
        /// <summary>
        /// Day Order
        /// </summary>
        public const char DAY = '0';
        /// <summary>
        /// Good Till Canceled
        /// </summary>
        public const char GTC = '1';
        /// <summary>
        /// Opening Order
        /// </summary>
        public const char OPG = '2';
        /// <summary>
        /// Immediate or Cancel
        /// </summary>
        public const char IOC = '3';
        /// <summary>
        /// Fill or Kill
        /// </summary>
        public const char FOK = '4';
        /// <summary>
        /// Good till Crossing
        /// </summary>
        public const char GTX = '5';
        /// <summary>
        /// Good till Date
        /// </summary>
        public const char GTD = '6';
    }
}
