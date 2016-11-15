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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TradeHub.Installer.Configuration
{
    public partial class SettingsForm : Form
    {
        public SettingsForm()
        {
            InitializeComponent();
        }

        private void OnTestButtonClick(object sender, EventArgs e)
        {
           TestConnection();
        }

        private void OnDoneButtonClick(object sender, EventArgs e)
        {
            if (MySqlConfiguration.TestConnection(usernameTextBox.Text, passwordTextBox.Text))
            {
                
                MySqlConfiguration.DeployScript(usernameTextBox.Text,passwordTextBox.Text);
                string filePath = Path.GetFullPath(@"~\..\..\TradeSharp User Interface\SpringConfig\SpringDao.xml");
                MySqlConfiguration.ChangeSpringConfigFile(usernameTextBox.Text, passwordTextBox.Text, filePath);

                //Display message to user about success.
                MessageBox.Show("Application has been configured, now you can start the application");

                //Close the application
                this.Close();
            }
            else
            {
                MessageBox.Show("Please verify connection first");
            }
        }

        /// <summary>
        /// Test Connection
        /// </summary>
        private void TestConnection()
        {
            if (MySqlConfiguration.TestConnection(usernameTextBox.Text, passwordTextBox.Text))
            {
                MessageBox.Show("Connection Successfull");
            }
            else
            {
                MessageBox.Show("Invalid credentials");
            }
        }
    }
}
