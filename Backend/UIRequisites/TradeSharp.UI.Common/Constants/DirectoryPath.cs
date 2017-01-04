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


using System.IO;

namespace TradeSharp.UI.Common.Constants
{
    public static class DirectoryPath
    {
        // ReSharper disable InconsistentNaming

        public static string ROOT_DIRECTORY_PATH = Path.GetFullPath(@"~\..\..\");
        //// NOTE: For running application locally
        //public static string ROOT_DIRECTORY_PATH = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\TradeHub\\";
        
        public static string ORDER_ENGINE_PATH = ROOT_DIRECTORY_PATH + @"Order Execution Engine\";
        public static string MARKETDATA_ENGINE_PATH = ROOT_DIRECTORY_PATH + @"Market Data Engine\";
        public static string NOTIFICATION_ENGINE_PATH = ROOT_DIRECTORY_PATH + @"Notification Engine\";

        // ReSharper enable InconsistentNaming
    }
}
