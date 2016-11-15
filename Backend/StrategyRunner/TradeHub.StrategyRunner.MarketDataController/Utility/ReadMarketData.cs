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
using TraceSourceLogger;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.ValueObjects.MarketData;

namespace TradeHub.StrategyRunner.MarketDataController.Utility
{
    /// <summary>
    /// Main Purpose of the Class is read historic data 
    /// It could be Bar or Tick.
    /// </summary>
    public class ReadMarketData
    {
        private static Type _type = typeof(ReadMarketData);

        private readonly AsyncClassLogger _classLogger;
        private static string _specificFolder;
        
        // Total tick read
        private int _totalTicks = 0;

        // Gets Total tick read
        public int TotalTicks
        {
            get { return _totalTicks; }
        }

        /// <summary>
        /// Argument Constructor
        /// </summary>
        /// <param name="classLogger"></param>
        public ReadMarketData(AsyncClassLogger classLogger)
        {
            try
            {
                _classLogger = classLogger;
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
                _classLogger.Error(exception, _type.FullName, "ReadMarketData");
            }
        }

        public virtual IEnumerable<Bar> ReadBars(DateTime startTime,DateTime endTime , string providerName,BarDataRequest request )
        {
            IList<string> listOfFileNames = ReturnExistingFileName(startTime, endTime, providerName, request);

            if(_classLogger.IsDebugEnabled)
            {
                _classLogger.Debug("Number of files read: " + listOfFileNames.Count, _type.FullName, "LiveBars - ReadBars");
            }

            //Read Bar from File And Return all Selected File.
            return from name in listOfFileNames from line in ReadLineByLine(name) select ParseToBar(line,request.Id);
        }

        public virtual IEnumerable<Bar> ReadBars(DateTime startTime, DateTime endTime, string providerName, Subscribe subscribe)
        {
            IList<string> listOfFileNames = ReturnExistingFileName(startTime, endTime, providerName, subscribe);

            _classLogger.Info("Number of files read: " + listOfFileNames.Count, _type.FullName, "Ticks - ReadBars");

            foreach (string fileName in listOfFileNames)
            {
                _totalTicks += TickDataCount(fileName);
            }

            //Read Bar from File And Return all Selected File.
            return from name in listOfFileNames from line in ReadLineByLine(name) select ParseToBar(line, "");
        }

        public virtual IEnumerable<Bar> ReadBars(string providerName, HistoricDataRequest historicDataRequest)
        {
            IList<string> listOfFileNames = ReturnExistingFileName(providerName, historicDataRequest);

            _classLogger.Info("Number of files read: " + listOfFileNames.Count, _type.FullName, "Historic - ReadBars");

            //Read Bar from File And Return all Selected File.
            return from name in listOfFileNames from line in ReadLineByLine(name) select ParseToBar(line, historicDataRequest.Id);
        }

        /// <summary>
        /// Parse String into bar
        /// </summary>
        /// <param name="line"></param>
        /// <param name="id"> </param>
        /// <returns></returns>
        private Bar ParseToBar(string line, string id)
        {
            try
            {
                string[] feilds = line.Split(',');
                Bar newBar = new Bar(new Security {Symbol = feilds[5]}, Common.Core.Constants.MarketDataProvider.SimulatedExchange, id);
                newBar.Close = Convert.ToDecimal(feilds[0]);
                newBar.Open = Convert.ToDecimal(feilds[1]);
                newBar.High = Convert.ToDecimal(feilds[2]);
                newBar.Low = Convert.ToDecimal(feilds[3]);
                newBar.Volume = (long) Convert.ToDouble(feilds[4]);
                //newBar.DateTime = Convert.ToDateTime(feilds[6]);
                newBar.DateTime = DateTime.ParseExact(feilds[6], "M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture);
                //newBar.DateTime = DateTime.Now;
                return newBar;
            }
            catch (Exception exception)
            {
                _classLogger.Error(exception, _type.FullName, "ParseToBar");
                return null;
            }
        }

        /// <summary>
        /// This Methord Returns the name of all the file containing data 
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="providerName"> </param>
        /// <param name="symbol"> </param>
        /// <returns></returns>
        private IList<string> ReturnExistingFileName(DateTime startTime, DateTime endTime,string providerName,BarDataRequest symbol)
        {
            try
            {
                IList<string> namesOfFiles=new List<string>();
                while (startTime.Date<=endTime.Date)
                {
                    var filename = startTime.ToString("yyyyMMdd") + ".txt";
                    
                    // Get the File paths of required date.
                    string[] path =
                        Directory.GetFiles(
                            _specificFolder + "\\" + providerName + "\\" + symbol.Security.Symbol + "\\Bar\\" +
                            symbol.BarFormat + "\\" + symbol.BarPriceType + "\\" + Convert.ToInt32(symbol.BarLength),
                            filename, SearchOption.AllDirectories);
                    
                    if (path.Any())
                    {
                        if (_classLogger.IsInfoEnabled)
                        {
                            _classLogger.Info("Trying to read data from " + path[0], _type.FullName, "ReturnExistingFileName");
                        }
                        namesOfFiles.Add(path[0]);
                    }
                    startTime=startTime.AddDays(1);
                }
                return namesOfFiles;
            }
            catch (Exception exception)
            {
                _classLogger.Error(exception, _type.FullName, "ReturnExistingFileName");
                return null;
            }
        }

        /// <summary>
        /// This Methord Returns the name of all the file containing data 
        /// </summary>
        /// <param name="startTime">Strating Time</param>
        /// <param name="endTime">Ending Time </param>
        /// <param name="providerName">Name of Market Data Provider</param>
        /// <param name="symbol"> Symbol for which to fetch data</param>
        /// <returns></returns>
        private IList<string> ReturnExistingFileName(DateTime startTime, DateTime endTime, string providerName, Subscribe symbol)
        {
            try
            {
                IList<string> namesOfFiles = new List<string>();

                string[] directoryNames = new string[1];

                //// Get possible directory path for bars created with BID Price
                //directoryNames[0] = _specificFolder + "\\" + providerName + "\\" + symbol.Security.Symbol + "\\Bar\\" +
                //                       TradeHubConstants.BarFormat.TIME + "\\" + TradeHubConstants.BarPriceType.BID;
                //// Get possible directory path for bars created with ASK Price
                //directoryNames[1] = _specificFolder + "\\" + providerName + "\\" + symbol.Security.Symbol + "\\Bar\\" +
                //                       TradeHubConstants.BarFormat.TIME + "\\" + TradeHubConstants.BarPriceType.ASK;
                // Get possible directory path for bars created with LAST Price
                directoryNames[0] = _specificFolder + "\\" + providerName + "\\" + symbol.Security.Symbol + "\\Bar\\" +
                                    Common.Core.Constants.BarFormat.TIME + "\\" + Common.Core.Constants.BarPriceType.LAST +
                                    "\\" + "60";

                // Traverse all possible directories
                foreach (string directoryName in directoryNames)
                {
                    var directory = new DirectoryInfo(directoryName);

                    // Find required files if the path exists
                    if (directory.Exists)
                    {
                        // Find all possible subfolders in the given directory
                        IEnumerable<string> subFolders = directory.GetDirectories().Select(subDirectory => subDirectory.Name);

                        // Use all sub-directories to find files with required info
                        foreach (string subFolder in subFolders)
                        {
                            DateTime tempStartTime = new DateTime(startTime.Ticks);
                            while (tempStartTime.Date <= endTime.Date)
                            {
                                var filename = tempStartTime.ToString("yyyyMMdd") + ".txt";

                                // Get the File paths of required date.
                                string[] path = Directory.GetFiles(directoryName + "\\" + subFolder,
                                                                   filename, SearchOption.AllDirectories);

                                if (path.Any())
                                {
                                    if (_classLogger.IsInfoEnabled)
                                    {
                                        _classLogger.Info("Trying to read data from " + path[0], _type.FullName, "ReturnExistingFileName");
                                    }
                                    namesOfFiles.Add(path[0]);
                                }
                                tempStartTime = tempStartTime.AddDays(1);
                            }
                        }
                    }
                }

                return namesOfFiles;
            }
            catch (Exception exception)
            {
                _classLogger.Error(exception, _type.FullName, "ReturnExistingFileName");
                return null;
            }
        }

        /// <summary>
        /// This Methord Returns the name of all the file containing data 
        /// </summary>
        /// <param name="providerName">Name of Market Data Provider</param>
        /// <param name="historicDataRequest"> Symbol for which to fetch data</param>
        /// <returns></returns>
        private IList<string> ReturnExistingFileName(string providerName, HistoricDataRequest historicDataRequest)
        {
            try
            {
                IList<string> namesOfFiles = new List<string>();

                DateTime startTime = historicDataRequest.StartTime;
                DateTime endTime = historicDataRequest.EndTime;

                // Get possible directory path for bars created with LAST Price
                string directoryName = _specificFolder + "\\" + providerName + "\\" + historicDataRequest.Security.Symbol +
                                "\\Bar\\" + Common.Core.Constants.BarFormat.TIME + "\\" + Common.Core.Constants.BarPriceType.LAST
                                + "\\" + historicDataRequest.Interval;

                var directory = new DirectoryInfo(directoryName);

                // Find required files if the path exists
                if (directory.Exists)
                {
                    // Find all possible subfolders in the given directory
                    IEnumerable<string> subFolders =
                        directory.GetDirectories().Select(subDirectory => subDirectory.Name);

                    // Use all sub-directories to find files with required info
                    foreach (string subFolder in subFolders)
                    {
                        DateTime tempStartTime = new DateTime(startTime.Ticks);
                        while (tempStartTime.Date <= endTime.Date)
                        {
                            var filename = tempStartTime.ToString("yyyyMMdd") + ".txt";

                            // Get the File paths of required date.
                            string[] path = Directory.GetFiles(directoryName + "\\" + subFolder, filename, SearchOption.AllDirectories);

                            if (path.Any())
                            {
                                if (_classLogger.IsInfoEnabled)
                                {
                                    _classLogger.Info("Trying to read data from " + path[0], _type.FullName, "ReturnExistingFileName");
                                }
                                namesOfFiles.Add(path[0]);
                            }
                            tempStartTime = tempStartTime.AddDays(1);
                        }
                    }
                }

                return namesOfFiles;
            }
            catch (Exception exception)
            {
                _classLogger.Error(exception, _type.FullName, "ReturnExistingFileName");
                return null;
            }
        }

        /// <summary>
        /// Returns Lines Contained by a file 
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private IEnumerable<string> ReadLineByLine(string filePath)
        {
            if (_classLogger.IsInfoEnabled)
            {
                _classLogger.Info("Reading data from " + filePath, _type.FullName, "ReadLineByLine");
            }
            return File.ReadAllLines(filePath);
        }

        /// <summary>
        /// Gets Total Tick count 
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private int TickDataCount(string filePath)
        {
            var lines = File.ReadAllLines(filePath);

            // get data count
            return lines.Length;
        }
    }
}
