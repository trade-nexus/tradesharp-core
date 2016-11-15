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
using System.Globalization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using TraceSourceLogger;
using TradeHub.Common.Core.Constants;
using TradeHub.Common.Core.DomainModels;
using TradeHub.DataDownloader.Common.ConcreteImplementation;
using TradeHub.DataDownloader.Common.Interfaces;

namespace TradeHub.DataDownloader.BinaryFileWriter
{
    /// <summary>
    /// This Class writes data to Binany file.
    /// This class is capable of writing any object to 
    /// file which is inherited from MarketDataEvent class. 
    /// e.g Tick or Bar objects
    /// </summary>
    public class FileWriterBinany : IWriter
    {
        private static Type _oType = typeof(FileWriterBinany);
        private static string _specificFolder;

        /// <summary>
        /// Staic Constructor:
        /// Creats Path For DataDownloader Directory in AppData Folder.
        /// </summary>
        static FileWriterBinany()
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
                Logger.Error(exception, _oType.FullName, "FileWriterBinany[Static]");
            }
        }

        /// <summary>
        /// Write Data to file
        /// </summary>
        /// <param name="dataObject"> Tick Or Bar Object</param>
        public void Write(MarketDataEvent dataObject)
        {
            try
            {
                //TODO: Safty Cast
                if (dataObject is DetailBar)
                {
                    //TODO : NULL Reference Check For CreateDirectoryPath
                    var newBar = (DetailBar)dataObject;
                    using (var fileStream = new FileStream(CreateDirectoryPathForBarObject(newBar.Security.Symbol, newBar, newBar.MarketDataProvider) + ".obj", FileMode.Append))
                    {
                        var bFormatter = new BinaryFormatter();
                        bFormatter.Serialize(fileStream, newBar);
                        fileStream.Close();
                    }
                }
                else if (dataObject is Tick)
                {
                    var newTick = (Tick)dataObject;
                    using (var fileStream = new FileStream(CreateDirectoryPath(newTick.Security.Symbol, MarketDataType.Tick, newTick.MarketDataProvider)+".obj", FileMode.Append))
                    {
                        var bFormatter = new BinaryFormatter();
                        bFormatter.Serialize(fileStream, newTick);
                        fileStream.Close();
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _oType.FullName, "Write");
            }
        }

        /// <summary>
        /// Creates Missing Directories and place returns proper 
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
                string[] directories =
                    {
                        _specificFolder + "\\" + dataProvider,
                        _specificFolder + "\\" + dataProvider + "\\" + symbol,
                        _specificFolder + "\\" + dataProvider + "\\" + symbol + "\\" + dataType,
                        _specificFolder + "\\" + dataProvider + "\\" + symbol + "\\" + dataType + "\\" +DateTime.Now.Year.ToString(CultureInfo.InvariantCulture),
                        _specificFolder + "\\" + dataProvider + "\\" + symbol + "\\" + dataType + "\\" +DateTime.Now.Year.ToString(CultureInfo.InvariantCulture) + "\\" + DateTime.Now.Month.ToString(CultureInfo.InvariantCulture)
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
                    Logger.Info(directories[directories.Length - 1] + "\\" + DateTime.Now.ToString("yyyyMMdd"),
                                _oType.FullName,
                                "CreateDirectoryPath");
                }
                return directories[directories.Length - 1] + "\\" + DateTime.Now.ToString("yyyyMMdd");
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
                return directories[directories.Length - 1] + "\\" + detailBar.DateTime.ToString("yyyyMMdd");
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _oType.FullName, "CreateDirectoryPath");
                return null;
            }
        }
    }
}
