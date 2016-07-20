using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TradeHub.StrategyEngine.OrderExecution.Utility
{
    /// <summary>
    /// Generates Unique IDs to be assigned to New Orders
    /// </summary>
    internal static class OrderIdGenerator
    {
        private const int Min = 0xA000;
        private const int Max = 0xFFF9;
        private static int _value = Min - 1;
        private static int _staticAppender = 0;

        /// <summary>
        /// Returns Unique ID to be used in Order.OrderID field
        /// </summary>
        /// <param name="appId">Service App ID to be appended</param>
        /// <returns></returns>
        public static string GetId(string appId)
        {
            // Check if a valid appender is returned
            if (string.IsNullOrEmpty(appId) || string.IsNullOrWhiteSpace(appId))
            {
                // Get Local value to Append
                appId = GetLocalValueToAppend();
            }

            // Create Date Time and Appender value combination
            return DateTime.Now.ToString("yyMMddHmsfff") +Interlocked.Increment(ref _staticAppender) + appId;
        }

        /// <summary>
        /// Return Local Unique Appender for the current session
        /// </summary>
        /// <returns></returns>
        private static string GetLocalValueToAppend()
        {
            if (_value < Max)
            {
                _value++;
            }
            else
            {
                _value = Min;
            }

            // Convert-to-String and Return
            return _value.ToString("X");
        }
    }
}
