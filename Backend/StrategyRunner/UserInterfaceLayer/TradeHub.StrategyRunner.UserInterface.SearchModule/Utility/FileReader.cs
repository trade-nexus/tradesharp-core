using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TraceSourceLogger;

namespace TradeHub.StrategyRunner.UserInterface.SearchModule.Utility
{
    /// <summary>
    /// Reads required info from the selected file
    /// </summary>
    public static class FileReader
    {
        /// <summary>
        /// Read parameters from the selected file
        /// </summary>
        /// <param name="file">File to be read</param>
        /// <returns></returns>
        public static List<string[]> ReadParameters(string file)
        {
            try
            {
                StreamReader streamReader = new StreamReader(file);

                // Save all the parameter sets defined in the list
                List<string[]> parametersList = new List<string[]>();

                // Holds single set of parameters
                string[] parameters = null;
                
                string input = streamReader.ReadLine();

                // Read values
                while((input = streamReader.ReadLine()) != null)
                {
                    // Split to get individual parameters
                    parameters = input.Split(',');

                    // Add to the list
                    parametersList.Add(parameters);
                }

                // return details
                return parametersList;
            }
            catch (Exception exception)
            {
                Logger.Error(exception, "TradeHub.StrategyRunner.UserInterface.SearchModule.Utility.FileReader", "ReadParameters");
                return null;
            }
        }
    }
}
