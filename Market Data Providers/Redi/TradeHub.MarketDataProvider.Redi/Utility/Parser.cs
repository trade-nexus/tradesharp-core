using System;
using TraceSourceLogger;

namespace TradeHub.MarketDataProvider.Redi.Utility
{
    public static class Parser
    {
        public static string StringValue(string message)
        {
            try
            {
                string temp = message.TrimStart('|');
                int start = temp.IndexOf("=", StringComparison.Ordinal);
                int end = temp.IndexOf("|", StringComparison.Ordinal);
                string result = temp.Substring(start + 1, end - start - 1);
                return result.Trim();
            }
            catch (Exception exception)
            {
                Logger.Error(exception+"|Error At:"+message, "Parser.StringValue", "StringValue");
                return null;
            }
        }
        public static decimal DecimalValue(string message)
        {
            string temp = message.TrimStart('|');
            int start = temp.IndexOf("=", StringComparison.Ordinal);
            int end = temp.IndexOf("|", StringComparison.Ordinal);
            string result = temp.Substring(start + 1, end - start - 1);
            return Convert.ToDecimal(result.Trim());
        }
        public static string RegularExp(string tag)
        {
            return "(\\|" + tag + "=\\w+\\.?(\\w+)?\\|)";
        }
    }
}
