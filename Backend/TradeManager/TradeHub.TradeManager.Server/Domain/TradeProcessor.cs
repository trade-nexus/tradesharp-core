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
using TradeHub.Common.Core.Constants;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.DomainModels.OrderDomain;
using TradeHub.Common.Persistence;

namespace TradeHub.TradeManager.Server.Domain
{
    /// <summary>
    /// Provides bussiness logic to create Trades based on the received executions
    /// </summary>
    public class TradeProcessor
    {
        private Type _type = typeof (TradeProcessor);

        /// <summary>
        /// Security on which Executions are occuring
        /// </summary>
        private readonly Security _security;

        /// <summary>
        /// Order Execution Provider for which to manage the Trades
        /// </summary>
        private readonly string _executionProvider = string.Empty;

        /// <summary>
        /// Overall position for the current Order Execution Provider
        /// </summary>
        private int _position = 0;

        /// <summary>
        /// Incremental value to be used as KEY for Open Trades MAP
        /// </summary>
        private int _tradeCount = 0;

        /// <summary>
        /// All open Trades for the given Order Execution Provider
        /// KEY = _tradeCount (As Incremental)
        /// VALUE = Trade
        /// </summary>
        private SortedDictionary<int, Trade> _openTrades;

        /// <summary>
        /// All open Trades for the given Order Execution Provider grouped on the basis of Security
        /// KEY = _tradeCount (As Incremental)
        /// VALUE = Trade
        /// </summary>
        private Dictionary<Security, SortedDictionary<int, Trade>> _openTradesBySecurity;

        /// <summary>
        /// Security on which Executions are occuring
        /// </summary>
        public Security Security
        {
            get { return _security; }
        }

        /// <summary>
        /// Order Execution Provider for which to manage the Trades
        /// </summary>
        public string ExecutionProvider
        {
            get { return _executionProvider; }
        }

        /// <summary>
        /// Overall position for the current Order Execution Provider
        /// </summary>
        public int Position
        {
            get { return _position; }
        }

        /// <summary>
        /// Return Read-Only version of the local MAP
        /// </summary>
        public IReadOnlyDictionary<int, Trade> OpenTrades
        {
            get { return _openTrades.ToDictionary(x => x.Key, x => x.Value); }
        }

        /// <summary>
        /// Argument Constructor
        /// </summary>
        /// <param name="security">Security on which Executions are occuring</param>
        /// <param name="executionProvider">Order Execution Provider for which to manage the Trades</param>
        public TradeProcessor(Security security, string executionProvider)
        {
            // Save information
            _security = security;
            _executionProvider = executionProvider;

            // Initialize
            _openTrades = new SortedDictionary<int, Trade>();
        }

        /// <summary>
        /// Called when new Execution is received
        /// </summary>
        /// <param name="execution">Order Execution Object</param>
        public void NewExecutionArrived(Execution execution)
        {
            try
            {
                // Check overall position
                if (_position == 0)
                {
                    // Reset Trade Count
                    _tradeCount = 0;

                    // Clear local MAP
                    _openTrades.Clear();

                    // Set Open Position
                    _position = execution.Fill.ExecutionSize;

                    // Create Trade Size
                    TradeSide tradeSide = TradeSide.Buy;

                    // Change position to 'Negative' if going towards 'SELL'/'SHORT'
                    UpdateTradeDirection(execution.Fill.ExecutionSide, ref _position, ref tradeSide);

                    if (Logger.IsDebugEnabled)
                    {
                        Logger.Debug("Position: " + _position, _type.FullName, "NewExecutionArrived" + _executionProvider);
                        Logger.Debug("Execution Size: " + _position, _type.FullName, "NewExecutionArrived" + _executionProvider);
                        Logger.Debug("Opening New Trade. Execution ID: " + execution.Fill.ExecutionId, _type.FullName, "NewExecutionArrived" + _executionProvider);
                    }

                    // Add new Trade
                    AddNewTrade(tradeSide, execution.Fill.ExecutionSize, execution.Fill.ExecutionPrice,
                        execution.Fill.ExecutionId, execution.Fill.Security, execution.Fill.ExecutionDateTime);
                }
                // Adjust current open trades
                else
                {
                    int executionSize = execution.Fill.ExecutionSize;
                    
                    // Create Trade Size
                    TradeSide tradeSide = TradeSide.Buy;
                    
                    // Set 'Negative' sign if the execution is 'SELL'/'SHORT'
                    UpdateTradeDirection(execution.Fill.ExecutionSide, ref executionSize, ref tradeSide);

                    // Check if new Trade needs to be opened
                    if (_position != 0 && HaveSameSign(_position, executionSize))
                    {
                        // Update Position
                        _position = _position + executionSize;

                        if (Logger.IsDebugEnabled)
                        {
                            Logger.Debug("Position: " + _position, _type.FullName, "NewExecutionArrived" + _executionProvider);
                            Logger.Debug("Execution Size: " + executionSize, _type.FullName, "NewExecutionArrived" + _executionProvider);
                            Logger.Debug("Opening New Trade. Execution ID: " + execution.Fill.ExecutionId, _type.FullName, "NewExecutionArrived" + _executionProvider);
                        }

                        // Add new Trade
                        AddNewTrade(tradeSide, execution.Fill.ExecutionSize, execution.Fill.ExecutionPrice,
                            execution.Fill.ExecutionId, execution.Fill.Security, execution.Fill.ExecutionDateTime);
                    }
                    // Update existing Trades
                    else
                    {
                        // Update Position
                        _position = _position + executionSize;

                        if (Logger.IsDebugEnabled)
                        {
                            Logger.Debug("Position: " + _position, _type.FullName, "NewExecutionArrived" + _executionProvider);
                            Logger.Debug("Execution Size: " + executionSize, _type.FullName, "NewExecutionArrived" + _executionProvider);
                            Logger.Debug("Updating Existing Trade. Execution ID: " + execution.Fill.ExecutionId, _type.FullName, "NewExecutionArrived" + _executionProvider);
                        }

                        // Update Trade
                        UpdateTrade(execution.Fill.ExecutionId, execution.Fill.ExecutionSize,
                            execution.Fill.ExecutionPrice, execution.Fill.Security, execution.Fill.ExecutionDateTime);
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "NewExecutionArrived" + _executionProvider);
            }
        }

        /// <summary>
        /// Add new Open Trade to the MAP
        /// </summary>
        /// <param name="tradeSide">Trade Side</param>
        /// <param name="executionSize">Execution Size</param>
        /// <param name="executionPrice">Execution Price</param>
        /// <param name="executionId">Execution ID</param>
        /// <param name="security">Security</param>
        /// <param name="executionTime">Execution Time</param>
        private void AddNewTrade(TradeSide tradeSide, int executionSize, decimal executionPrice, string executionId, Security security, DateTime executionTime)
        {
            // Create a new Trade Object
            Trade trade = new Trade(tradeSide, executionSize, executionPrice, _executionProvider, executionId, security, executionTime);

            // Increment Trade Count
            _tradeCount++;

            // Add to local MAP
            _openTrades.Add(_tradeCount, trade);

            if (Logger.IsInfoEnabled)
            {
                Logger.Info("New Trade Opened: " + trade, _type.FullName, "AddNewTrade");
            }
        }

        /// <summary>
        /// Update Existing Open Trades
        /// </summary>
        /// <param name="executionId">Execution ID</param>
        /// <param name="executionSize">Execution Size</param>
        /// <param name="executionPrice">Execution Price</param>
        /// <param name="security">Security</param>
        /// <param name="executionTime">Execution Time</param>
        private void UpdateTrade(string executionId, int executionSize, decimal executionPrice, Security security, DateTime executionTime)
        {
            while (true)
            {
                // Get Oldest Trade
                var tradeKeyValuePair = _openTrades.First();

                // Update Trade
                int unusedQuantity = tradeKeyValuePair.Value.Add(executionId, executionSize, executionPrice, executionTime);

                // Check if the Current Trade is Complete
                if (tradeKeyValuePair.Value.IsComplete())
                {
                    // Remove from Open Trade's MAP
                    _openTrades.Remove(tradeKeyValuePair.Key);

                    // Persist Completed Trade
                    PersistTrade(tradeKeyValuePair.Value);

                    // Update Remaining Open Trades if there is Un-Used Execution Quantity
                    if (unusedQuantity > 0)
                    {
                        // All Open Trades have closed but we still have un-used Execution Quantity
                        if (_openTrades.Count == 0)
                        {
                            // Open New Trade
                            AddNewTrade(_position > 0 ? TradeSide.Buy : TradeSide.Sell, unusedQuantity, executionPrice, executionId, security, executionTime);

                            // Terminate Loop
                            break;
                        }

                        // Use Unsed Quantity for the open trades
                        executionSize = unusedQuantity;

                        // Continue Loop Cycle
                        continue;
                    }
                }

                // Terminate Loop
                break;
            }
        }

        /// <summary>
        /// Updates Direction i.e. Negative or Positive
        /// </summary>
        /// <param name="executionSide">Execution Size</param>
        /// <param name="position">Position to manage direction</param>
        /// <param name="tradeSide">Trade Side</param>
        private void UpdateTradeDirection(string executionSide, ref int position, ref TradeSide tradeSide)
        {
            // Set 'Negative' sign if the execution is 'SELL'/'SHORT'
            if (executionSide.Equals(OrderSide.SELL) || executionSide.Equals(OrderSide.SHORT))
            {
                // Set Position to 'Negative'
                position *= -1;

                // Update Trade Side
                tradeSide = TradeSide.Sell;
            }
        }

        /// <summary>
        /// Persist the received Trade Object
        /// </summary>
        /// <param name="trade">Trade to be stored</param>
        private void PersistTrade(Trade trade)
        {
            try
            {
                // Persist Trade
                PersistencePublisher.PublishDataForPersistence(trade);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "PersistTrade");
            }
        }

        /// <summary>
        /// Checks whether the two integers have same sign (positive or negative)
        /// </summary>
        /// <param name="x">First parameter</param>
        /// <param name="y">Second parameter</param>
        /// <returns>Bool value indicating if both values had the same sign</returns>
        private bool HaveSameSign(int x, int y)
        {
            return ((x < 0) == (y < 0));
        }
    }
}
