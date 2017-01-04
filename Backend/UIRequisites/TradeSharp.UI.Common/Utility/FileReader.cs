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


using System;
using System.Collections.Generic;
using System.IO;
using TraceSourceLogger;

namespace TradeSharp.UI.Common.Utility
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
                while ((input = streamReader.ReadLine()) != null)
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
                Logger.Error(exception, "TradeHubGui.Common.Utility.Utility.FileReader", "ReadParameters");
                return null;
            }
        }
    }
}
