using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeHub.Common.Core.Constants;
using TradeHub.Common.Fix.Constants;

namespace TradeHub.Common.Fix.Converter
{
    public static class ConvertOrderSide
    {
        /// <summary>
        /// Takes FIX order side and returns TradeHUB order side
        /// </summary>
        /// <param name="side"></param>
        /// <returns></returns>
        public static string GetLocalOrderSide(char side)
        {
            switch (side)
            {
                case FixOrderSide.Buy:
                    return OrderSide.BUY;
                case FixOrderSide.Sell:
                    return OrderSide.SELL;
                case FixOrderSide.SellShort:
                    return OrderSide.SHORT;
                default:
                    return OrderSide.NONE;
            }
        }
    }
}
