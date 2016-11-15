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
using System.Threading;
using System.Timers;
using TraceSourceLogger;
using TradeHub.Common.Core.DomainModels;
using TradeHub.MarketDataEngine.BarFactory.Interfaces;
using TradeHub.MarketDataEngine.BarFactory.Utility;
using TradeHubBarPriceType = TradeHub.Common.Core.Constants.BarPriceType;

namespace TradeHub.MarketDataEngine.BarFactory.Service
{
    /// <summary>
    /// Generates Time Based Bars from incoming Ticks
    /// </summary>
    internal class TimeBasedBarGenerator : ITimeBasedBarGenerator
    {
        private Type _type = typeof (TimeBasedBarGenerator);

        public event Action<Bar, string> BarArrived;

        private DateTime? _cutoffTimestampSecond = null;
        private DateTime? _currentTimestampSecond = null;

        private readonly int _lateEventSlackMilliseconds = 0;    // Buffer for late ticks
        private readonly string _marketDataProvider;
        private readonly int _barWindowLengthInSeconds;          // Window length in seconds
        private readonly Security _security = null;
        private Decimal? _open = null;
        private Decimal? _close = null;
        private Decimal? _high = null;
        private Decimal? _low = null;

        private Bar _lastBar = null;
        private DateTime? _currentTimeStamp = null;
        private BarTimer _timer = null;

        private readonly Object _postDataLock = new Object();
        private bool _firstTick = true;
        private int _noOfTicksProcessedInCurrentBar = 0;

        private int _noOfSecondsToAdd = 0;
        private int _timerElapsedTime = 0;

        public string BarPriceType { get; set; }
        public string BarGeneratorKey { get; set; }

        /// <summary>
        /// Returns the bar window length in seconds
        /// </summary>
        public int BarWindowLengthInSeconds
        {
            get
            {
                return  _barWindowLengthInSeconds;
            }
        }

        /// <summary>
        /// Returns the Security
        /// </summary>
        public Security Security
        {
            get
            {
                return  _security;
            }
        }

        /// <summary>
        /// Argument constructor
        /// </summary>
        /// <param name="security"></param>
        /// <param name="barGeneratorKey">Key to identify the Bars Produced</param>
        /// <param name="windowLengthInSeconds"></param>
        /// <param name="lateEventSlackMilliseconds"> </param>
        /// <param name="barPriceType"> </param>
        /// <param name="marketDataProvider">Market Data Provider which is used to generate Bars</param>
        public TimeBasedBarGenerator(Security security, string barGeneratorKey, decimal windowLengthInSeconds, int lateEventSlackMilliseconds, string barPriceType, string marketDataProvider)
        {
            _security = security;
            _barWindowLengthInSeconds = Convert.ToInt32(windowLengthInSeconds);
            _lateEventSlackMilliseconds = lateEventSlackMilliseconds;
            _marketDataProvider = marketDataProvider;

            BarGeneratorKey = barGeneratorKey;
            BarPriceType = barPriceType;
        }

        /// <summary>
        /// Update the OHLC values
        /// </summary>
        public void Update(Tick tick)
        {
            try
            {
                if (tick == null)
                {
                    if (Logger.IsDebugEnabled)
                    {
                        Logger.Debug( _security + " - The tick is null.", _type.FullName, "Update");
                    }
                    return;
                }

                if (!_security.Symbol.Equals(tick.Security.Symbol))
                {
                    if (Logger.IsDebugEnabled)
                    {
                        Logger.Debug( _security + " - Symbols don't match.", _type.FullName, "Update");
                    }
                    return;
                }
                if (!((tick.HasBid && BarPriceType == TradeHubBarPriceType.BID) || (tick.HasAsk && BarPriceType == TradeHubBarPriceType.ASK) ||
                      BarPriceType == TradeHubBarPriceType.MEAN || ((tick.LastPrice > 0) && BarPriceType == TradeHubBarPriceType.LAST)))
                {
                    if (Logger.IsDebugEnabled)
                    {
                        Logger.Debug("Price required to update bar is not available", _type.FullName, "Update");
                    }

                    return;
                }

                decimal price = tick.LastPrice;
                if (BarPriceType == TradeHubBarPriceType.ASK)
                    price = tick.AskPrice;
                else if (BarPriceType == TradeHubBarPriceType.BID)
                    price = tick.BidPrice;
                else if ( BarPriceType == TradeHubBarPriceType.MEAN)
                    price = (tick.BidPrice + tick.AskPrice)/2;

                 _currentTimeStamp = tick.DateTime;

                var timestampSecond = tick.DateTime.RoundSecond();

                if (( _cutoffTimestampSecond != null) &&
                    ( _cutoffTimestampSecond > timestampSecond))
                {
                    if (Logger.IsDebugEnabled)
                    {
                        Logger.Debug(
                             _security + " - The tick is late - " + tick.DateTime.ToString("yyyy-MM-dd HH:mm:ss:fff") +
                            " - CutoffTimestampSecond - " +
                             _cutoffTimestampSecond.Value.ToString("yyyy-MM-dd HH:mm:ss:fff"),
                            _type.FullName, "Update");
                    }
                    return;
                }

                // If the same window, aggregate
                if (timestampSecond <  _currentTimestampSecond)
                {
                    ApplyValue(price);
                }
                // First time we see an event for  window
                else
                {
                    lock ( _postDataLock)
                    {
                        // There is data to post and ticks were processed in last window.
                        if ( _currentTimestampSecond != null)
                        {
                            if (Logger.IsDebugEnabled)
                            {
                                Logger.Debug( _security + " - Update is posting regular bar.", _type.FullName, "Update");
                            }
                            PostData(false);
                        }
                        // There is data to post and NO ticks were processed in last window and  is not the first tick & its not already posted.
                        else if (! _firstTick && timestampSecond >  _cutoffTimestampSecond.Value.AddSeconds( _barWindowLengthInSeconds))
                        {
                            if (Logger.IsDebugEnabled)
                            {
                                Logger.Debug( _security + " - Update is posting missing bar.", _type.FullName, "Update");
                            }
                            PostData(true);
                        }

                        // _noOfSecondsToAdd =  _barWindowLengthInSeconds - (timestampSecond.Second %  _barWindowLengthInSeconds);
                         _noOfSecondsToAdd = AdjustNoOfSecondsToAdd(timestampSecond);
                         _currentTimestampSecond = timestampSecond.AddSeconds( _noOfSecondsToAdd);

                        ApplyValue(price);
                        // Update timer for each window
                        ScheduleCallback();
                        // First tick check
                        if ( _firstTick)
                        {
                             _firstTick = false;
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "Update");
            }
        }

        /// <summary>
        /// Apply OHLC values
        /// </summary>
        /// <param name="value"></param>
        private void ApplyValue(decimal value)
        {
            Interlocked.Increment(ref  _noOfTicksProcessedInCurrentBar);
            if ( _open == null)
            {
                 _open = value;
                 _low = value;
                 _high = value;
            }

             _close = value;

            if ( _low > value)
            {
                 _low = value;
            }

            if ( _high < value)
            {
                 _high = value;
            }

            if (Logger.IsDebugEnabled)
            {
                Logger.Debug( _security + " - New value applied - " + value, _type.FullName, "ApplyValue");
            }
        }

        /// <summary>
        /// Schedule callback
        /// </summary>
        private void ScheduleCallback()
        {
            try
            {
                if (_timer != null)
                {
                    _timer.Elapsed -= new ElapsedEventHandler(TimerElapsed);
                    _timer.Enabled = false;
                    _timer.Dispose();
                    _timer = null;
                    if (Logger.IsDebugEnabled)
                    {
                        Logger.Debug(_security + " - Timer disposed by ScheduleCallback.", _type.FullName, "ScheduleCallback");
                    }
                }

                // Use the time of tick to figure out when to schedule callback
                var currentTimeInSeconds = Helper.RoundSecond(_currentTimeStamp.Value);

                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug(_security + " - Current time - " + currentTimeInSeconds, _type.FullName, "ScheduleCallback");
                }

                var targetTimeInSeconds = AdjustTargetTime(currentTimeInSeconds).AddMilliseconds(_lateEventSlackMilliseconds);

                //var targetTimeInSeconds = currentTimeInSeconds.AddSeconds( _noOfSecondsToAdd).AddMilliseconds(_lateEventSlackMilliseconds);

                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug(_security + " - Target time - " + targetTimeInSeconds, _type.FullName, "ScheduleCallback");
                }

                var scheduleAfterMSec = (long)((targetTimeInSeconds - _currentTimeStamp.Value).TotalMilliseconds);

                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug("Time Remaining: " + (targetTimeInSeconds - _currentTimeStamp.Value), _type.FullName, "ScheduleCallBack");
                    Logger.Debug(_security + " - Call back event after milliseconds - " + scheduleAfterMSec, _type.FullName, "ScheduleCallback");
                }

                _timer = new BarTimer(scheduleAfterMSec);
                _timer.TimeStampSecond = _currentTimestampSecond.Value;
                _timer.Enabled = true;
                _timer.Elapsed += new ElapsedEventHandler(TimerElapsed);


            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "ScheduleCallback");
            }
        }

        /// <summary>
        /// Post data
        /// </summary>
        private void PostData(bool postOldBar)
        {
            try
            {
                Bar bar;

                if (!postOldBar)
                {
                    bar = new Bar(new Security { Symbol = _security.Symbol }, _marketDataProvider, "",_currentTimestampSecond.Value)
                        {
                            Open = _open.Value,
                            Close = _close.Value,
                            High = _high.Value,
                            Low = _low.Value,
                            Volume = 0
                        };

                    _cutoffTimestampSecond = _currentTimestampSecond;
                }
                else
                {
                    bar = new Bar(new Security { Symbol = _security.Symbol }, _marketDataProvider, "",
                                  _lastBar.DateTime.AddSeconds(_barWindowLengthInSeconds))
                        {
                            Open = _lastBar.Open,
                            Close = _lastBar.Close,
                            High = _lastBar.High,
                            Low = _lastBar.Low,
                            Volume = _lastBar.Volume,
                            IsBarCopied = true
                        };

                    _cutoffTimestampSecond = _lastBar.DateTime.AddSeconds(_noOfSecondsToAdd);
                }

                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug(_security + " - Posting new bar at " + DateTime.Now + " - Bar - " + bar
                                 + " | Number of ticks processed - " + _noOfTicksProcessedInCurrentBar,
                                 _type.FullName, "PostData");
                }

                // Post new bar.
                if (BarArrived != null)
                {
                    BarArrived(bar, BarGeneratorKey);
                }

                _lastBar = bar;
                _open = null;
                _close = null;
                _high = null;
                _low = null;
                _currentTimestampSecond = null;
                Interlocked.Exchange(ref  _noOfTicksProcessedInCurrentBar, 0);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "PostData");
            }
        }

        /// <summary>
        /// On timer elapsed post a bar.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                BarTimer localTimer = (BarTimer)sender;
                DateTime timestampSecond = localTimer.TimeStampSecond;

                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug( _security + " - TimerElapsed called for bar - " + timestampSecond,
                             _type.FullName, "TimerElapsed");
                }

                if ( _timer != null)
                {
                     _timer.Interval =  _barWindowLengthInSeconds * 1000;

                    //AdjustTargetTime(Util.RoundSecond(_currentTimeStamp.Value));
                    // _timer.Interval =  _timerElapsedTime * 1000;

                     _timer.TimeStampSecond = timestampSecond.AddSeconds( _barWindowLengthInSeconds);
                    if (Logger.IsDebugEnabled)
                    {
                        Logger.Debug( _security + " - Timer reset to be called for bar - " +  _timer.TimeStampSecond,
                             _type.FullName, "TimerElapsed");
                    }
                }

                lock ( _postDataLock)
                {
                    // There is data to post and ticks were processed in last window
                    if ( _currentTimestampSecond != null)
                    {
                        if (timestampSecond ==  _currentTimestampSecond)
                        {
                            if (Logger.IsDebugEnabled)
                            {
                                Logger.Debug( _security + " - TimerElapsed is posting regular bar.", _type.FullName, "TimerElapsed");
                            }
                            PostData(false);
                        }
                        else
                        {
                            if (Logger.IsDebugEnabled)
                            {
                                Logger.Debug( _security + " - TimerElapsed - CurrentTimestampMinute!=null && Timer.TimeStampMinute!=CurrentTimestampMinute - " +
                                    timestampSecond + "," +  _currentTimestampSecond, _type.FullName, "TimerElapsed");
                            }
                        }
                    }
                    // There is data to post and NO ticks were processed in last window
                    else
                    {
                        if (Logger.IsDebugEnabled)
                        {
                            Logger.Debug( _security + " - TimerElapsed is posting missing bar.", _type.FullName, "TimerElapsed");
                        }
                        PostData(true);
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception.ToString(), _type.FullName, "TimerElapsed");
            }
        }

        /// <summary>
        /// Disposes the timer.
        /// </summary>
        public void DisposeTimer()
        {
            if ( _timer != null)
            {
                 _timer.Elapsed -= TimerElapsed;
                 _timer.Enabled = false;
                 _timer.Dispose();
                 _timer = null;
                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug( _security + " - Timer disposed by DisposeTimer.", _type.FullName, "DisposeTimer");
                }
            }
        }

        /// <summary>
        /// Adjust Target Time
        /// </summary>
        /// <param name="dateTime"> </param>
        private DateTime AdjustTargetTime(DateTime dateTime)
        {
            try
            {
                DateTime targetTime;
                if (_barWindowLengthInSeconds > 60)
                {
                    var currentMinutes = dateTime.Minute;
                    var diff = currentMinutes%(_barWindowLengthInSeconds/60);
                    var toAdd = (_barWindowLengthInSeconds/60) - diff;
                    var actualMinutes = currentMinutes + toAdd;

                    var actualHour = (actualMinutes >= 60) ? dateTime.Hour + 1 : dateTime.Hour;

                    if (actualMinutes >= 60)
                    {
                        actualMinutes -= 60;
                    }

                    if (Logger.IsDebugEnabled)
                    {
                        Logger.Debug("Actual Minutes: " + actualMinutes, _type.FullName, "AdjustTargetTime");
                    }

                    targetTime = new DateTime(dateTime.Year,
                                                  dateTime.Month,
                                                  dateTime.Day,
                                                  actualHour,
                                                  actualMinutes,
                                                  0, 0);
                    _timerElapsedTime = toAdd * 60;
                }
                else
                {
                    targetTime = dateTime.AddSeconds( _noOfSecondsToAdd);
                    _timerElapsedTime = _barWindowLengthInSeconds;
                }
                return targetTime;
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "AdjustTargetTime");
                return dateTime.AddSeconds( _noOfSecondsToAdd);
            }
        }

        /// <summary>
        /// Adjust CurrentTimeStamp value
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        private int AdjustNoOfSecondsToAdd(DateTime dateTime)
        {
            try
            {
                int noOfSecondsToAdd;
                if (_barWindowLengthInSeconds > 60)
                {
                    var currentMinutes = dateTime.Minute;
                    var diff = currentMinutes % (_barWindowLengthInSeconds / 60);
                    var currentSeconds = (diff * 60) + dateTime.Second;

                    noOfSecondsToAdd = _barWindowLengthInSeconds - (currentSeconds %  _barWindowLengthInSeconds);

                    if (Logger.IsDebugEnabled)
                    {
                        Logger.Debug("No Of Seconds to Add: " + noOfSecondsToAdd, _type.FullName, "AdjustCurrentTimeStamp");
                    }
                }
                else
                {
                    noOfSecondsToAdd = _barWindowLengthInSeconds - (dateTime.Second %  _barWindowLengthInSeconds);
                }
                return noOfSecondsToAdd;
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "AdjustCurrentTimeStamp");
                return  _barWindowLengthInSeconds - (dateTime.Second %  _barWindowLengthInSeconds);
            }
        }
    }
}
