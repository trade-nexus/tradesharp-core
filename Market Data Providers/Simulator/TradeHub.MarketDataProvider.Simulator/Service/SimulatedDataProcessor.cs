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
using TradeHub.Common.Core.DomainModels;
using TradeHub.MarketDataProvider.Simulator.Utility;

namespace TradeHub.MarketDataProvider.Simulator.Service
{
    /// <summary>
    /// Process the Provided input for the validity of data
    /// Also tranform incoming message to TradeHub messages
    /// </summary>
    public class SimulatedDataProcessor
    {
        private Type _type = typeof (SimulatedDataProcessor);

        public event Action<Tick> TickArrived;
        public event Action<Bar> LiveBarArrived;
        public event Action<Bar[]> HistoricalBarsArrived; 

        /// <summary>
        /// Processes the new incoming message and raises appropariate events
        /// </summary>
        /// <param name="message"></param>
        public void ProcessIncomingMessage(string message)
        {
            try
            {
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("", _type.FullName, "ProcessIncomingMessage");
                }

                // Remove spaces from the start and end of the message
                message = message.Trim();

                // Get message type
                Type type = GetMessageType(message);
                if (type != null)
                {
                    if (type.Equals(typeof (Tick)))
                    {
                        CreateTradeHubTick(message);
                    }
                    else if (type.Equals(typeof(Bar)))
                    {
                        CreateTradeHubBar(message);
                    }
                    else
                    {
                        ConsoleWriter.WriteLine(ConsoleColor.Red, "Unknown message type");
                        ConsoleWriter.WriteLine(ConsoleColor.Cyan, "Type 'Help' for info");
                        if (Logger.IsInfoEnabled)
                        {
                            Logger.Info("Unknown message type", _type.FullName, "ProcessIncomingMessage");
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                ConsoleWriter.WriteLine(ConsoleColor.DarkRed, exception.ToString());
                Logger.Error(exception, _type.FullName, "ProcessIncomingMessage");
            }
        }

        /// <summary>
        /// Returns the Type of Message Recieved
        /// </summary>
        /// <param name="message">Incoming string Message</param>
        /// <returns>Type of the TradeHub message intended</returns>
        private Type GetMessageType(string message)
        {
            try
            {
                string[] messageArray = message.Split(' ');
                if (messageArray.Length > 0)
                {
                    switch (messageArray[0].ToLower())
                    {
                        case SimulatorConstants.MessageTypes.Tick:
                            return typeof(Tick);
                        case SimulatorConstants.MessageTypes.LiveBar:
                            return typeof(Bar);
                        case SimulatorConstants.MessageTypes.HistoricBar:
                            return typeof(Bar[]);
                        default:
                            ConsoleWriter.WriteLine(ConsoleColor.Cyan, "Invalid Message. Type 'Help' for info");
                            return null;
                    }
                }
                ConsoleWriter.WriteLine(ConsoleColor.Cyan, "Invalid Message. Type 'Help' for info");
                return null;
            }
            catch (Exception exception)
            {
                ConsoleWriter.WriteLine(ConsoleColor.DarkRed, exception.ToString());
                Logger.Error(exception, _type.FullName, "GetMessageType");
                return null;
            }
        }

        /// <summary>
        /// Creates TradeHub Tick message and raises the event to notify listeners
        /// </summary>
        /// <param name="message">Incoming string message</param>
        private void CreateTradeHubTick(string message)
        {
            try
            {
                string[] messageArray = message.Split(' ');
                if (messageArray.Length < 13)
                {
                    ConsoleWriter.WriteLine(ConsoleColor.Red, "Missing fields in the Tick Message");
                    ConsoleWriter.WriteLine(ConsoleColor.Cyan, "Type 'Help' for info");
                    return;
                }

                var fields = ValidateTickFields(messageArray);
                if (fields != null)
                {
                    Tick tick = new Tick()
                    {
                        Security = new Security() { Symbol = messageArray[1].ToUpper() },
                        MarketDataProvider = Common.Core.Constants.MarketDataProvider.Simulated,
                        DateTime = DateTime.Now,
                        AskPrice = fields["AskPrice"],
                        AskSize = fields["AskSize"],
                        BidPrice = fields["BidPrice"],
                        BidSize = fields["BidSize"],
                        LastPrice = fields["LastPrice"],
                        LastSize = fields["LastSize"],
                        Depth = (int) fields["Depth"]
                        
                    };

                    // Raise Event
                    if (TickArrived != null)
                    {
                        TickArrived(tick);
                    }
                }
            }
            catch (Exception exception)
            {
                ConsoleWriter.WriteLine(ConsoleColor.DarkRed, exception.ToString());
                Logger.Error(exception, _type.FullName, "CreateTradeHubTick");
            }
        }

        /// <summary>
        /// Creates TradeHub Bar Message and raises the event to notify listeners
        /// </summary>
        /// <param name="message"></param>
        private void CreateTradeHubBar(string message)
        {
            try
            {
                string[] messageArray = message.Split(' ');
                if (messageArray.Length < 8)
                {
                    ConsoleWriter.WriteLine(ConsoleColor.Red, "Missing fields in the Bar Message");
                    ConsoleWriter.WriteLine(ConsoleColor.Cyan, "Type 'Help' for info");
                    return;
                }

                var fields = ValidateBarFields(messageArray);
                if (fields != null)
                {
                    Bar bar = new Bar(new Security {Symbol = messageArray[1].ToUpper()},
                        Common.Core.Constants.MarketDataProvider.Simulated, fields["ID"].ToString())
                        {
                            DateTime = DateTime.Now,
                            Open = Convert.ToDecimal(fields["Open"]),
                            High = Convert.ToDecimal(fields["High"]),
                            Low = Convert.ToDecimal(fields["Low"]),
                            Close = Convert.ToDecimal(fields["Close"]),
                            Volume = Convert.ToInt64(fields["Volume"])
                        };

                    // Raise Event
                    if (LiveBarArrived != null)
                    {
                        LiveBarArrived(bar);
                    }
                }
            }
            catch (Exception exception)
            {
                ConsoleWriter.WriteLine(ConsoleColor.DarkRed, exception.ToString());
                Logger.Error(exception, _type.FullName, "CreateTradeHubBar");
            }
        }

        /// <summary>
        /// Checks if the required Tick fields are in the correct format
        /// </summary>
        private Dictionary<string, decimal> ValidateTickFields(string[] messageArray)
        {
            try
            {
                Dictionary<string, decimal> _fields = new Dictionary<string, decimal>();

                decimal price;
                int volume;

                #region ASK

                if (decimal.TryParse(messageArray[3], out price))
                {
                    _fields.Add("AskPrice", price);
                }
                else
                {
                    ConsoleWriter.WriteLine(ConsoleColor.Red, "Ask Price was not in the correct format");
                    return null;
                }

                if (int.TryParse(messageArray[4], out volume))
                {
                    _fields.Add("AskSize", volume);
                }
                else
                {
                    ConsoleWriter.WriteLine(ConsoleColor.Red, "Ask Volume was not in the correct format");
                    return null;
                }

                #endregion

                #region BID

                if (decimal.TryParse(messageArray[6], out price))
                {
                    _fields.Add("BidPrice", price);
                }
                else
                {
                    ConsoleWriter.WriteLine(ConsoleColor.Red, "Bid Price was not in the correct format");
                    return null;
                }

                if (int.TryParse(messageArray[7], out volume))
                {
                    _fields.Add("BidSize", volume);
                }
                else
                {
                    ConsoleWriter.WriteLine(ConsoleColor.Red, "Bid Volume was not in the correct format");
                    return null;
                }

                #endregion

                #region LAST

                if (decimal.TryParse(messageArray[9], out price))
                {
                    _fields.Add("LastPrice", price);
                }
                else
                {
                    ConsoleWriter.WriteLine(ConsoleColor.Red, "Last Price was not in the correct format");
                    return null;
                }

                if (int.TryParse(messageArray[10], out volume))
                {
                    _fields.Add("LastSize", volume);
                }
                else
                {
                    ConsoleWriter.WriteLine(ConsoleColor.Red, "Last Volume was not in the correct format");
                    return null;
                }

                #endregion

                #region DEPTH

                int depth;
                if (int.TryParse(messageArray[12], out depth))
                {
                    _fields.Add("Depth", depth);
                }
                else
                {
                    ConsoleWriter.WriteLine(ConsoleColor.Red, "Depth was not in the correct format");
                    return null;
                }

                #endregion

                // Return values
                return _fields;
            }
            catch (Exception exception)
            {
                ConsoleWriter.WriteLine(ConsoleColor.DarkRed, exception.ToString());
                Logger.Error(exception, _type.FullName, "ValidateTickFields");
                return null;
            }
        }

        /// <summary>
        /// Checks if the required Bar fields are in the correct format
        /// </summary>
        private Dictionary<string, string> ValidateBarFields(string[] messageArray)
        {
            try
            {
                Dictionary<string, string> fields = new Dictionary<string, string>();

                decimal price;
                int volume;

                #region 

                if (!string.IsNullOrEmpty(messageArray[2]))
                {
                    fields.Add("ID", messageArray[2]);
                }
                else
                {
                    ConsoleWriter.WriteLine(ConsoleColor.Red, "ID not found");
                    return null;
                }

                #endregion

                #region OPEN

                if (decimal.TryParse(messageArray[3], out price))
                {
                    fields.Add("Open", price.ToString());
                }
                else
                {
                    ConsoleWriter.WriteLine(ConsoleColor.Red, "Open was not in the correct format");
                    return null;
                }

                #endregion

                #region HIGH

                if (decimal.TryParse(messageArray[4], out price))
                {
                    fields.Add("High", price.ToString());
                }
                else
                {
                    ConsoleWriter.WriteLine(ConsoleColor.Red, "High was not in the correct format");
                    return null;
                }

                #endregion

                #region LOW

                if (decimal.TryParse(messageArray[5], out price))
                {
                    fields.Add("Low", price.ToString());
                }
                else
                {
                    ConsoleWriter.WriteLine(ConsoleColor.Red, "Low was not in the correct format");
                    return null;
                }

                #endregion

                #region CLOSE

                if (decimal.TryParse(messageArray[6], out price))
                {
                    fields.Add("Close", price.ToString());
                }
                else
                {
                    ConsoleWriter.WriteLine(ConsoleColor.Red, "Close was not in the correct format");
                    return null;
                }

                #endregion

                #region VOLUME

                if (int.TryParse(messageArray[7], out volume))
                {
                    fields.Add("Volume", volume.ToString());
                }
                else
                {
                    ConsoleWriter.WriteLine(ConsoleColor.Red, "Volume was not in the correct format");
                    return null;
                }

                #endregion

                // Return values
                return fields;
            }
            catch (Exception exception)
            {
                ConsoleWriter.WriteLine(ConsoleColor.DarkRed, exception.ToString());
                Logger.Error(exception, _type.FullName, "ValidateBarFields");
                return null;
            }
        }

        /// <summary>
        /// Displays the Tick Subscription request received
        /// </summary>
        /// <param name="subscriptions">List of Subscriptions</param>
        public void DisplayTickSubscriptionInfo(List<Security> subscriptions)
        {
            ConsoleWriter.WriteLine(ConsoleColor.Blue,"Following Symbols are receievd for Tick Subscriptions");
            foreach (var subscription in subscriptions)
            {
                ConsoleWriter.WriteLine(ConsoleColor.Blue, subscription.Symbol);
            }
            ConsoleWriter.WriteLine(ConsoleColor.Blue, "");
        }

        /// <summary>
        /// Displays the Bar Subscription request received
        /// </summary>
        /// <param name="subscriptions">List of Subscriptions</param>
        public void DisplayBarSubscriptionInfo(Dictionary<string, Tuple<string, string>> subscriptions)
        {
            ConsoleWriter.WriteLine(ConsoleColor.Blue, "Following Symbols are receievd for Bar Subscriptions");
            foreach (var subscription in subscriptions.Values)
            {
                ConsoleWriter.WriteLine(ConsoleColor.Blue,
                                        String.Format("Request ID: {0} | Info: {1}", subscription.Item1,
                                                      subscription.Item2));
            }
            ConsoleWriter.WriteLine(ConsoleColor.Blue, "");
        }

        /// <summary>
        /// Dispalys Helpful Info
        /// </summary>
        public void DisplayHelp()
        {
            ConsoleWriter.WriteLine(ConsoleColor.DarkYellow, "HELP");
            ConsoleWriter.WriteLine(ConsoleColor.DarkYellow, "Supported Commands Inlcude: ");
            ConsoleWriter.WriteLine(ConsoleColor.DarkYellow, "Help, Sub Info Tick, Sub Info Bar");
            ConsoleWriter.WriteLine(ConsoleColor.DarkYellow, "");
            ConsoleWriter.WriteLine(ConsoleColor.DarkYellow, "Supported Message Types Inlcude: ");
            ConsoleWriter.WriteLine(ConsoleColor.DarkYellow, "Tick, Bar");
            ConsoleWriter.WriteLine(ConsoleColor.DarkYellow, "");
            ConsoleWriter.WriteLine(ConsoleColor.DarkYellow, "Message Details");
            ConsoleWriter.WriteLine(ConsoleColor.DarkYellow, "TICK");
            ConsoleWriter.WriteLine(ConsoleColor.DarkYellow, "The supported message format for Tick is:");
            ConsoleWriter.WriteLine(ConsoleColor.DarkYellow, "Tick <Symbol> Ask <Price> <Volume> Bid <Price> <Volume> Last <Price> <Volume> Depth <Level>");
            ConsoleWriter.WriteLine(ConsoleColor.DarkYellow, "BAR");
            ConsoleWriter.WriteLine(ConsoleColor.DarkYellow, "The supported message format Bar is:");
            ConsoleWriter.WriteLine(ConsoleColor.DarkYellow, "Bar <Symbol> <ID> <Open> <High> <Low> <Close> <Volume>");
            ConsoleWriter.WriteLine(ConsoleColor.DarkYellow, "");
            ConsoleWriter.WriteLine(ConsoleColor.DarkYellow, "Command Details");
            ConsoleWriter.WriteLine(ConsoleColor.DarkYellow, "SUB INFO TICK");
            ConsoleWriter.WriteLine(ConsoleColor.DarkYellow, "Provides the list of Tick Subscriptions");
            ConsoleWriter.WriteLine(ConsoleColor.DarkYellow, "SUB INFO BAR");
            ConsoleWriter.WriteLine(ConsoleColor.DarkYellow, "Provides the list of Bar Subscriptions");
            ConsoleWriter.WriteLine(ConsoleColor.DarkYellow, "");
            ConsoleWriter.WriteLine(ConsoleColor.DarkYellow, "Start");
            ConsoleWriter.WriteLine(ConsoleColor.DarkYellow, "Starts sending conitnues tick data for already subscribed symbols");
        }
    }
}
