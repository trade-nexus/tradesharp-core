using System;
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
