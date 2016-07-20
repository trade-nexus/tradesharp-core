using System;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using TradeHub.Common.Core.CustomAttributes;
using TradeHub.StrategyEngine.Testing.SimpleStrategy.EMA;
using TradeHub.StrategyEngine.Testing.SimpleStrategy.Utility;
using TradeHubConstants = TradeHub.Common.Core.Constants;

namespace TradeHub.StrategyEngine.Testing.SimpleStrategy
{
    class Program
    {
        static void Main(string[] args)
        {
            //var temp = DateTime.ParseExact("2/09/2013", "d/MM/yyyy", CultureInfo.InvariantCulture);

            EmaStrategy _strategy;
            bool initialized = false;
            string response = string.Empty;

            // Get TradeHub-Attributes
            Type classType = typeof(EmaStrategy);

            ConsoleWriter.WriteLine(ConsoleColor.Blue, "TradeHub custom attributes used are");

            foreach (PropertyInfo field in classType.GetProperties())
            {
                foreach (Attribute attr in field.GetCustomAttributes(true))
                {
                    var tradeHubAtt = attr as TradeHubAttributes;
                    if (tradeHubAtt != null)
                    {
                        string description = tradeHubAtt.Description;
                        Type type = tradeHubAtt.Value;

                        ConsoleWriter.WriteLine(ConsoleColor.DarkCyan, "Description: " + description);
                        ConsoleWriter.WriteLine(ConsoleColor.DarkCyan, "Type: " + type);
                        ConsoleWriter.WriteLine(ConsoleColor.DarkCyan, "");
                    }
                }
            }

            // Get TradeHub-Attributes

            //do
            //{
            ConsoleWriter.WriteLine(ConsoleColor.Green, "Enter name of market data provider to be used");
            response = ConsoleWriter.Prompt();
            if (!string.IsNullOrEmpty(response) || string.IsNullOrWhiteSpace(response))
            {
                string marketDataProvider = response;
                ConsoleWriter.WriteLine(ConsoleColor.Green, "Enter name of order execution provider to be used");
                response = ConsoleWriter.Prompt();
                if (!string.IsNullOrEmpty(response) || string.IsNullOrWhiteSpace(response))
                {
                    string orderExecutionProvider = response;
                    if (!initialized)
                    {
                        _strategy = new EmaStrategy(1, 2, Constants.EmaPriceType.HIGH, "MSFT" , 60,
                                                    TradeHubConstants.BarFormat.TIME,
                                                    TradeHubConstants.BarPriceType.LAST,
                                                    marketDataProvider, orderExecutionProvider);
                        initialized = true;

                        //_strategy.Dispose();
                    }
                }
            }
            //} while (response.ToLower().Equals("exit"));
        }
    }
}
