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
using TradeHub.Common.Core.DomainModels.OrderDomain;
using TradeHub.OrderExecutionProviders.Simulator.Utility;
using TradeHubConstants = TradeHub.Common.Core.Constants;

namespace TradeHub.OrderExecutionProviders.Simulator.Service
{
    /// <summary>
    /// Process the Provided input for the validity of data
    /// Also tranform incoming message to TradeHub messages
    /// </summary>
    public class SimulatedOrderProcessor
    {
        private Type _type = typeof(SimulatedOrderProcessor);

        public event Action<Order> NewArrived;
        public event Action<Order> CancellationArrived;
        public event Action<Execution> ExecutionArrived;
        public event Action<Rejection> RejectionArrived;
        public event Action<LimitOrder> LocateMessageArrived;
        public event Action<Position> PositionArrived;

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
                    Logger.Info("Process incoming user input: " + message, _type.FullName, "ProcessIncomingMessage");
                }

                // Remove spaces from the start and end of the message
                message = message.Trim();

                // Create message response depending on input
                CreateRequiredMessage(message);
            }
            catch (Exception exception)
            {
                ConsoleWriter.WriteLine(ConsoleColor.DarkRed, exception.ToString());
                Logger.Error(exception, _type.FullName, "ProcessIncomingMessage");
            }
        }

        /// <summary>
        /// Creates the required Message from received input
        /// </summary>
        /// <param name="message">Incoming string Message</param>
        private void CreateRequiredMessage(string message)
        {
            try
            {
                string[] messageArray = message.Split(' ');
                if (messageArray.Length > 0)
                {
                    switch (messageArray[0].ToLower())
                    {
                        case SimulatorConstants.MessageTypes.New:
                            CreateTradeHubNewOrder(messageArray);
                            return;
                        case SimulatorConstants.MessageTypes.Cancel:
                            CreateTradeHubCancelOrder(messageArray);
                            return;
                        case SimulatorConstants.MessageTypes.Execution:
                            CreateTradeHubExecution(messageArray);
                            return;
                        case SimulatorConstants.MessageTypes.Rejection:
                            CreateTradeHubRejection(messageArray);
                            return;
                        case SimulatorConstants.MessageTypes.Locate:
                            CreateLocateMessage(messageArray);
                            return;
                        case SimulatorConstants.MessageTypes.Position:
                            CreatePositionMessage(messageArray);
                            return;
                        default:
                            ConsoleWriter.WriteLine(ConsoleColor.Red, "Unknown message type");
                            ConsoleWriter.WriteLine(ConsoleColor.Cyan, "Type 'Help' for info");
                            return;
                    }
                }
                ConsoleWriter.WriteLine(ConsoleColor.Cyan, "Invalid Message. Type 'Help' for info");
            }
            catch (Exception exception)
            {
                ConsoleWriter.WriteLine(ConsoleColor.DarkRed, exception.ToString());
                Logger.Error(exception, _type.FullName, "CreateRequiredMessage");
            }
        }

        private void CreatePositionMessage(string[] message)
        {
            try
            {
                //create Position
                if (ValidatePositionFields(message))
                {
                    Position position = new Position();
                    position.Provider = TradeHubConstants.OrderExecutionProvider.Simulated;
                    position.Security = new Security() {Symbol = message[1]};
                    position.Quantity = long.Parse(message[2]);
                    position.ExitValue = decimal.Parse(message[3]);
                    position.Price = decimal.Parse(message[4]);
                    position.AvgBuyPrice = decimal.Parse(message[5]);
                    position.AvgSellPrice = decimal.Parse(message[6]);
                    position.IsOpen = position.Quantity != 0;
                    position.Type = (position.Quantity > 0) ? PositionType.Long : PositionType.Short;
                    if (position.Quantity == 0)
                        position.Type = PositionType.Flat;
                    if (PositionArrived != null)
                        PositionArrived(position);
                }
            }
            catch (Exception exception)
            {
                ConsoleWriter.WriteLine(ConsoleColor.DarkRed, exception.ToString());
                Logger.Error(exception, _type.FullName, "CreatePositionMessage");
               
            }
            
           
        }

        /// <summary>
        /// Creates TradeHub Order from user input
        /// </summary>
        /// <returns></returns>
        /// <param name="message">Incoming string message</param>
        private Order CreateTradeHubOrder(string[] message)
        {
            try
            {
                // Creat TradeHub Order
                Order order = new Order(TradeHubConstants.OrderExecutionProvider.Simulated);

                // Add Order ID from user input
                order.OrderID = message[1];

                return order;
            }
            catch (Exception exception)
            {
                ConsoleWriter.WriteLine(ConsoleColor.DarkRed, exception.ToString());
                Logger.Error(exception, _type.FullName, "CreateTradeHubOrder");
                return null;
            }
        }

        /// <summary>
        /// Creates TradeHub LimitOrder from user input
        /// </summary>
        /// <returns></returns>
        /// <param name="message">Incoming string message</param>
        private LimitOrder CreateTradeHubLimitOrder(string[] message)
        {
            try
            {
                // Creat TradeHub Order
                LimitOrder order = new LimitOrder(TradeHubConstants.OrderExecutionProvider.Simulated);

                // Add Order ID from user input
                order.OrderID = message[1];
                // Add Order Symbol from user input
                order.Security = new Security() {Symbol = message[2]};
                // Add Order Price
                order.LimitPrice = Convert.ToDecimal(message[3]);
                // Add Order Size
                order.OrderSize = Convert.ToInt32(message[4]);

                return order;
            }
            catch (Exception exception)
            {
                ConsoleWriter.WriteLine(ConsoleColor.DarkRed, exception.ToString());
                Logger.Error(exception, _type.FullName, "CreateTradeHubOrder");
                return null;
            }
        }

        /// <summary>
        /// Creates TradeHub Order with status New/Submitted from user input
        /// </summary>
        /// <returns></returns>
        /// <param name="message">Incoming string message</param>
        private void CreateTradeHubNewOrder(string[] message)
        {
            try
            {
                if (ValidateOrderFields(message))
                {
                    Order order = CreateTradeHubOrder(message);
                    order.OrderStatus = TradeHubConstants.OrderStatus.SUBMITTED;

                    // Raise Event
                    if (NewArrived != null)
                    {
                        NewArrived(order);
                    }
                }
            }
            catch (Exception exception)
            {
                ConsoleWriter.WriteLine(ConsoleColor.DarkRed, exception.ToString());
                Logger.Error(exception, _type.FullName, "CreateTradeHubNewOrder");
            }
        }

        /// <summary>
        /// Creates TradeHub Order with status Cancelled from user input
        /// </summary>
        /// <returns></returns>
        /// <param name="message">Incoming string message</param>
        private void CreateTradeHubCancelOrder(string[] message)
        {
            try
            {
                if (ValidateOrderFields(message))
                {
                    Order order = CreateTradeHubOrder(message);
                    order.OrderStatus = TradeHubConstants.OrderStatus.CANCELLED;

                    // Raise Event
                    if (CancellationArrived != null)
                    {
                        CancellationArrived(order);
                    }
                }
            }
            catch (Exception exception)
            {
                ConsoleWriter.WriteLine(ConsoleColor.DarkRed, exception.ToString());
                Logger.Error(exception, _type.FullName, "CreateTradeHubCancelOrder");
            }
        }

        /// <summary>
        /// Creates TradeHub Execution Info from user input
        /// </summary>
        /// <returns></returns>
        /// <param name="message">Incoming string message</param>
        private void CreateTradeHubExecution(string[] message)
        {
            try
            {
                if (ValidateExecutionFields(message))
                {
                    // Create TradeHub Order containing baisc info
                    Order order = CreateTradeHubOrder(message);

                    // Set Order Status
                    order.OrderStatus = TradeHubConstants.OrderStatus.EXECUTED;

                    // Create TradeHub Execution object containing execution details
                    Fill execution = new Fill(new Security{ Symbol = message[2] }, TradeHubConstants.OrderExecutionProvider.Simulated, order.OrderID);

                    // Set Execution ID
                    execution.ExecutionId = DateTime.UtcNow.ToString("HHmmssfff");

                    // Set Execution Time
                    execution.ExecutionDateTime = DateTime.UtcNow;

                    // Set Execution Price
                    execution.ExecutionPrice = Convert.ToDecimal(message[3]);

                    // Set Execution Quantity
                    execution.ExecutionSize = Convert.ToInt32(message[4]);

                    //Set Commulative Quantity 
                    execution.CummalativeQuantity = Convert.ToInt32(message[4]);

                    // Set Leaves Quantity
                    execution.CummalativeQuantity = 0;

                    // Create TradeHub Excution Info Object
                    Execution executionInfo = new Execution(execution, order);
                    executionInfo.OrderExecutionProvider = TradeHubConstants.OrderExecutionProvider.Simulated;

                    // Raise Event
                    if (ExecutionArrived != null)
                    {
                        ExecutionArrived(executionInfo);
                    }
                }
            }
            catch (Exception exception)
            {
                ConsoleWriter.WriteLine(ConsoleColor.DarkRed, exception.ToString());
                Logger.Error(exception, _type.FullName, "CreateTradeHubExecutionInfo");
            }
        }

        /// <summary>
        /// Creates TradeHub Rejection from user input
        /// </summary>
        /// <returns></returns>
        /// <param name="message">Incoming string message</param>
        private void CreateTradeHubRejection(string[] message)
        {
            try
            {
                if (ValidateRejectionFields(message))
                {
                    Rejection rejection = new Rejection(new Security(),TradeHubConstants.OrderExecutionProvider.Simulated);

                    // Add Order ID from User Input
                    rejection.OrderId = message[1];

                    // Add rejection reason from User Input
                    rejection.RejectioReason = message[2];

                    // Raise Event
                    if (RejectionArrived != null)
                    {
                        RejectionArrived(rejection);
                    }
                }
            }
            catch (Exception exception)
            {
                ConsoleWriter.WriteLine(ConsoleColor.DarkRed, exception.ToString());
                Logger.Error(exception, _type.FullName, "CreateTradeHubRejection");
            }
        }

        /// <summary>
        /// Creates Locate Message from user input
        /// </summary>
        /// <param name="message">Incoming string message</param>
        private void CreateLocateMessage(string[] message)
        {
            try
            {
                if (ValidateLocateFields(message))
                {
                    // Create TradeHub LimitOrder containing Locate info
                    LimitOrder limitOrder= CreateTradeHubLimitOrder(message);

                    // Raise Event
                    if (LocateMessageArrived != null)
                    {
                        LocateMessageArrived(limitOrder);
                    }
                }
            }
            catch (Exception exception)
            {
                ConsoleWriter.WriteLine(ConsoleColor.DarkRed, exception.ToString());
                Logger.Error(exception, _type.FullName, "CreateLocateMessage");
            }
        }

        /// <summary>
        /// Performs basic validation on user input for Order
        /// </summary>
        /// <param name="message">User input</param>
        /// <returns>True/False depending on validation</returns>
        private bool ValidateOrderFields(string[] message)
        {
            try
            {
                if (message.Length == 2)
                {
                    return true;
                }
                ConsoleWriter.WriteLine(ConsoleColor.Red, "Missing ID field from order input.");
                ConsoleWriter.WriteLine(ConsoleColor.Cyan, "Type 'Help' for info");
                return false;
            }
            catch (Exception exception)
            {
                ConsoleWriter.WriteLine(ConsoleColor.DarkRed, exception.ToString());
                Logger.Error(exception, _type.FullName, "ValidateOrderFields");
                return false;
            }
        }

        /// <summary>
        /// Performs basic validation on user input for Rejection
        /// </summary>
        /// <param name="message">User input</param>
        /// <returns>True/False depending on validation</returns>
        private bool ValidateRejectionFields(string[] message)
        {
            try
            {
                if (message.Length == 3)
                {
                    return true;
                }
                ConsoleWriter.WriteLine(ConsoleColor.Red, "Missing fields from rejection input.");
                ConsoleWriter.WriteLine(ConsoleColor.Cyan, "Type 'Help' for info");
                return false;
            }
            catch (Exception exception)
            {
                ConsoleWriter.WriteLine(ConsoleColor.DarkRed, exception.ToString());
                Logger.Error(exception, _type.FullName, "ValidateRejectionFields");
                return false;
            }
        }

        /// <summary>
        /// Performs basic validation on user input for Execution
        /// </summary>
        /// <param name="message">User input</param>
        /// <returns>True/False depending on validation</returns>
        private bool ValidateExecutionFields(string[] message)
        {
            try
            {
                if (message.Length == 5)
                {
                    // Validate Execution Price
                    decimal executionPrice;
                    if (!decimal.TryParse(message[3], out  executionPrice))
                    {
                        ConsoleWriter.WriteLine(ConsoleColor.Red, "Price was not in the correct format");
                        return false;
                    }

                    // Validate Execution Price
                    int executionQuantity;
                    if (!int.TryParse(message[4], out  executionQuantity))
                    {
                        ConsoleWriter.WriteLine(ConsoleColor.Red, "Quantity was not in the correct format");
                        return false;
                    }

                    return true;
                }
                ConsoleWriter.WriteLine(ConsoleColor.Red, "Missing fields from execution input.");
                ConsoleWriter.WriteLine(ConsoleColor.Cyan, "Type 'Help' for info");
                return false;
            }
            catch (Exception exception)
            {
                ConsoleWriter.WriteLine(ConsoleColor.DarkRed, exception.ToString());
                Logger.Error(exception, _type.FullName, "ValidateExecutionFields");
                return false;
            }
        }

        private bool ValidatePositionFields(string[] message)
        {
            try
            {
                if (message.Length == 7)
                {
                    // Validate AvgSell price
                    decimal avgsell;
                    if (!decimal.TryParse(message[6], out  avgsell))
                    {
                        ConsoleWriter.WriteLine(ConsoleColor.Red, "AvgSell price was not in the correct format");
                        return false;
                    }
                    // Validate AvgBuy price
                    decimal buyPrice;
                    if (!decimal.TryParse(message[5], out  buyPrice))
                    {
                        ConsoleWriter.WriteLine(ConsoleColor.Red, "AvgBuy price was not in the correct format");
                        return false;
                    }
                    // Validate Price
                    decimal price;
                    if (!decimal.TryParse(message[4], out  price))
                    {
                        ConsoleWriter.WriteLine(ConsoleColor.Red, "Price was not in the correct format");
                        return false;
                    }
                    // Validate Exit Value
                    decimal exitValue;
                    if (!decimal.TryParse(message[3], out  exitValue))
                    {
                        ConsoleWriter.WriteLine(ConsoleColor.Red, "Exit value was not in the correct format");
                        return false;
                    }

                    // Validate position Quantity
                    long positionQuantity;
                    if (!long.TryParse(message[2], out  positionQuantity))
                    {
                        ConsoleWriter.WriteLine(ConsoleColor.Red, "Quantity was not in the correct format");
                        return false;
                    }

                    return true;
                }
                ConsoleWriter.WriteLine(ConsoleColor.Red, "Missing fields from Position input.");
                ConsoleWriter.WriteLine(ConsoleColor.Cyan, "Type 'Help' for info");
                return false;
            }
            catch (Exception exception)
            {
                ConsoleWriter.WriteLine(ConsoleColor.DarkRed, exception.ToString());
                Logger.Error(exception, _type.FullName, "ValidatePositionFields");
                return false;
            }
        }

        /// <summary>
        /// Performs basic validation on user input for Locate Message
        /// </summary>
        /// <param name="message">User input</param>
        /// <returns>True/False depending on validation</returns>
        private bool ValidateLocateFields(string[] message)
        {
            try
            {
                if (message.Length == 5)
                {
                    // Validate Execution Price
                    decimal limitPrice;
                    if (!decimal.TryParse(message[3], out  limitPrice))
                    {
                        ConsoleWriter.WriteLine(ConsoleColor.Red, "Price was not in the correct format");
                        return false;
                    }

                    // Validate Execution Price
                    int quantity;
                    if (!int.TryParse(message[4], out  quantity))
                    {
                        ConsoleWriter.WriteLine(ConsoleColor.Red, "Quantity was not in the correct format");
                        return false;
                    }

                    return true;
                }
                ConsoleWriter.WriteLine(ConsoleColor.Red, "Missing fields from execution input.");
                ConsoleWriter.WriteLine(ConsoleColor.Cyan, "Type 'Help' for info");
                return false;
            }
            catch (Exception exception)
            {
                ConsoleWriter.WriteLine(ConsoleColor.DarkRed, exception.ToString());
                Logger.Error(exception, _type.FullName, "ValidateExecutionFields");
                return false;
            }
        }

        /// <summary>
        /// Displays the Orders List
        /// </summary>
        /// <param name="orders">List of Order</param>
        /// <param name="info">string message to be appended</param>
        public void DisplayOrdersInfo(Dictionary<string, Order> orders, string info)
        {
            ConsoleWriter.WriteLine(ConsoleColor.Blue, "Following Orders are " + info);
            foreach (var order in orders)
            {
                ConsoleWriter.WriteLine(ConsoleColor.Blue,
                                        String.Format("Request ID: {0} | Info: {1}", order.Key,
                                                      order.Value));
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
            ConsoleWriter.WriteLine(ConsoleColor.DarkYellow, "Help, Info New, Info Accepted, Info Filled");
            ConsoleWriter.WriteLine(ConsoleColor.DarkYellow, "");
            ConsoleWriter.WriteLine(ConsoleColor.DarkYellow, "Supported Message Types Inlcude: ");
            ConsoleWriter.WriteLine(ConsoleColor.DarkYellow, "New, Cancel, Execution, Rejection");
            ConsoleWriter.WriteLine(ConsoleColor.DarkYellow, "");
            ConsoleWriter.WriteLine(ConsoleColor.DarkYellow, "Message Details");
            ConsoleWriter.WriteLine(ConsoleColor.DarkYellow, "NEW");
            ConsoleWriter.WriteLine(ConsoleColor.DarkYellow, "The supported message format for NEW Order is:");
            ConsoleWriter.WriteLine(ConsoleColor.DarkYellow, "N <ID>");
            ConsoleWriter.WriteLine(ConsoleColor.DarkYellow, "CANCEL");
            ConsoleWriter.WriteLine(ConsoleColor.DarkYellow, "The supported message format for CANCEL Order is:");
            ConsoleWriter.WriteLine(ConsoleColor.DarkYellow, "C <ID>");
            ConsoleWriter.WriteLine(ConsoleColor.DarkYellow, "EXECUTION");
            ConsoleWriter.WriteLine(ConsoleColor.DarkYellow, "The supported message format for EXECUTION Order is:");
            ConsoleWriter.WriteLine(ConsoleColor.DarkYellow, "E <ID> <Symbol> <Price> <Quantity>");
            ConsoleWriter.WriteLine(ConsoleColor.DarkYellow, "REJECTION");
            ConsoleWriter.WriteLine(ConsoleColor.DarkYellow, "The supported message format for REJECTION is:");
            ConsoleWriter.WriteLine(ConsoleColor.DarkYellow, "R <ID> <Reason>");
            ConsoleWriter.WriteLine(ConsoleColor.DarkYellow, "Locate");
            ConsoleWriter.WriteLine(ConsoleColor.DarkYellow, "The supported message format for LOCATE is:");
            ConsoleWriter.WriteLine(ConsoleColor.DarkYellow, "L <ID> <Symbol> <Price> <Size>");
            ConsoleWriter.WriteLine(ConsoleColor.DarkYellow, "");
            ConsoleWriter.WriteLine(ConsoleColor.DarkYellow, "Position");
            ConsoleWriter.WriteLine(ConsoleColor.DarkYellow, "The supported message format for POSITION is:");
            ConsoleWriter.WriteLine(ConsoleColor.DarkYellow, "P <Symbol> <Quantity> <ExitValue> <Price> <AvgBuyPrice> <AvgSellPrice>");
            ConsoleWriter.WriteLine(ConsoleColor.DarkYellow, "");
            ConsoleWriter.WriteLine(ConsoleColor.DarkYellow, "Command Details");
            ConsoleWriter.WriteLine(ConsoleColor.DarkYellow, "INFO NEW");
            ConsoleWriter.WriteLine(ConsoleColor.DarkYellow, "Provides the list of unaccepted order");
            ConsoleWriter.WriteLine(ConsoleColor.DarkYellow, "INFO ACCEPTED");
            ConsoleWriter.WriteLine(ConsoleColor.DarkYellow, "Provides the list of accepted order");
            ConsoleWriter.WriteLine(ConsoleColor.DarkYellow, "INFO FILLED");
            ConsoleWriter.WriteLine(ConsoleColor.DarkYellow, "Provides the list of filled order");
            ConsoleWriter.WriteLine(ConsoleColor.DarkYellow, "");
        }
    }
}
