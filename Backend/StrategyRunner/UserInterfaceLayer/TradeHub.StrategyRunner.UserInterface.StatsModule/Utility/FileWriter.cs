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
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeHub.Common.Core.DomainModels.OrderDomain;

namespace TradeHub.StrategyRunner.UserInterface.StatsModule.Utility
{
    public class FileWriter
    {
        /// <summary>
        /// Writes order executions in CSV File
        /// </summary>
        public static void WriteFile(string path, ObservableCollection<Execution> statsCollection)
        {
            string activeDir = path;
            string newPath = Path.Combine(activeDir, string.Format("DATA_{0:yyyy-MM-dd}", DateTime.Now));
            Directory.CreateDirectory(newPath);
            string newFileName = string.Empty;
            newFileName = string.Format("stats_{0:hh-mm-ss-tt}.txt", DateTime.Now);
            string newLine = Environment.NewLine;
            newPath = Path.Combine(newPath, newFileName);

            if (!File.Exists(newPath))
            {
                StreamWriter outputFile = new StreamWriter(newPath);
                foreach (Execution execution in statsCollection)
                {
                    outputFile.WriteLine(execution.BasicExecutionInfo());
                }
                outputFile.Close();
            }
        }
    }
}
