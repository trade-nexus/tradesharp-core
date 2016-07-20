using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TradeHub.Common.Core.CustomAttributes;
using TradeHub.StrategyRunner.Infrastructure.Service;
using TradeHubConstants = TradeHub.Common.Core.Constants;

namespace TradeHub.StrategyRunner.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            //IList<string> namesOfFiles = new List<string>();

            //DateTime startTime = new DateTime(2013,8,25);
            //DateTime endTime = new DateTime(2013,8,30);

            //string specificFolder = @"C:\aurora\ATS\Saved Data";
            //string providerName = "InteractiveBrokers";

            //string[] directoryNames = new string[3];

            //// Get possible directory path for bars created with BID Price
            //directoryNames[0] = specificFolder + "\\" + providerName + "\\" + "AAPL" + "\\Bar\\" +
            //                       TradeHubConstants.BarFormat.TIME + "\\" + TradeHubConstants.BarPriceType.BID;
            //// Get possible directory path for bars created with ASK Price
            //directoryNames[1] = specificFolder + "\\" + providerName + "\\" + "AAPL" + "\\Bar\\" +
            //                       TradeHubConstants.BarFormat.TIME + "\\" + TradeHubConstants.BarPriceType.ASK;
            //// Get possible directory path for bars created with LAST Price
            //directoryNames[2] = specificFolder + "\\" + providerName + "\\" + "AAPL" + "\\Bar\\" +
            //                       TradeHubConstants.BarFormat.TIME + "\\" + TradeHubConstants.BarPriceType.LAST;

            //// Traverse all possible directories
            //foreach (string directoryName in directoryNames)
            //{
            //    var directory = new DirectoryInfo(directoryName);

            //    // Find required files if the path exists
            //    if (directory.Exists)
            //    {
            //        // Find all possible subfolders in the given directory
            //        IEnumerable<string> subFolders = directory.GetDirectories().Select(subDirectory => subDirectory.Name);

            //        // Use all sub-directories to find files with required info
            //        foreach (string subFolder in subFolders)
            //        {
            //            DateTime tempStartTime = new DateTime(startTime.Ticks);
            //            while (tempStartTime.Date <= endTime.Date)
            //            {
            //                var filename = tempStartTime.ToString("yyyyMMdd") + ".txt";

            //                // Get the File paths of required date.
            //                string[] path = Directory.GetFiles(directoryName + "\\" + subFolder,
            //                                                   filename, SearchOption.AllDirectories);

            //                if (path.Any())
            //                {
            //                    namesOfFiles.Add(path[0]);
            //                }
            //                tempStartTime = tempStartTime.AddDays(1);
            //            }
            //        }
            //    }
            //}

            //foreach (string namesOfFile in namesOfFiles)
            //{
            //    System.Console.WriteLine(namesOfFile);
            //}

            var strategyDetails = LoadCustomStrategy.GetConstructorDetails("TradeHub.StrategyRunner.SampleStrategy.dll");

            if (strategyDetails != null)
            {
                var strategyType = strategyDetails.Item1;
                var ctorDetails = strategyDetails.Item2;

                object[] ctrArgs = new object[ctorDetails.Length];

                foreach (ParameterInfo parameterInfo in ctorDetails)
                {
                    object value;
                    do
                    {
                        System.Console.WriteLine("Enter " + parameterInfo.ParameterType.Name + " value for: " +
                                                 parameterInfo.Name);
                        var input = System.Console.ReadLine();
                        value = LoadCustomStrategy.GetParametereValue(input, parameterInfo.ParameterType.Name);

                    } while (value == null);

                    ctrArgs[parameterInfo.Position] = value;
                }

                LoadCustomStrategy.CreateStrategyInstance(strategyType, ctrArgs);
            }

            while (true)
            {
                
            }
        }

        static T? Cast<T>(object obj) where T : struct
        {
            return obj as T?;
        }
    }
}
