using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using TraceSourceLogger;
using TradeHub.Common.Core.Constants;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.ValueObjects.MarketData;
using TradeHub.MarketDataEngine.BarFactory.Interfaces;

namespace TradeHub.MarketDataEngine.BarFactory.Service
{
    /// <summary>
    /// Provides required Bars
    /// </summary>
    public class BarFactory
    {
        private Type _type = typeof (BarFactory);

        private ConcurrentDictionary<string, IBarGenerator> _barGenerators;
        private Dictionary<string, int> _subscriberCount;
     
        /// <summary>
        /// Default constructor
        /// </summary>
        public BarFactory()
        {
            _barGenerators = new ConcurrentDictionary<string, IBarGenerator>();
            _subscriberCount = new Dictionary<string, int>();
        }

        /// <summary>
        /// subscribes to the bar with given specifications
        /// </summary>
        /// <param name="barDataRequest">TradeHub Bar Data Reqeust Message</param>
        /// <param name="key">Unique Key to identify the Bar</param>
        /// <param name="barAction">The bar event that the subscribing code needs to hook the bar event to</param>
        public void Subscribe(BarDataRequest barDataRequest, string key, Action<Bar, string> barAction)
        {
            try
            {
                if (barDataRequest.BarSeed.Equals(default(decimal)))
                {
                    Subscribe(barDataRequest.Security, key, barDataRequest.BarFormat, barDataRequest.BarLength,
                              barDataRequest.PipSize, barDataRequest.BarPriceType, barDataRequest.MarketDataProvider,
                              barAction);
                }
                else
                {
                    Subscribe(barDataRequest.Security, key, barDataRequest.BarFormat, barDataRequest.BarLength,
                              barDataRequest.PipSize, barDataRequest.BarPriceType, barDataRequest.MarketDataProvider,
                              barAction, barDataRequest.BarSeed);

                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "Subscribe");
            }
        }

        /// <summary>.
        /// Subscribes the calling space to the bar specifications needed
        /// </summary>
        /// <param name="security">TradeHub Security.</param>
        /// <param name="barGeneratorKey">Unique Key to identify the Bar</param>
        /// <param name="barFormat">The type of bar needed, time based or engineered</param>
        /// <param name="barLength">The length of bar in seconds or pips</param>
        /// <param name="pipSize">The size of pip being used</param>
        /// <param name="barPriceType">Build the bar using bid, ask or mid price</param>
        /// <param name="marketDataProvider">Name of Market Data Provider for which to create bars</param>
        /// <param name="barEvent">The bar event that the subscribing code needs to hook the bar event to</param>
        /// <returns></returns>
        private void Subscribe(Security security, string barGeneratorKey, string barFormat, decimal barLength,
                                       decimal pipSize, string barPriceType, string marketDataProvider,Action<Bar,string> barEvent)
        {
            try
            {
                lock (_barGenerators)
                {
                    IBarGenerator barGenerator;
                    if (_barGenerators.TryGetValue(barGeneratorKey, out barGenerator))
                    {
                        _subscriberCount[barGeneratorKey] = _subscriberCount[barGeneratorKey] + 1;
                        barGenerator.BarArrived += barEvent;
                        if (Logger.IsDebugEnabled)
                        {
                            Logger.Debug(
                                "Updating count of bar generator to bar factory for key: " + barGeneratorKey +
                                ". New count:" + _subscriberCount[barGeneratorKey],
                                "Infrastructure.BarEngine.BarFactory", "Subscribe");

                        }
                    }
                    else
                    {
                        // Get Required Bar Generator
                        barGenerator = GetBarGenerator(barFormat, security, barLength, barPriceType, pipSize, null, barGeneratorKey, marketDataProvider);

                        if (barGenerator != null)
                        {
                            if (Logger.IsDebugEnabled)
                            {
                                Logger.Debug("Adding bar generator to bar factory for key: " + barGeneratorKey,
                                             _type.FullName, "Subscribe");
                            }
                            barGenerator.BarArrived += barEvent;
                            _barGenerators.TryAdd(barGeneratorKey, barGenerator);
                            _subscriberCount.Add(barGeneratorKey, 1);
                        }
                        else
                        {
                            Logger.Info("Not able to get the Bar Generator of requested type: " + barFormat, _type.FullName, "Subscribe");
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception.ToString(), _type.FullName, "Subscribe");
            }
        }

        /// <summary>.
        /// Subscribes the calling space to the bar specifications needed with Bar Seed value
        /// </summary>
        /// <param name="security">TradeHub Security.</param>
        /// <param name="barGeneratorKey">Unique Key to identify the Bar</param>
        /// <param name="barFormat">The type of bar needed, time based or engineered</param>
        /// <param name="barLength">The length of bar in seconds or pips</param>
        /// <param name="pipSize">The size of pip being used</param>
        /// <param name="barPriceType">Build the bar using bid, ask or mid price</param>
        /// <param name="marketDataProvider">Name of Market Data Provider for which to create bars</param>
        /// <param name="barEvent">The bar event that the subscribing code needs to hook the bar event to</param>
        /// <param name="barSeed"> </param>
        /// <returns></returns>
        private void Subscribe(Security security, string barGeneratorKey, string barFormat, decimal barLength,
                                       decimal pipSize, string barPriceType, string marketDataProvider, Action<Bar, string> barEvent, decimal barSeed)
        {
            try
            {
                lock (_barGenerators)
                {
                    IBarGenerator barGenerator;
                    if (_barGenerators.TryGetValue(barGeneratorKey, out barGenerator))
                    {
                        _subscriberCount[barGeneratorKey] = _subscriberCount[barGeneratorKey] + 1;
                        barGenerator.BarArrived += barEvent;

                        if (Logger.IsDebugEnabled)
                        {
                            Logger.Debug(
                                "Updating count of bar generator to bar factory for key: " + barGeneratorKey +
                                ". New count:" + _subscriberCount[barGeneratorKey],
                                 _type.FullName, "Subscribe");
                        }
                    }

                    else
                    {
                        // Get Required Bar Generator
                        barGenerator = GetBarGenerator(barFormat, security, barLength, barPriceType, pipSize, barSeed, barGeneratorKey, marketDataProvider);

                        if (barGenerator != null)
                        {
                            if (Logger.IsDebugEnabled)
                            {
                                Logger.Debug("Adding bar generator to bar factory for key: " + barGeneratorKey,
                                             _type.FullName, "Subscribe");
                            }
                            barGenerator.BarArrived += barEvent;
                            _barGenerators.TryAdd(barGeneratorKey, barGenerator);
                            _subscriberCount.Add(barGeneratorKey, 1);
                        }
                        else
                        {
                            Logger.Info("Not able to get the Bar Generator of requested type: " + barFormat, _type.FullName, "Subscribe");
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "Subscribe");
            }
        }

        /// <summary>
        /// Receives the new tick and applies it to all current bar generators
        /// </summary>
        /// <param name="tick"></param>
        public void Update(Tick tick)
        {
            try
            {
                lock (_barGenerators)
                {
                    foreach (IBarGenerator barGenerator in _barGenerators.Values)
                    {
                        if (tick.Security.Symbol.Equals(barGenerator.Security.Symbol)/* &&
                            ((tick.HasBid&& barGenerator.BarPriceType == BarPriceType.BID) ||
                            (tick.HasAsk && barGenerator.BarPriceType == BarPriceType.ASK) ||
                            barGenerator.BarPriceType == BarPriceType.MEAN)*/)
                        {
                            barGenerator.Update(tick);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception.ToString(), _type.FullName, "Update");
            }
        }

        /// <summary>
        /// Unsubscribe Bar with given specifications
        /// </summary>
        /// <param name="barDataRequest">TradeHub Bar Data Request Message</param>
        /// <param name="barGeneratorKey">Unique Key to identify Bar</param>
        /// <param name="barEvent">The bar event that the subscribing code needs to hook the bar event to</param>
        public void Unsubscribe(BarDataRequest barDataRequest, string barGeneratorKey, Action<Bar, string> barEvent)
        {
            try
            {
                Unsubscribe(barDataRequest.Security, barDataRequest.BarFormat, barDataRequest.BarLength,
                            barDataRequest.PipSize, barDataRequest.BarPriceType, barEvent, barGeneratorKey);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "Unsubscribe");
            }
        }

        /// <summary>.
        /// Unsubscribes the calling space to the bar specifications mentioned
        /// </summary>
        ///<param name="security">TradeHub Security</param>
        ///<param name="barFormat">The type of bar needed, time based or engineered</param>
        ///<param name="barLength">The length of bar in seconds or pips</param>
        ///<param name="pipSize">The size of pip being used</param>
        ///<param name="barPriceType">Build the bar using bid, ask or mid price</param>
        ///<param name="barEvent">The bar event that the subscribing code needs to hook the bar event to</param>
        ///<param name="barGeneratorKey">Unique Key to identify Bar</param>
        ///<returns></returns>
        private void Unsubscribe(Security security, string barFormat, decimal barLength, decimal pipSize, string barPriceType,
            Action<Bar, string> barEvent, string barGeneratorKey)
        {
            try
            {
                lock (_barGenerators)
                {
                    IBarGenerator barGenerator;

                    if (Logger.IsDebugEnabled)
                    {
                        Logger.Debug("Going to remove bar generator for key: " + barGeneratorKey,
                                     _type.FullName, "Unsubscribe");
                    }
                    if (_barGenerators.TryGetValue(barGeneratorKey, out barGenerator))
                    {
                        _subscriberCount[barGeneratorKey] = _subscriberCount[barGeneratorKey] - 1;
                        barGenerator.BarArrived -= barEvent;
                        if (Logger.IsDebugEnabled)
                        {
                            Logger.Debug("Bar event removed for key: " + barGeneratorKey,
                                         _type.FullName, "Unsubscribe");
                        }
                        if (_subscriberCount[barGeneratorKey] == 0)
                        {
                            if (barFormat.Equals(BarFormat.TIME))
                            {
                                var tempGenerator = (TimeBasedBarGenerator)_barGenerators[barGeneratorKey];
                                tempGenerator.DisposeTimer();
                            }

                            IBarGenerator removedBarGenerator;
                            _barGenerators.TryRemove(barGeneratorKey, out removedBarGenerator);
                            _subscriberCount.Remove(barGeneratorKey);
                            if (Logger.IsDebugEnabled)
                            {
                                Logger.Debug("Bar engine count was 0 for key: " + barGeneratorKey,
                                             _type.FullName, "Unsubscribe");
                            }
                        }

                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "Unsubscribe");
            }
        }

        /// <summary>
        /// Return required Bar Generator Object
        /// </summary>
        /// <param name="barFormat">Decides which type of Bar Generator to use</param>
        /// <param name="security">TradeHub Security</param>
        /// <param name="barLength">Lenght of required Bar</param>
        /// <param name="barPriceType">Price to be used for creating Bar</param>
        /// <param name="pipSize"></param>
        /// <param name="barSeed"></param>
        /// <param name="key"> </param>
        /// <param name="marketDataProvider"> Market Data Proivder Name for which to bars are created</param>
        /// <returns></returns>
        private IBarGenerator GetBarGenerator(string barFormat, Security security, decimal barLength,
            string barPriceType, decimal pipSize,decimal? barSeed, string key, string marketDataProvider)
        {
            try
            {
                if (barFormat.Equals(BarFormat.TIME))
                {
                    return new TimeBasedBarGenerator(security, key, barLength, 250, barPriceType, marketDataProvider);
                }
                else if (barFormat.Equals(BarFormat.EQUAL_ENGINEERED))
                {
                    if (barSeed != null) return new EngineeredEqualBarGenerator(security, key, pipSize, barLength, barPriceType, barSeed.Value);
                    return new EngineeredEqualBarGenerator(security, key, pipSize, barLength, barPriceType);
                }
                else if (barFormat.Equals(BarFormat.UNEQUAL_ENGINEERED))
                {
                    if (barSeed != null) return new EngineeredUnequalBarGenerator(security, key, pipSize, barLength, barPriceType, barSeed.Value);
                    return new EngineeredUnequalBarGenerator(security, key, pipSize, barLength, barPriceType);
                }
                else if (barFormat.Equals(BarFormat.DISPLACEMENT))
                {
                    if (barSeed != null) return new DisplacementBarGenerator(security, key, pipSize, barLength, barPriceType, barSeed.Value);
                    return new DisplacementBarGenerator(security, key, pipSize, barLength, barPriceType);
                }
                return null;
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "GetBarGenerator");
                return null;
            }
        }
    }
}
