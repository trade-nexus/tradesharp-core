using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeHub.StrategyRunner.Infrastructure.Utility
{
    public static class ObjectInitializer
    {
        public static object CastObject(string input, string type)
        {
            switch (type)
            {
                case "UInt32": // uint
                    UInt32 outputUInt32;
                    if (UInt32.TryParse(input, out outputUInt32))
                        return outputUInt32;
                    else
                        return null;

                case "Int32": // int
                    Int32 output32;
                    if(Int32.TryParse(input, out output32))
                        return output32;
                    else
                        return null;

                case "UInt16": // ushort
                    UInt16 outputUInt16;
                    if (UInt16.TryParse(input, out outputUInt16))
                        return outputUInt16;
                    else
                        return null;

                case "Int16": // short
                    Int16 outputInt16;
                    if (Int16.TryParse(input, out outputInt16))
                        return outputInt16;
                    else
                        return null;

                case "UInt64": //ulong
                    UInt64 outputUInt64;
                    if (UInt64.TryParse(input, out outputUInt64))
                        return outputUInt64;
                    else
                        return null;

                case "Int64": // long
                    Int64 output64;
                    if (Int64.TryParse(input, out output64))
                        return output64;
                    else
                        return null;

                case "Decimal": // decimal
                    decimal output;
                    if (decimal.TryParse(input, out output))
                        return output;
                    else
                        return null;

                case "Single": // Float
                    Single outputSingle;
                    if (float.TryParse(input, out outputSingle))
                        return outputSingle;
                    else
                        return null;

                case "Char": // char
                    return Convert.ToChar(input);

                case "String": // string
                    return input;
                default:
                    return null;
            }
        }
    }
}
