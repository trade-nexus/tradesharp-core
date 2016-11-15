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
using Disruptor.Dsl;
using TraceSourceLogger;
using TradeHub.Common.Core.Repositories;
using TradeHub.Common.Core.Utility;

namespace TradeHub.Common.Persistence
{
    /// <summary>
    /// Service to persist strategy info and orders.
    /// </summary>
    public class PersistencePublisher
    {
        private static Type _type = typeof (PersistencePublisher);

        #region Disruptor Fields
        private static readonly int _ringSize = 256;
        private static Disruptor<byte[]> _disruptor;
        private static RingBuffer<byte[]> _ringBuffer;
        private static EventPublisher<byte[]> _publisher;
        #endregion

        public static bool EnablePersistence { get; private set; }

        /// <summary>
        /// Publish data for persistence
        /// </summary>
        /// <param name="data"></param>
        public static void PublishDataForPersistence(object data)
        {
            if (EnablePersistence)
            {
                Publish(data);
            }
        }

        /// <summary>
        /// Publish Data to the disruptor
        /// </summary>
        /// <param name="obj"></param>
        private static void Publish(object obj)
        {
            byte[] received = StreamConversion.ObjectToByteArray(obj);
            _publisher.PublishEvent((entry, sequenceNo) =>
            {
                //copy byte to diruptor ring byte
                Buffer.BlockCopy(received, 0, entry, 0, received.Length);
                return entry;
            });
        }

        /// <summary>
        /// Sets whether information should be persisted or not
        /// </summary>
        /// <param name="enablePersistence"></param>
        public static void AllowPersistence(bool enablePersistence)
        {
            EnablePersistence = enablePersistence;
        }

        /// <summary>
        /// Initialize Disruptor
        /// </summary>
        public static void InitializeDisruptor(bool enablePersistence,IPersistRepository<object> persistRepository)
        {
            EnablePersistence = enablePersistence;
            try
            {
                if (enablePersistence)
                {
                    if (_disruptor == null)
                    {
                        Journaler journaler = new Journaler(persistRepository);
                        // Initialize Disruptor
                        _disruptor = new Disruptor<byte[]>(() => new byte[10000], _ringSize, TaskScheduler.Default);

                        // Add Consumer
                        _disruptor.HandleEventsWith(journaler);
                        // Start Disruptor
                        _ringBuffer = _disruptor.Start();
                        // Get Publisher
                        _publisher = new EventPublisher<byte[]>(_ringBuffer);
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "InitializeDisruptor");
            }
        }

        /// <summary>
        /// Initialize Disruptor
        /// </summary>
        public static void InitializeDisruptor(IPersistRepository<object> persistRepository)
        {
            try
            {
                if (_disruptor == null)
                {
                    Journaler journaler = new Journaler(persistRepository);
                    // Initialize Disruptor
                    _disruptor = new Disruptor<byte[]>(() => new byte[10000], _ringSize, TaskScheduler.Default);

                    // Add Consumer
                    _disruptor.HandleEventsWith(journaler);
                    // Start Disruptor
                    _ringBuffer = _disruptor.Start();
                    // Get Publisher
                    _publisher = new EventPublisher<byte[]>(_ringBuffer);
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "InitializeDisruptor");
            }
        }

        /// <summary>
        /// ShutDown
        /// </summary>
        public static void ShutDown()
        {
            if (_disruptor != null)
            {
                // Shutdown disruptor if it was already running
                _disruptor.Shutdown();
                _disruptor = null;
            }
        }
    }
}
