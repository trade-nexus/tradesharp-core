using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Disruptor;
using TraceSourceLogger;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.ValueObjects;
using TradeHub.Common.HistoricalDataProvider.ValueObjects;

namespace TradeHub.Common.HistoricalDataProvider.Utility
{
    /// <summary>
    /// Listens incoming market data intended for strategies and triggers appropriate events
    /// </summary>
    public class MarketDataListener : IEventHandler<RabbitMqRequestMessage>, IEventHandler<MarketDataObject>
    {
        private Type _type = typeof (MarketDataListener);
        private AsyncClassLogger _asyncClassLogger;

        public event Action<Tick> TickArrived;
        public event Action<Bar> BarArrived;

        /// <summary>
        /// Will contain all the symbols for which bar is subscribed
        /// </summary>
        private IList<string> _barSubscriptionList;

        /// <summary>
        /// Will contain all the symbols for which tick is subscribed
        /// </summary>
        private IList<string> _tickSubscriptionList;

        /// <summary>
        /// Argument Constuctor
        /// </summary>
        /// <param name="asyncClassLogger"> </param>
        public MarketDataListener(AsyncClassLogger asyncClassLogger)
        {
            _asyncClassLogger = asyncClassLogger;
        }

        /// <summary>
        /// Will contain all the symbols for which bar is subscribed
        /// </summary>
        public IList<string> BarSubscriptionList
        {
            get { return _barSubscriptionList; }
            set { _barSubscriptionList = value; }
        }

        /// <summary>
        /// Will contain all the symbols for which tick is subscribed
        /// </summary>
        public IList<string> TickSubscriptionList
        {
            get { return _tickSubscriptionList; }
            set { _tickSubscriptionList = value; }
        }

        #region Handler incoming Market Data

        /// <summary>
        /// Called when new Tick message is received and processed by Disruptor
        /// </summary>
        /// <param name="message"></param>
        private void OnTickDataReceived(string[] message)
        {
            try
            {
                Tick tick = new Tick();

                // Parse incoming message to Tick
                if (ParseToTick(tick, message))
                {
                    // Notify Listeners
                    TickArrived(tick);
                }
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "OnTickDataReceived");
            }
        }

        /// <summary>
        /// Called when new Bar message is received and processed by Disruptor
        /// </summary>
        /// <param name="message"></param>
        private void OnBarDataReceived(string[] message)
        {
            try
            {
                Bar bar = new Bar("");

                // Parse incoming message to Bar
                if (ParseToBar(bar, message))
                {
                    // Notify Listeners
                    BarArrived(bar);
                }
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "OnBarDataReceived");
            }
        }

        #endregion

        #region Market Data Parsing

        /// <summary>
        /// Creats tick object from incoming string message
        /// </summary>
        /// <param name="tick">Tick to add values to</param>
        /// <param name="message">Received message</param>
        /// <returns></returns>
        private bool ParseToTick(Tick tick, string[] message)
        {
            try
            {
                // Get Bid Values
                tick.BidPrice = Convert.ToDecimal(message[1]);
                tick.BidSize = Convert.ToDecimal(message[2]);

                // Get Ask Values
                tick.AskPrice = Convert.ToDecimal(message[3]);
                tick.AskSize = Convert.ToDecimal(message[4]);

                // Get Last Values
                tick.LastPrice = Convert.ToDecimal(message[5]);
                tick.LastSize = Convert.ToDecimal(message[6]);

                // Get Symbol
                tick.Security = new Security() {Symbol = message[7]};
                // Get Time Value
                tick.DateTime = DateTime.ParseExact(message[8], "M/d/yyyy h:mm:ss.fff tt", CultureInfo.InvariantCulture);
                // Get Provider name
                tick.MarketDataProvider = message[9];
                return true;
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "ParseToTick");
                return true;
            }
        }

        /// <summary>
        /// Parse String into bar
        /// </summary>
        /// <param name="bar">Bar to add values to</param>
        /// <param name="message">Received message</param>
        /// <returns></returns>
        private bool ParseToBar(Bar bar, string[] message)
        {
            try
            {
                bar.Security = new Security {Symbol = message[6]};
                bar.Close = Convert.ToDecimal(message[1]);
                bar.Open = Convert.ToDecimal(message[2]);
                bar.High = Convert.ToDecimal(message[3]);
                bar.Low = Convert.ToDecimal(message[4]);
                bar.Volume = Convert.ToInt64((message[5]));
                bar.DateTime = DateTime.ParseExact(message[7], "M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture);
                bar.MarketDataProvider = message[8];
                bar.RequestId = message[9];
                return true;
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "ParseToBar");
                return false;
            }
        }

        #endregion

        #region Implementation of IEventHandler<in RabbitMqMessage>

        /// <summary>
        /// Called when a publisher has committed an event to the <see cref="T:Disruptor.RingBuffer`1"/>
        /// </summary>
        /// <param name="data">Data committed to the <see cref="T:Disruptor.RingBuffer`1"/></param><param name="sequence">Sequence number committed to the <see cref="T:Disruptor.RingBuffer`1"/></param><param name="endOfBatch">flag to indicate if this is the last event in a batch from the <see cref="T:Disruptor.RingBuffer`1"/></param>
        public void OnNext(RabbitMqRequestMessage data, long sequence, bool endOfBatch)
        {
            string message = Encoding.UTF8.GetString(data.Message);

            var messageArray = message.Split(',');

            if (messageArray[0].Equals("TICK"))
                OnTickDataReceived(messageArray);
            else
                OnBarDataReceived(messageArray);
        }

        #endregion

        #region Implementation of IEventHandler<in MarketDataObject>

        /// <summary>
        /// Called when a publisher has committed an event to the <see cref="T:Disruptor.RingBuffer`1"/>
        /// </summary>
        /// <param name="data">Data committed to the <see cref="T:Disruptor.RingBuffer`1"/></param><param name="sequence">Sequence number committed to the <see cref="T:Disruptor.RingBuffer`1"/></param><param name="endOfBatch">flag to indicate if this is the last event in a batch from the <see cref="T:Disruptor.RingBuffer`1"/></param>
        public void OnNext(MarketDataObject data, long sequence, bool endOfBatch)
        {
            if (data.IsTick)
            {
                // Publish Tick if the subscription request is received
                if (TickSubscriptionList.Contains(data.Tick.Security.Symbol))
                {
                    TickArrived(data.Tick);
                }
            }
            else
            {
                // Publish Bar if the subscription request is received
                if (BarSubscriptionList.Contains(data.Bar.Security.Symbol))
                {
                    BarArrived(data.Bar);
                }
            }
        }

        #endregion
    }
}
