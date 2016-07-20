using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using TraceSourceLogger;
using TradeHub.Common.Core.Constants;
using TradeHub.Common.Core.DomainModels;
using TradeHub.DataDownloader.Common.ConcreteImplementation;
using TradeHub.DataDownloader.Common.Interfaces;

namespace TradeHub.DataDownloader.CsvFileWriter
{
    /// <summary>
    /// This Class writes data to csv file.
    /// This class is capable of writing any object to 
    /// file which is inherited from MarketDataEvent class. 
    /// e.g Tick or Bar objects  
    /// </summary>
    public class FileWriterCsv : IWriter
    {
        //TODO: It would be better if we change IWriterCsv Interface to Abstract Class because CreateDirectoryPath methord wil remain same from tick and Bin writer 
        private static Type _oType = typeof(FileWriterCsv);
        private static string _specificFolder;

        /// <summary>
        /// Staic Constructor:
        /// Creats Path For DataDownloader Directory in AppData Folder.
        /// </summary>
        static FileWriterCsv()
        {
            try
            {
                // The folder for the roaming current user 
                string folder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

                // Combine the base folder with your specific folder....
                _specificFolder = Path.Combine(folder, "DataDownloader");

                // Check if folder exists and if not, create it
                if (!Directory.Exists(_specificFolder))
                    Directory.CreateDirectory(_specificFolder);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _oType.FullName, "FileWriterCsv[Static]");
            }
        }

        /// <summary>
        /// Writes Data to csv file.
        /// Main Purpose of this Methord is to 
        /// check that if data is Bar or Tick 
        /// and handle is Respectively
        /// </summary>
        /// <param name="dataObject">Bar Or Tick</param>
        public void Write(MarketDataEvent dataObject)
        {
            try
            {

                if (dataObject is HistoricBarData)
                {
                    WriteHistoricBarsToFile(dataObject as HistoricBarData);
                }
                else if (dataObject is Bar)
                {
                    WriteBarDataToCsvFile(dataObject as DetailBar);
                }
                else if (dataObject is Tick)
                {
                    var newTick = dataObject as Tick;
                    WriteTickToCsvFile(newTick);
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _oType.FullName, "WriteToFile");
            }
        }

        /// <summary>
        /// Save Historic Bars To Csv File
        /// The also check that no Data is repeated.
        /// </summary>
        /// <param name="historicBarData"></param>
        private void WriteHistoricBarsToFile(HistoricBarData historicBarData)
        {
            try
            {
                var barLength = Convert.ToInt32(historicBarData.Bars[1].DateTime.Subtract(historicBarData.Bars[0].DateTime).TotalSeconds);
                var date = historicBarData.Bars[0].DateTime.Date;
                var path = CreateDirectoryPathForHistoricBarObject(historicBarData.Bars[0].Security.Symbol,
                                               historicBarData.MarketDataProvider,barLength) + "//" +
                           historicBarData.Bars[0].DateTime.Date.ToString("yyyyMMdd") + ".txt";
                if (File.Exists(path))
                {
                    // Removes The File 
                    File.Delete(path);
                }
                foreach (Bar bar in historicBarData.Bars)
                {
                    if (date != bar.DateTime.Date)
                    {
                        path = CreateDirectoryPathForHistoricBarObject(bar.Security.Symbol,
                                                   historicBarData.MarketDataProvider,barLength) + "//" +
                               bar.DateTime.Date.ToString("yyyyMMdd") + ".txt";
                        if (File.Exists(path))
                        {
                            // Removes The File 
                            File.Delete(path);
                        }
                        date = bar.DateTime.Date;
                    }
                    try
                    {
                        StreamWriter sw = File.AppendText(path);
                        //Using Reflection to write data to file
                        sw.Write(bar.Close);
                        sw.Write(",");
                        sw.Write(bar.Open);
                        sw.Write(",");
                        sw.Write(bar.High);
                        sw.Write(",");
                        sw.Write(bar.Low);
                        sw.Write(",");
                        sw.Write(bar.Volume);
                        sw.Write(",");
                        sw.Write(bar.Security.Symbol);
                        sw.Write(",");
                        sw.Write(bar.DateTime);
                        sw.Write(",");
                        sw.Write(bar.MarketDataProvider);
                        sw.Write(Environment.NewLine);
                        sw.Close();
                    }
                    catch (Exception exception)
                    {
                        Logger.Error(exception, _oType.FullName, "WriteBarDataToFile");
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Error("Outer Exception" + exception, _oType.FullName, "WriteHistoricBarsToFile");
            }
        }

        /// <summary>
        /// Creates Missing Directories and place returns propper 
        /// path of file depending on dataType and symbol
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="dataType"> </param>
        /// <param name="dataProvider"> </param>
        /// <returns></returns>
        private string CreateDirectoryPath(string symbol, MarketDataType dataType, string dataProvider)
        {
            try
            {
                string[] path =
                    {
                        _specificFolder+"\\"+dataProvider,
                        _specificFolder+"\\"+dataProvider + "\\" + symbol,
                        _specificFolder+"\\"+dataProvider + "\\" + symbol + "\\" + dataType,
                        _specificFolder+"\\"+dataProvider + "\\" + symbol + "\\" + dataType + "\\" + DateTime.Now.Year.ToString(CultureInfo.InvariantCulture),
                        _specificFolder+"\\"+dataProvider + "\\" + symbol + "\\" + dataType + "\\" + DateTime.Now.Year.ToString(CultureInfo.InvariantCulture) + "\\" +DateTime.Now.Month.ToString(CultureInfo.InvariantCulture)
                    };

                foreach (string t in path)
                {
                    if (!Directory.Exists(t))
                    {
                        Directory.CreateDirectory(t);
                    }
                }
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info(path[path.Length - 1] + "\\" + DateTime.Now.ToString("yyyyMMdd"), _oType.FullName,
                                "CreateDirectoryPath");
                }
                return path[path.Length - 1];
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _oType.FullName, "CreateDirectoryPath");
                return null;
            }
        }

        /// <summary>
        /// Enters Tick Into File
        /// </summary>
        /// <param name="newTick"> </param>
        private void WriteTickToCsvFile(Tick newTick)
        {
            try
            {
                //Writing Quote to file
                if (newTick.HasAsk || newTick.HasBid)
                {
                    using (
                        StreamWriter sw =
                            File.AppendText(
                                CreateDirectoryPath(newTick.Security.Symbol, MarketDataType.Tick,
                                                    newTick.MarketDataProvider) + "\\" + DateTime.Now.ToString("yyyyMMdd") + "Quote.txt"))
                    {
                        sw.Write(newTick.Security.Symbol);
                        sw.Write(",");
                        sw.Write(newTick.DateTime);
                        sw.Write(",");
                        sw.Write(newTick.AskSize);
                        sw.Write(",");
                        sw.Write(newTick.AskPrice);
                        sw.Write(",");
                        sw.Write(newTick.BidPrice);
                        sw.Write(",");
                        sw.Write(newTick.BidSize);
                        sw.Write(",");
                        sw.Write(newTick.MarketDataProvider);
                        sw.Write(",");
                        sw.Write(Environment.NewLine);
                    }
                }

                //Writing Trade to File
                if (newTick.HasTrade)
                {
                    using (
                        StreamWriter sw =
                            File.AppendText(
                                CreateDirectoryPath(newTick.Security.Symbol, MarketDataType.Tick,
                                                    newTick.MarketDataProvider) + "\\" + DateTime.Now.ToString("yyyyMMdd") + "Trade.txt"))
                    {
                        sw.Write(newTick.Security.Symbol);
                        sw.Write(",");
                        sw.Write(newTick.DateTime);
                        sw.Write(",");
                        sw.Write(newTick.LastSize);
                        sw.Write(",");
                        sw.Write(newTick.LastSize);
                        sw.Write(",");
                        sw.Write(newTick.MarketDataProvider);
                        sw.Write(",");
                        sw.Write(Environment.NewLine);
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _oType.FullName, "WriteTickToCsvFile");
            }
        }

        /// <summary>
        /// Enters Bar Into File
        /// </summary>
        private void WriteBarDataToCsvFile(DetailBar newBar)
        {
            try
            {
                using (StreamWriter sw = File.AppendText(CreateDirectoryPathForBarObject(newBar.Security.Symbol, newBar, newBar.MarketDataProvider) + "\\" + newBar.DateTime.ToString("yyyyMMdd") + ".txt"))
                {
                    var bar = (Bar) newBar;
                    sw.Write(bar.Close);
                    sw.Write(",");
                    sw.Write(bar.Open);
                    sw.Write(",");
                    sw.Write(bar.High);
                    sw.Write(",");
                    sw.Write(bar.Low);
                    sw.Write(",");
                    sw.Write(bar.Volume);
                    sw.Write(",");
                    sw.Write(bar.Security.Symbol);
                    sw.Write(",");
                    sw.Write(bar.DateTime);
                    sw.Write(",");
                    sw.Write(bar.MarketDataProvider);
                    sw.Write(Environment.NewLine);
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _oType.FullName, "WriteBarDataToFile");
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
        private string CreateDirectoryPathForBarObject(string symbol, DetailBar detailBar, string dataProvider)
        {
            try
            {
                string[] directories =
                    {
                        _specificFolder+"\\"+dataProvider,
                        _specificFolder+"\\"+dataProvider + "\\" + symbol,
                        _specificFolder+"\\"+dataProvider + "\\" + symbol+"\\BAR",
                        _specificFolder+"\\"+dataProvider + "\\" + symbol+"\\BAR" + "\\" + detailBar.BarFormat,
                        _specificFolder+"\\"+dataProvider + "\\" + symbol+"\\BAR" + "\\" + detailBar.BarFormat+"\\"+detailBar.BarPriceType,
                        _specificFolder+"\\"+dataProvider + "\\" + symbol+"\\BAR" + "\\" + detailBar.BarFormat+"\\"+detailBar.BarPriceType+"\\"+detailBar.BarLength,
                        _specificFolder+"\\"+dataProvider + "\\" + symbol+"\\BAR" + "\\" + detailBar.BarFormat+"\\"+detailBar.BarPriceType+"\\"+detailBar.BarLength+"\\"+ DateTime.Now.Year.ToString(CultureInfo.InvariantCulture),
                        _specificFolder+"\\"+dataProvider + "\\" + symbol+"\\BAR" + "\\" + detailBar.BarFormat+"\\"+detailBar.BarPriceType+"\\"+detailBar.BarLength +"\\"+ DateTime.Now.Year.ToString(CultureInfo.InvariantCulture) + "\\" +DateTime.Now.Month.ToString(CultureInfo.InvariantCulture)
                    };

                foreach (string path in directories)
                {
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                }
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info(directories[directories.Length - 1] + "\\" + DateTime.Now.ToString("yyyyMMdd"), _oType.FullName,
                                "CreateDirectoryPath");
                }
                return directories[directories.Length - 1] ;
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _oType.FullName, "CreateDirectoryPath");
                return null;
            }
        }

        /// <summary>
        /// Creates Missing Directories and place returns proper 
        /// path of file depending on dataType and symbol
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="dataProvider"> </param>
        /// <param name="length"> </param>
        /// <returns></returns>
        private string CreateDirectoryPathForHistoricBarObject(string symbol, string dataProvider,int length)
        {
            try
            {
                string[] directories =
                    {
                        _specificFolder+"\\"+dataProvider,
                        _specificFolder+"\\"+dataProvider + "\\" + symbol,
                        _specificFolder+"\\"+dataProvider + "\\" + symbol+"\\BAR",
                        _specificFolder+"\\"+dataProvider + "\\" + symbol+"\\BAR" + "\\" + BarFormat.TIME,
                        _specificFolder+"\\"+dataProvider + "\\" + symbol+"\\BAR" + "\\" + BarFormat.TIME+"\\"+BarPriceType.LAST,
                        _specificFolder+"\\"+dataProvider + "\\" + symbol+"\\BAR" + "\\" + BarFormat.TIME+"\\"+BarPriceType.LAST+"\\"+length,
                        _specificFolder+"\\"+dataProvider + "\\" + symbol+"\\BAR" + "\\" + BarFormat.TIME+"\\"+BarPriceType.LAST+"\\"+length+"\\"+ DateTime.Now.Year.ToString(CultureInfo.InvariantCulture),
                        _specificFolder+"\\"+dataProvider + "\\" + symbol+"\\BAR" + "\\" + BarFormat.TIME+"\\"+BarPriceType.LAST+"\\"+length +"\\"+ DateTime.Now.Year.ToString(CultureInfo.InvariantCulture) + "\\" +DateTime.Now.Month.ToString(CultureInfo.InvariantCulture)
                    };

                foreach (string path in directories)
                {
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                }
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info(directories[directories.Length - 1] + "\\" + DateTime.Now.ToString("yyyyMMdd"), _oType.FullName,
                                "CreateDirectoryPath");
                }
                return directories[directories.Length - 1];
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _oType.FullName, "CreateDirectoryPath");
                return null;
            }
        }
    }

}
