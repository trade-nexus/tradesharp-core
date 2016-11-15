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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TraceSourceLogger;

namespace TradeHub.DataConverter.EsignalToDataDownloader
{
    /// <summary>
    /// Converts Bar data into required format
    /// </summary>
    public static class BarConverter
    {
        private static Type _type = typeof(BarConverter);

        /// <summary>
        /// Converts eSignal Bar data into Data Downloader bar format
        /// </summary>
        /// <param name="data">Data to be converted</param>
        /// <param name="symbol">Symbol for which the data is to be converted</param>
        /// <param name="provider">Market Data Provider name to be used</param>
        /// <returns></returns>
        public static string ConvertBars(string data, string symbol, string provider = "Blackwood")
        {
            try
            {
                string convertedData = string.Empty;

                string[] dataArray = data.Split(',');

                if (dataArray.Length > 5)
                {
                    StringBuilder stringBuilder = new StringBuilder();

                    // Add Close Price
                    //stringBuilder.Append(Math.Round(Convert.ToDecimal(dataArray[4]), 2));
                    stringBuilder.Append(dataArray[4]);
                    stringBuilder.Append(",");

                    // Add Open Price
                    //stringBuilder.Append(Math.Round(Convert.ToDecimal(dataArray[1]), 2));
                    stringBuilder.Append(dataArray[1]);
                    stringBuilder.Append(",");

                    // Add High Price
                    //stringBuilder.Append(Math.Round(Convert.ToDecimal(dataArray[2]), 2));
                    stringBuilder.Append(dataArray[2]);
                    stringBuilder.Append(",");

                    // Add Low Price
                    //stringBuilder.Append(Math.Round(Convert.ToDecimal(dataArray[3]), 2));
                    stringBuilder.Append(dataArray[3]);
                    stringBuilder.Append(",");

                    // Add Volume
                    stringBuilder.Append("150");
                    stringBuilder.Append(",");

                    // Add Symbol
                    stringBuilder.Append(symbol);
                    stringBuilder.Append(",");

                    // Add Data Time
                    var a = 86400M / 1e-7M;
                    var offset = -367;
                    var datetimeticks = (Convert.ToDecimal(dataArray[0]) + offset) * a;

                    DateTime dateTime = RoundUp(new DateTime(Convert.ToInt64(datetimeticks)), TimeSpan.FromSeconds(15));
                    dateTime = dateTime.AddSeconds(-dateTime.Second);
                    stringBuilder.Append(dateTime.ToString("M/d/yyyy h:mm:ss tt"));
                    stringBuilder.Append(",");

                    // Add Data Provider
                    stringBuilder.Append(provider);

                    // Convert to String
                    convertedData = stringBuilder.ToString();
                }
                return convertedData;
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "CovertBars");
                return null;
            }
        }

        /// <summary>
        /// Rounds the date time to nearest value specified
        /// </summary>
        private static DateTime RoundUp(DateTime dateTime, TimeSpan timeSpan)
        {
            return new DateTime(((dateTime.Ticks + timeSpan.Ticks - 1) / timeSpan.Ticks) * timeSpan.Ticks);
        }
    }
}
