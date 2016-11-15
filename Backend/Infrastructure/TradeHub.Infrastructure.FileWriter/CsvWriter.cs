/***************************************************************************** 
* Copyright 2016 Aurora Solutions 
* 
*    http://www.aurorasolutions.io 
* 
* Aurora Solutions is an innovative services and product company at 
* the forefront of the software industry, with processes and practices 
* involving Domain Driven Design(DDD), Agile methodologies to build 
* scalable, secure, reliable and high performance products.
* 
* TradeSharp is a C# based data feed and broker neutral Algorithmic 
* Trading Platform that lets trading firms or individuals automate 
* any rules based trading strategies in stocks, forex and ETFs. 
* TradeSharp allows users to connect to providers like Tradier Brokerage, 
* IQFeed, FXCM, Blackwood, Forexware, Integral, HotSpot, Currenex, 
* Interactive Brokers and more. 
* Key features: Place and Manage Orders, Risk Management, 
* Generate Customized Reports etc 
* 
* Licensed under the Apache License, Version 2.0 (the "License"); 
* you may not use this file except in compliance with the License. 
* You may obtain a copy of the License at 
* 
*    http://www.apache.org/licenses/LICENSE-2.0 
* 
* Unless required by applicable law or agreed to in writing, software 
* distributed under the License is distributed on an "AS IS" BASIS, 
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
* See the License for the specific language governing permissions and 
* limitations under the License. 
*****************************************************************************/


﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TraceSourceLogger;
using TradeHub.Common.Core.Constants;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Infrastructure.FileWriter.Interface;

namespace TradeHub.Infrastructure.FileWriter
{
    /// <summary>
    /// This Class writes data to csv file.
    /// This class is capable of writing any object to 
    /// file which is inherited from MarketDataEvent class. 
    /// e.g Tick or Bar objects  
    /// </summary>
    public class CsvWriter : IWriter
    {
        //TODO: It would be better if we change IWriterCsv Interface to Abstract Class because CreateDirectoryPath methord wil remain same from tick and Bin writer 
        private static Type _oType = typeof(CsvWriter);
        private static string _specificFolder;

        /// <summary>
        /// Staic Constructor:
        /// Creats Path For DataDownloader Directory in AppData Folder.
        /// </summary>
        static CsvWriter()
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
        /// Writes Tick Data to csv file.
        /// </summary>
        /// <param name="tick"></param>
        public void Write(Tick tick)
        {
            try
            {
                WriteTickToCsvFile(tick);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _oType.FullName, "Write - Tick");
            }
        }

        /// <summary>
        /// Writes Bar Data to csv file.
        /// </summary>
        /// <param name="bar"></param>
        /// <param name="barFormat"></param>
        /// <param name="barPriceType"></param>
        /// <param name="barLength"></param>
        public void Write(Bar bar, string barFormat, string barPriceType, string barLength)
        {
            try
            {
                WriteBarDataToCsvFile(bar, barFormat, barPriceType, barLength);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _oType.FullName, "Write - Bar");
            }
        }

        /// <summary>
        /// Writes Historical Bar Data to csv file.
        /// </summary>
        /// <param name="historicBarData"></param>
        public void Write(HistoricBarData historicBarData)
        {
            try
            {
                WriteHistoricBarsToFile(historicBarData);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _oType.FullName, "Write - Historic");
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
                        sw.Write(newTick.DateTime.ToString("M/d/yyyy h:mm:ss tt"));
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
                        sw.Write(newTick.DateTime.ToString("M/d/yyyy h:mm:ss tt"));
                        sw.Write(",");
                        sw.Write(newTick.LastPrice);
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
        private void WriteBarDataToCsvFile(Bar newBar, string barFormat, string barPriceType, string barLength)
        {
            try
            {
                using (
                    StreamWriter sw =
                        File.AppendText(
                            CreateDirectoryPathForBarObject(newBar.Security.Symbol, barFormat,barPriceType, barLength,newBar.MarketDataProvider) +
                            "\\" + newBar.DateTime.ToString("yyyyMMdd") + ".txt"))
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
                    sw.Write(bar.DateTime.ToString("M/d/yyyy h:mm:ss tt"));
                    sw.Write(",");
                    sw.Write(bar.MarketDataProvider);
                    sw.Write(Environment.NewLine);
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _oType.FullName, "WriteBarDataToCsvFile");
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
                int barLength = 60;

                // Retrieve orignal bar information for bar length
                if (historicBarData.BarsInformation != null)
                {
                    barLength = (int) historicBarData.BarsInformation.Interval;
                }
                // Manually calculate bar length
                else
                {
                    barLength =
                        Convert.ToInt32(
                            historicBarData.Bars[1].DateTime.Subtract(historicBarData.Bars[0].DateTime).TotalSeconds);
                }

                var date = historicBarData.Bars[0].DateTime.Date;
                var path = CreateDirectoryPathForHistoricBarObject(
                                    historicBarData.Bars[0].Security.Symbol,
                                    historicBarData.MarketDataProvider, barLength) + "//" +
                                    historicBarData.Bars[0].DateTime.Date.ToString("yyyyMMdd") + ".txt";

                if (File.Exists(path))
                {
                    // Removes The File 
                    File.Delete(path);
                }

                // Traverse all bars
                foreach (Bar bar in historicBarData.Bars)
                {
                    if (date != bar.DateTime.Date)
                    {
                        path = CreateDirectoryPathForHistoricBarObject(bar.Security.Symbol,
                                                   historicBarData.MarketDataProvider, barLength) + "//" +
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
                        sw.Write(bar.DateTime.ToString("M/d/yyyy h:mm:ss tt"));
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
        /// Creates Missing Directories and place returns proper 
        /// path of file depending on dataType and symbol
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="barFormat"></param>
        /// <param name="barPriceType"></param>
        /// <param name="barLength"></param>
        /// <param name="dataProvider"> </param>
        /// <returns></returns>
        private string CreateDirectoryPathForBarObject(string symbol, string barFormat, string barPriceType, string barLength, string dataProvider)
        {
            try
            {
                string[] directories =
                    {
                        _specificFolder+"\\"+dataProvider,
                        _specificFolder+"\\"+dataProvider + "\\" + symbol,
                        _specificFolder+"\\"+dataProvider + "\\" + symbol+"\\BAR",
                        _specificFolder+"\\"+dataProvider + "\\" + symbol+"\\BAR" + "\\" + barFormat,
                        _specificFolder+"\\"+dataProvider + "\\" + symbol+"\\BAR" + "\\" + barFormat+"\\"+barPriceType,
                        _specificFolder+"\\"+dataProvider + "\\" + symbol+"\\BAR" + "\\" + barFormat+"\\"+barPriceType+"\\"+barLength,
                        _specificFolder+"\\"+dataProvider + "\\" + symbol+"\\BAR" + "\\" + barFormat+"\\"+barPriceType+"\\"+barLength+"\\"+ DateTime.Now.Year.ToString(CultureInfo.InvariantCulture),
                        _specificFolder+"\\"+dataProvider + "\\" + symbol+"\\BAR" + "\\" + barFormat+"\\"+barPriceType+"\\"+barLength +"\\"+ DateTime.Now.Year.ToString(CultureInfo.InvariantCulture) + "\\" +DateTime.Now.Month.ToString(CultureInfo.InvariantCulture)
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
                                "CreateDirectoryPathForBarObject");
                }
                return directories[directories.Length - 1];
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _oType.FullName, "CreateDirectoryPathForBarObject");
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
        private string CreateDirectoryPathForHistoricBarObject(string symbol, string dataProvider, int length)
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
                                "CreateDirectoryPathForHistoricBarObject");
                }
                return directories[directories.Length - 1];
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _oType.FullName, "CreateDirectoryPathForHistoricBarObject");
                return null;
            }
        }
    }
}
