using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TraceSourceLogger;
using TradeHub.DataConverter.Common;

namespace TradeHub.DataConverter.EsignalToDataDownloader
{
    /// <summary>
    /// Writes converted data into files
    /// </summary>
    public static class FileWriter
    {
        private static Type _type = typeof (FileWriter);

        /// <summary>
        /// Writes given data into required files
        /// </summary>
        /// <param name="data">Data to be writen in file</param>
        /// <param name="symbol">Symbol</param>
        /// <param name="newBar">Contains Bar info</param>
        /// <param name="provider">name of market data provider to be used</param>
        public static void WriteData(string data, string symbol, DetailBar newBar, string provider = "Blackwood")
        {
            try
            {
                using (StreamWriter sw = File.AppendText(CreateDirectoryPathForBarObject(symbol, newBar, provider) + "\\" + newBar.DateTime.ToString("yyyyMMdd") + ".txt"))
                {
                    sw.Write(data);
                    sw.Write(Environment.NewLine);
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "WriteData");
            }
        }


        /// <summary>
        /// Creates Missing Directories and place returns proper 
        /// path of file depending on dataType and symbol
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="detailBar"> </param>
        /// <param name="dataProvider"> </param>
        /// <returns></returns>
        private static string CreateDirectoryPathForBarObject(string symbol, DetailBar detailBar, string dataProvider)
        {
            try
            {
                // The folder for the roaming current user 
                string folder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

                // Combine the base folder with your specific folder....
                string specificFolder = Path.Combine(folder, "DataDownloader");

                string[] directories =
                    {
                        specificFolder+"\\"+dataProvider,
                        specificFolder+"\\"+dataProvider + "\\" + symbol,
                        specificFolder+"\\"+dataProvider + "\\" + symbol+"\\BAR",
                        specificFolder+"\\"+dataProvider + "\\" + symbol+"\\BAR" + "\\" + detailBar.BarFormat,
                        specificFolder+"\\"+dataProvider + "\\" + symbol+"\\BAR" + "\\" + detailBar.BarFormat+"\\"+detailBar.BarPriceType,
                        specificFolder+"\\"+dataProvider + "\\" + symbol+"\\BAR" + "\\" + detailBar.BarFormat+"\\"+detailBar.BarPriceType+"\\"+detailBar.BarLength,
                        specificFolder+"\\"+dataProvider + "\\" + symbol+"\\BAR" + "\\" + detailBar.BarFormat+"\\"+detailBar.BarPriceType+"\\"+detailBar.BarLength+"\\"+ DateTime.Now.Year.ToString(CultureInfo.InvariantCulture),
                        specificFolder+"\\"+dataProvider + "\\" + symbol+"\\BAR" + "\\" + detailBar.BarFormat+"\\"+detailBar.BarPriceType+"\\"+detailBar.BarLength +"\\"+ DateTime.Now.Year.ToString(CultureInfo.InvariantCulture) + "\\" +DateTime.Now.Month.ToString(CultureInfo.InvariantCulture)
                    };

                foreach (string path in directories)
                {
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                }
                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug(directories[directories.Length - 1] + "\\" + DateTime.Now.ToString("yyyyMMdd"), _type.FullName,
                                "CreateDirectoryPath");
                }
                return directories[directories.Length - 1];
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "CreateDirectoryPath");
                return null;
            }
        }

    }
}
