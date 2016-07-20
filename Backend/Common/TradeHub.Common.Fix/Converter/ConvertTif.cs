using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeHub.Common.Core.Constants;
using TradeHub.Common.Fix.Constants;

namespace TradeHub.Common.Fix.Converter
{
    public static class ConvertTif
    {
        /// <summary>
        /// Take TradeHub TIF value and returns FIX variant
        /// </summary>
        /// <param name="tif"></param>
        /// <returns></returns>
        public static char GetFixValue(string tif)
        {
            switch (tif)
            {
                case OrderTif.DAY:
                    return TimeInForce.DAY;
                case OrderTif.GTC:
                    return TimeInForce.GTC;
                case OrderTif.GTD:
                    return TimeInForce.GTD;
                case OrderTif.GTX:
                    return TimeInForce.GTX;
                case OrderTif.IOC:
                    return TimeInForce.IOC;
                case OrderTif.OPG:
                    return TimeInForce.OPG;
                case OrderTif.FOK:
                    return TimeInForce.FOK;
                default:
                    return ' ';
            }
        }

        /// <summary>
        /// Take FIX TIF value and returns TradeHUB variant
        /// </summary>
        /// <param name="tif"></param>
        /// <returns></returns>
        public static string GetLocalValue(char tif)
        {
            switch (tif)
            {
                case TimeInForce.DAY:
                    return OrderTif.DAY;
                case TimeInForce.GTC:
                    return OrderTif.GTC;
                case TimeInForce.GTD:
                    return OrderTif.GTD;
                case TimeInForce.GTX:
                    return OrderTif.GTX;
                case TimeInForce.IOC:
                    return OrderTif.IOC;
                case TimeInForce.OPG:
                    return OrderTif.OPG;
                case TimeInForce.FOK:
                    return OrderTif.FOK;
                default:
                    return OrderTif.NONE;
            }
        }
    }
}
