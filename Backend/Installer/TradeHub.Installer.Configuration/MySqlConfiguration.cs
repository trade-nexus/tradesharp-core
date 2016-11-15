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
using System.Collections;
using System.Collections.Generic;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using MySql.Data.MySqlClient;

namespace TradeHub.Installer.Configuration
{
    /// <summary>
    /// Mysql configuration class
    /// </summary>
    public static class MySqlConfiguration
    {
        /// <summary>
        /// Test connection with server
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static bool TestConnection(string username, string password)
        {
            bool result = false;
            string connectionString = string.Format("Server=127.0.0.1;Uid={0};Pwd={1};Database=test;", username,
                password);
            MySqlConnection connection = new MySqlConnection(connectionString);
            try
            {
                connection.Open();
                result = true;
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
            finally
            {
                connection.Close();
            }
            return result;
        }
        
        /// <summary>
        /// Change nhibernate spring config file
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <param name="filePath"></param>
        public static void ChangeSpringConfigFile(string userName,string password,string filePath)
        {
            try
            {
                string connectionString = string.Format("Server=localhost;Database=TradeHub;User ID={0};Password={1};",userName,password);
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(filePath);
                var enumer=xmlDoc.GetEnumerator();
                while (enumer.MoveNext())
                {
                    var x=enumer.Current;
                    XmlNode node = (XmlNode)x;
                    if (node.HasChildNodes)
                    {
                        foreach (XmlNode currentNode in node.ChildNodes)
                        {
                            if (currentNode.Name == "db:provider")
                            {
                                //replace new connection string
                                currentNode.Attributes[2].Value = connectionString;
                                break;
                            }
                        }
                    }
                }
                xmlDoc.Save(filePath);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        /// <summary>
        /// Deploy Database Script
        /// </summary>
        public static void DeployScript(string username,string password)
        {
            string script = File.ReadAllText("TradeHubDBScript.sql");
            string connectionString = string.Format("Server=127.0.0.1;Uid={0};Pwd={1};Database=test;", username,
                password);
            //create database if not exist
            ExecuteScript("CREATE DATABASE  IF NOT EXISTS `tradehub`", connectionString);
            //use that database(tradehub)
            connectionString = string.Format("Server=127.0.0.1;Uid={0};Pwd={1};Database=tradehub;", username,
                password);
            //run migrations
            MigrationsConfig.StartMigration(connectionString);
        }

        /// <summary>
        /// Execute sql script
        /// </summary>
        private static void ExecuteScript(string script,string connectionString)
        {
            MySqlConnection mySqlConnection=new MySqlConnection(connectionString);
            MySqlCommand command = new MySqlCommand(script, mySqlConnection);
            try
            {
                mySqlConnection.Open();
                command.ExecuteNonQuery();
                mySqlConnection.Close();
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
            finally
            {
                mySqlConnection.Close();
            }
        }
    }
}
