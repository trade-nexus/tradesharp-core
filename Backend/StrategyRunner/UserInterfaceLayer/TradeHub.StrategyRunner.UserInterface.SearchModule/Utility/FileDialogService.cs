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
using Microsoft.Win32;

namespace TradeHub.StrategyRunner.UserInterface.SearchModule.Utility
{
    public class FileDialogService
    {
        public string FileName
        {
            get { return _fileName; }
        }

        public Nullable<bool> OpenFileDialog(string extension, string fileInfo)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();

            // Set filter for file extension and default file extension
            fileDialog.DefaultExt = extension;
            fileDialog.Filter = fileInfo + "(" + extension + ")|*" + extension;

            // Display OpenFileDialog by calling ShowDialog method
            Nullable<bool> result = fileDialog.ShowDialog();

            // Get the selected file name and display in a TextBox
            if (result == true)
            {
                // Get file name
                this._fileName = fileDialog.FileName;
            }

            return result;
        }

        private string _fileName;
    }
}
