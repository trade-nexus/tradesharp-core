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
using Disruptor;
using TraceSourceLogger;
using TradeHub.Common.Core.DomainModels.OrderDomain;
using TradeHub.Common.Core.ValueObjects;
using TradeHub.TradeManager.Client.Service;
using Disruptor.Dsl;

namespace TradeHub.StrategyEngine.TradeService
{
    /// <summary>
    /// Provides access to Trade Manager - Server using client underneath
    /// </summary>
    public class TradeManagerService : IEventHandler<MessageQueueObject>
    {
        private Type _type = typeof (TradeManagerService);

        #region Disruptor

        /// <summary>
        /// Disruptor Ring Buffer Size 
        /// </summary>
        private readonly int _ringSize = 65536;  // Must be multiple of 2

        /// <summary>
        /// Main disruptor object
        /// </summary>
        private Disruptor<MessageQueueObject> _disruptor;

        /// <summary>
        /// Ring buffer to be used with disruptor
        /// </summary>
        private RingBuffer<MessageQueueObject> _ringBuffer;

        /// <summary>
        /// Publishes messages to Disruptor
        /// </summary>
        private EventPublisher<MessageQueueObject> _messagePublisher;

        #endregion

        /// <summary>
        /// Contains functionality to communicate with Trade Manager - Client
        /// </summary>
        private readonly TradeManagerClient _tradeManagerClient;

        /// <summary>
        /// Argument Constructor
        /// </summary>
        /// <param name="tradeManagerClient">Contains functionality to communicate with Trade Manager - Client</param>
        public TradeManagerService(TradeManagerClient tradeManagerClient)
        {
            // Save Instance
            _tradeManagerClient = tradeManagerClient;

            // Initialize Disruptor
            InitializeDisruptor();
        }

        /// <summary>
        /// Initilaizes Disruptor and relavent resources
        /// </summary>
        private void InitializeDisruptor()
        {
            // Initialize Disruptor
            _disruptor = new Disruptor<MessageQueueObject>(() => new MessageQueueObject(), _ringSize, TaskScheduler.Default);

            // Set Event Handler
            _disruptor.HandleEventsWith(this);

            // Start Ring Buffer
            _ringBuffer = _disruptor.Start();

            // Start Event Publisher
            _messagePublisher = new EventPublisher<MessageQueueObject>(_ringBuffer);
        }

        #region Start/Stop

        /// <summary>
        /// Starts Trade Manager Service
        /// </summary>
        /// <returns>Indicates whether the operation was successful or not.</returns>
        public bool StartService()
        {
            if (_tradeManagerClient != null)
            {
                // Start Client
                _tradeManagerClient.StartCommunicator();

                return true;
            }

            if (Logger.IsInfoEnabled)
            {
                Logger.Info("Client object not initialized.", _type.FullName, "StartService");
            }

            return false;
        }

        /// <summary>
        /// Stops Trade Manager Service
        /// </summary>
        /// <returns>Indicates whether the operation was successful or not.</returns>
        public bool StopService()
        {
            if (_tradeManagerClient != null)
            {
                // Stop Client
                _tradeManagerClient.StopCommunicator();

                return true;
            }

            if (Logger.IsInfoEnabled)
            {
                Logger.Info("Client object not initialized.", _type.FullName, "StopService");
            }

            return false;
        }

        #endregion

        /// <summary>
        /// Forwards Execution messages to Client to be sent to Server
        /// </summary>
        /// <param name="execution">Contains Order Execution information</param>
        public void SendExecution(Execution execution)
        {
            try
            {
                // Process request only if the Client is working
                if (_tradeManagerClient.IsConnected())
                {
                    if (Logger.IsDebugEnabled)
                    {
                        Logger.Debug("Execution received: " + execution, _type.FullName, "SendExecution");
                    }

                    // Send Execution to Disruptor
                    _messagePublisher.PublishEvent((messageQueueObject, sequenceNo) =>
                    {
                        byte[] messageBytes = Encoding.UTF8.GetBytes(execution.DataToPublish());

                        // Initialize Parameter
                        messageQueueObject.Message = new byte[messageBytes.Length];

                        // Copy information
                        messageBytes.CopyTo(messageQueueObject.Message, 0);

                        // Return updated object
                        return messageQueueObject;
                    });
                }
                else
                {
                    if (Logger.IsInfoEnabled)
                    {
                        Logger.Info("Execution not sent as Client is not connected",_type.FullName,"SendExecution");
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "SendExecution");
            }
        }

        /// <summary>
        /// Called when a publisher has committed an event to the <see cref="T:Disruptor.RingBuffer`1"/>
        /// </summary>
        /// <param name="data">Data committed to the <see cref="T:Disruptor.RingBuffer`1"/></param><param name="sequence">Sequence number committed to the <see cref="T:Disruptor.RingBuffer`1"/></param><param name="endOfBatch">flag to indicate if this is the last event in a batch from the <see cref="T:Disruptor.RingBuffer`1"/></param>
        public void OnNext(MessageQueueObject data, long sequence, bool endOfBatch)
        {
            // Send Data to Client
            _tradeManagerClient.SendExecution(data);
        }
    }
}
