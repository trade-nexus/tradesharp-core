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

namespace TradeSharpLicense.Manager
{
    public class LicenseCreator
    {
        /// <summary>
        /// Creates a new License file with the given information
        /// </summary>
        /// <param name="expirationDate">Data at which the license should exprice. Format: YYYYMMDD</param>
        /// <param name="clientDetails">Brief client details e.g. name or company, can be max 20 characters</param>
        /// <param name="licenseType">LicenseType from the supported enums</param>
        public string Create(string expirationDate, string clientDetails, LicenseType licenseType)
        {
            // Add padding to create a 10 char long string
            expirationDate = expirationDate.PadLeft(10);

            // Add padding to complete the 20 char length
            clientDetails = clientDetails.PadRight(20);

            // Convert enum to string and add padding to fill 10 char limit
            string licenseTypeString = (licenseType.ToString()).PadLeft(10);

            // Combine all the information in a single block
            string sampleData = @expirationDate + clientDetails + licenseTypeString;

            // Initialze enrcyptor
            Encryptor encryptor = new Encryptor();

            // Get the encrypted string
            string encryptedString = encryptor.EncryptToString(sampleData);

            // Write encrypted string to create license file
            WriteBinaryData(encryptedString);

            return sampleData;
        }

        /// <summary>
        /// Writes the incoming string data in a binary file
        /// </summary>
        /// <param name="dataToWrite"></param>
        private static void WriteBinaryData(string dataToWrite)
        {
            BinaryFileWriter binaryFileWriter = new BinaryFileWriter();

            binaryFileWriter.Write(dataToWrite);
        }
    }
}
