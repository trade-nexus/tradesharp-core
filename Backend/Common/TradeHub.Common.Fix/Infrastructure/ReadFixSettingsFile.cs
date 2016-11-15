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
