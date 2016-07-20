using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TraceSourceLogger;

namespace TradeHub.Common.Fix.Infrastructure
{
    public static class ReadFixSettingsFile
    {
        /// <summary>
        /// Reads the FIX settings text file and retrieves parameter values
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static Dictionary<string, string> GetSettings(string filePath)
        {
            try
            {
                var settings = new Dictionary<string, string>();

                // Open settings file
                using (StreamReader streamReader = new StreamReader(filePath)) 
                {
                    string line = streamReader.ReadLine();
                    while (line != null)
                    {
                        // Only process valid setting lines
                        if (!(line.Equals(String.Empty) || line.StartsWith("#") || line.StartsWith("[")))
                        {
                            var values = line.Split('=');
                            if (values.Length.Equals(2))
                            {
                                settings.Add(values[0], values[1]);
                            }
                        }

                        // Read next line
                        line = streamReader.ReadLine();
                    }

                    // Remove buffered data
                    streamReader.DiscardBufferedData();  
                    // Close stream
                    streamReader.Close(); // CLOSE THE readIt.Readlin
                }

                return settings;
            } 
            catch (Exception exception)
            {
                Logger.Error(exception, "TradeHub.Common.Fix.Infrastructure.ReadFixSettingsFile", "exception");
                return null;
            }
        }
    }
}
