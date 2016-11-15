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
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuickFix;
using QuickFix.Fields;
using QuickFix.FIX42;
using QuickFix.Transport;
using TraceSourceLogger;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.DomainModels.OrderDomain;
using TradeHub.Common.Core.OrderExecutionProvider;
using TradeHub.Common.Fix.Infrastructure;
using Constants = TradeHub.Common.Core.Constants;
using FixCommon = TradeHub.Common.Fix;
using Message = QuickFix.Message;

namespace TradeHub.OrderExecutionProvider.Currenex.Provider
{
    public class CurrenexOrderExecutionProvider : QuickFix.MessageCracker, IApplication, IMarketOrderProvider, ILimitOrderProvider
    {
        private Type _type = typeof(CurrenexOrderExecutionProvider);

        private readonly string _provider;

        private bool _isConnected = false;

        private string _fixSettingsFile;

        #region Currenex FIX Connector Members

        private string _userName = string.Empty;
        private string _password = string.Empty;
        private string _tradeSenderCompId = string.Empty;
        private string _tradeTargetCompId = string.Empty;

        private QuickFix.IInitiator _initiator = null;
        private QuickFix.SessionID _orderSessionId = null;

        #endregion

        #region Events

        public event Action<string> LogonArrived;
        public event Action<string> LogoutArrived;
        public event Action<Order> NewArrived;
        public event Action<Execution> ExecutionArrived;
        public event Action<Rejection> RejectionArrived;
        public event Action<Rejection> OrderRejectionArrived;
        public event Action<LimitOrder> OnLocateMessage;
        public event Action<Position> OnPositionMessage;
        public event Action<Order> CancellationArrived;

        #endregion

        /// <summary>
        /// Is Order Execution client connected
        /// </summary>
        /// <returns></returns>
        public bool IsConnected()
        {
            return (_orderSessionId != null && Session.LookupSession(_orderSessionId).IsLoggedOn);
        }

        /// <summary>
        /// Default Constructor
        /// </summary>
        public CurrenexOrderExecutionProvider()
        {
            // Set provider name
            _provider = Constants.OrderExecutionProvider.Currenex;

            _fixSettingsFile = AppDomain.CurrentDomain.BaseDirectory + @"\Config\" + "CurrenexFIXSettings.txt";
        }

        #region Connection Methods

        /// <summary>
        /// Connects/Starts a client
        /// </summary>
        public bool Start()
        {
            try
            {
                if (this._initiator == null)
                {
                    PopulateFixSettings();

                    SessionSettings settings = new SessionSettings(this._fixSettingsFile);
                    IApplication application = this;
                    FileStoreFactory storeFactory = new FileStoreFactory(settings);
                    FileLogFactory logFactory = new FileLogFactory(settings);
                    IMessageStoreFactory messageFactory = new FileStoreFactory(settings);

                    this._initiator = new SocketInitiator(application, storeFactory, settings,
                        logFactory);
                    this._initiator.Start();

                    if (Logger.IsInfoEnabled)
                    {
                        Logger.Info("Currenex Fix Order Client Started.", _type.FullName, "Start");
                    }
                }
                else
                {
                    if (!this._initiator.IsStopped)
                    {
                        if (Logger.IsInfoEnabled)
                        {
                            Logger.Info("Currenex Fix Order Client Already Started.", _type.FullName, "Start");
                        }
                    }
                    else
                    {
                        this._initiator.Start();
                        if (Logger.IsInfoEnabled)
                        {
                            Logger.Info("Currenex Fix Order Client Started.", _type.FullName, "Start");
                        }
                    }
                }

                return true;
            }
            catch (Exception exception)
            {
                Logger.Error(exception.ToString(), _type.FullName, "Start");
            }
            return false;
        }

        /// <summary>
        /// Disconnects/Stops a client
        /// </summary>
        public bool Stop()
        {
            try
            {
                if (!this._initiator.IsStopped)
                {
                    this._initiator.Stop();
                    this._initiator.Dispose();
                    this._initiator = null;

                    if (Logger.IsInfoEnabled)
                    {
                        Logger.Info("Currenex Fix Order Client Stoped.", _type.FullName, "Stop");
                    }
                }
                else
                {
                    if (Logger.IsInfoEnabled)
                    {
                        Logger.Info("Currenex Fix Order Client Already Stoped.", _type.FullName, "Stop");
                    }
                }
                return true;
            }
            catch (Exception exception)
            {
                Logger.Error(exception.ToString(), _type.FullName, "Stop");
            }
            return false;
        }

        #endregion

        #region Order Requests

        /// <summary>
        /// Sends Locate message Accepted/Rejected response to Broker
        /// </summary>
        /// <param name="locateResponse">TradeHub LocateResponse Object</param>
        /// <returns></returns>
        public bool LocateMessageResponse(LocateResponse locateResponse)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Send Market Order
        /// </summary>
        /// <param name="marketOrder"></param>
        public void SendMarketOrder(MarketOrder marketOrder)
        {
            try
            {
                // Create FIX order
                NewOrderSingle order = NewOrderSingle(marketOrder);

                // Send request
                Session.SendToTarget(order, _orderSessionId);

                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug("Market Order Sent " + marketOrder, _type.FullName, "SendMarketOrder");
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "SendMarketOrder");
            }
        }

        /// <summary>
        /// Send Limit Order
        /// </summary>
        /// <param name="limitOrder"></param>
        public void SendLimitOrder(LimitOrder limitOrder)
        {
            try
            {
                // Create FIX order
                NewOrderSingle order = NewOrderSingle(limitOrder);

                // Send request
                Session.SendToTarget(order, _orderSessionId);

                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug("Limit Order Sent " + limitOrder, _type.FullName, "SendLimitOrder");
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "SendLimitOrder");
            }
        }

        /// <summary>
        /// Cancel Limit order
        /// </summary>
        /// <param name="order"></param>
        public void CancelLimitOrder(Order order)
        {
            try
            {
                // Create FIX order
                OrderCancelRequest orderCancelRequest = OrderCancelRequest(order);

                // Send request
                Session.SendToTarget(orderCancelRequest, _orderSessionId);

                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug("Cancel Order Sent " + order, _type.FullName, "CancelLimitOrder");
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "CancelLimitOrder");
            }
        }

        #endregion

        #region Application Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="sessionId"></param>
        public void FromAdmin(Message message, SessionID sessionId)
        {
            Crack(message, sessionId);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="sessionId"></param>
        public void FromApp(Message message, SessionID sessionId)
        {
            Crack(message, sessionId);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sessionId"></param>
        public void OnCreate(SessionID sessionId)
        {
            //Do nothing
        }

        /// <summary>
        /// Quick fix on logon method.
        /// This callback notifies you when a valid logon has been established with a counter party. This is called when 
        /// a connection has been established and the FIX logon process has completed with both parties exchanging valid 
        /// logon messages. 
        /// </summary>
        /// <param name="sessionId"></param>
        public void OnLogon(SessionID sessionId)
        {
            try
            {
                this._isConnected = true;
                
                if (this._tradeSenderCompId.Equals(sessionId.SenderCompID))
                {
                    this._orderSessionId = sessionId;
                }

                if (LogonArrived != null)
                {
                    LogonArrived(_provider);
                }
                
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info(
                        " FIX OnLogon - SenderCompID : " + sessionId.SenderCompID + " -> TargetCompID : " +
                        sessionId.TargetCompID, _type.FullName, "OnLogon");
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception.ToString(), _type.FullName, "OnLogon");
            }
        }

        /// <summary>
        /// Quick fix on logout method.
        /// This callback notifies you when an FIX session is no longer online. This could happen during a normal logout 
        /// exchange or because of a forced termination or a loss of network connection. 
        /// </summary>
        /// <param name="sessionId"></param>
        public void OnLogout(SessionID sessionId)
        {
            try
            {
                this._isConnected = false;

                if (this._tradeSenderCompId.Equals(sessionId.SenderCompID))
                {
                    this._orderSessionId = null;
                }

                if (LogoutArrived != null)
                {
                    LogoutArrived(_provider);
                }
                
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info(
                        " FIX OnLogout - SenderCompID : " + sessionId.SenderCompID + " -> TargetCompID : " +
                        sessionId.TargetCompID, _type.FullName, "OnLogout");
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception.ToString(), _type.FullName, "OnLogout");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="sessionId"></param>
        public void ToAdmin(Message message, SessionID sessionId)
        {
            Crack(message, sessionId);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="sessionId"></param>
        public void ToApp(Message message, SessionID sessionId)
        {
            Crack(message, sessionId);
        }
        #endregion

        #region Message Crackers

        /// <summary>
        /// FIX42 message cracker
        /// </summary>
        /// <param name="message"></param>
        /// <param name="sessionId"></param>
        public void Crack(Message message, SessionID sessionId)
        {
            if (message is QuickFix.FIX42.Reject)
            {
                OnMessage((QuickFix.FIX42.Reject)(message), sessionId);
            }
            else if (message is QuickFix.FIX42.ExecutionReport)
            {
                OnMessage((QuickFix.FIX42.ExecutionReport)(message), sessionId);
            }
            else if (message is QuickFix.FIX42.OrderCancelReject)
            {
                OnMessage((QuickFix.FIX42.OrderCancelReject)(message), sessionId);
            }
            else if (message is QuickFix.FIX42.Logon)
            {
                OnMessage((QuickFix.FIX42.Logon)(message), sessionId);
            }
            else if (message is QuickFix.FIX42.TradingSessionStatus)
            {

            }
        }

        #region Logon Handler

        /// <summary>
        /// Add username and password before sending the message
        /// </summary>
        /// <param name="message"></param>
        /// <param name="sessionId"></param>
        private void OnMessage(QuickFix.FIX42.Logon message, SessionID sessionId)
        {
            try
            {
                // Username & Password
                QuickFix.Fields.Username username = new QuickFix.Fields.Username(this._userName);
                QuickFix.Fields.Password password = new QuickFix.Fields.Password(this._password);
                QuickFix.Fields.ResetSeqNumFlag resetSeqNumFlag = new QuickFix.Fields.ResetSeqNumFlag(true);

                // Set values in the message body before sending to Currenex gateway
                //message.SetField(username);
                message.SetField(password);
                message.SetField(resetSeqNumFlag);
            }
            catch (Exception exception)
            {
                Logger.Error(exception.ToString(), _type.FullName, "OnMessage");
            }
        }
        #endregion

        #region TradingSessionStatus Handler

        /// <summary>
        /// Handle Trade Session Message 
        /// NOTE: All business level messages will be rejected by currenex if sent before getting this message
        /// </summary>
        /// <param name="message"></param>
        /// <param name="sessionId"></param>
        private void OnMessage(QuickFix.FIX42.TradingSessionStatus message, SessionID sessionId)
        {
            try
            {
                //message.TradingSessionID;
                //message.TradSesStatus;
            }
            catch (Exception exception)
            {
                Logger.Error(exception.ToString(), _type.FullName, "OnMessage");
            }
        }

        #endregion

        #region Execution Handler

        /// <summary>
        /// 
        /// </summary>
        /// <param name="executionReport">Execution report</param>
        /// <param name="sessionId">Session ID</param>
        private void OnMessage(QuickFix.FIX42.ExecutionReport executionReport, SessionID sessionId)
        {
            try
            {
                switch (executionReport.ExecType.getValue())
                {
                    case ExecType.NEW:
                        {
                            Order order = PopulateOder(executionReport);
                            if (Logger.IsDebugEnabled)
                            {
                                Logger.Debug("New arrived : " + order, _type.FullName, "OnMessage");
                            }
                            if (NewArrived != null)
                            {
                                NewArrived(order);
                            }
                            break;
                        }
                    case ExecType.CANCELED:
                        {
                            Order order = PopulateOder(executionReport);
                            if (Logger.IsDebugEnabled)
                            {
                                Logger.Debug("Cancellation arrived : " + order, _type.FullName, "OnMessage");
                            }
                            if (CancellationArrived != null)
                            {
                                CancellationArrived(order);
                            }
                            break;
                        }
                    case ExecType.REJECTED:

                        var rejection = ExtractOrderRejection(executionReport);

                        if (OrderRejectionArrived != null)
                        {
                            OrderRejectionArrived(rejection);
                        }
                        break;
                    case '2': // Currenex has its own Trade Tag
                        {
                            Execution execution = PopulateExecution(executionReport);
                            if (Logger.IsDebugEnabled)
                            {
                                Logger.Debug("Trade arrived : " + execution, _type.FullName, "OnMessage");
                            }
                            if (ExecutionArrived != null)
                            {
                                ExecutionArrived(execution);
                            }
                            break;
                        }
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception.ToString(), _type.FullName, "OnMessage");
            }
        }

        /// <summary>
        /// Makes an execution object
        /// </summary>
        /// <param name="executionReport"></param>
        /// <returns></returns>
        private Execution PopulateExecution(QuickFix.FIX42.ExecutionReport executionReport)
        {
            string orderSide = FixCommon.Converter.ConvertOrderSide.GetLocalOrderSide(executionReport.Side.getValue());

            // Extract Fill information
            Fill fill = new Fill(new Security {Symbol = executionReport.Symbol.getValue()},
                _provider, executionReport.ClOrdID.getValue())
            {
                ExecutionDateTime = executionReport.TransactTime.getValue(),

                ExecutionType =
                    executionReport.OrdStatus.getValue() == FixCommon.Constants.FixOrderStatus.Filled
                        ? Constants.ExecutionType.Fill
                        : Constants.ExecutionType.Partial,

                ExecutionId = executionReport.ExecID.getValue(),
                
                ExecutionPrice = Convert.ToDecimal(executionReport.AvgPx.getValue(), CultureInfo.InvariantCulture),

                ExecutionSize =
                    Convert.ToInt32(executionReport.CumQty.getValue(), CultureInfo.InvariantCulture) -
                    Convert.ToInt32(executionReport.LeavesQty.getValue(), CultureInfo.InvariantCulture),

                ExecutionSide = orderSide,

                AverageExecutionPrice =
                    Convert.ToDecimal(executionReport.AvgPx.getValue(), CultureInfo.InvariantCulture),

                LeavesQuantity = Convert.ToInt32(executionReport.LeavesQty.getValue(), CultureInfo.InvariantCulture),

                CummalativeQuantity = Convert.ToInt32(executionReport.CumQty.getValue(), CultureInfo.InvariantCulture)
            };

            // Extract Order information
            Order order = new Order(_provider)
            {
                OrderID = executionReport.ClOrdID.getValue(),
                OrderSide = orderSide,
                OrderSize = Convert.ToInt32(executionReport.OrderQty.getValue(), CultureInfo.InvariantCulture),
                OrderTif = FixCommon.Converter.ConvertTif.GetLocalValue(executionReport.TimeInForce.getValue())
            };

            return new Execution(fill, order);
        }

        /// <summary>
        /// Makes an order object
        /// </summary>
        /// <param name="executionReport"></param>
        /// <returns></returns>
        private Order PopulateOder(QuickFix.FIX42.ExecutionReport executionReport)
        {
            string orderSide = FixCommon.Converter.ConvertOrderSide.GetLocalOrderSide(executionReport.Side.getValue());

            // Extract Order information
            Order order = new Order(_provider)
            {
                OrderID = executionReport.ExecType.getValue().Equals(ExecType.NEW) ? executionReport.ClOrdID.getValue() : executionReport.OrigClOrdID.getValue(),
                OrderSide = orderSide,
                OrderSize = Convert.ToInt32(executionReport.OrderQty.getValue(), CultureInfo.InvariantCulture),
                OrderTif = FixCommon.Converter.ConvertTif.GetLocalValue(executionReport.TimeInForce.getValue())
            };

            return order;
        }

        #endregion

        #region Business Reject Handler

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reject"></param>
        /// <param name="sessionId"></param>
        private void OnMessage(QuickFix.FIX42.Reject reject, SessionID sessionId)
        {
            try
            {
                // SessionRejectReason (373)
                // 0 = Invalid tag number
                // 1 = Required tag missing
                // 2 = Tag not defined for this message type
                // 3 = Undefined Tag
                // 4 = Tag specified without a value
                // 5 = Value is incorrect (out of range) for this tag
                // 6 = Incorrect data format for value
                // 7 = Decryption problem
                // 8 = Signature problem
                // 9 = CompID problem
                // 10 = SendingTime (52) accuracy problem
                // 11 = Invalid MsgType (35)
                // (Note other session-level rule violations may exist in which case SessionRejectReason (373) is not specified) 

                if (Logger.IsInfoEnabled)
                {
                    Logger.Info(
                        "Message rejected at business level : " +
                        reject.GetField(58).ToString(CultureInfo.InvariantCulture),
                        _type.FullName, "OnMessage");
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception.ToString(), _type.FullName, "OnMessage");
            }
        }
        
        #endregion

        #region Cancel Reject Handler

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="sessionId"></param>
        private void OnMessage(QuickFix.FIX42.OrderCancelReject message, SessionID sessionId)
        {
            try
            {
                Rejection rejection = new Rejection(new Security() { Symbol = String.Empty }, _provider, message.TransactTime.getValue());

                rejection.OrderId = message.OrigClOrdID.getValue();
                rejection.RejectioReason = message.CxlRejReason.getValue().ToString();

                if (RejectionArrived != null)
                {
                    RejectionArrived(rejection);
                }

                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("Cancel / CancelReplace rejection arrived : " + rejection.OrderId, _type.FullName, "OnMessage");
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception.ToString(), _type.FullName, "OnMessage");
            }
        }

        #endregion

        #endregion

        #region Message Creators

        #region Create New Order Single Message

        /// <summary>
        /// Creates a FIX4.2 NewOrderSingle message for Currenex
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public QuickFix.FIX42.NewOrderSingle NewOrderSingle(Order order)
        {
            var newOrderSingle = new QuickFix.FIX42.NewOrderSingle();

            var clOrdId = new QuickFix.Fields.ClOrdID(order.OrderID);
            newOrderSingle.SetField(clOrdId);

            var securityType = new QuickFix.Fields.SecurityType(FixCommon.Constants.SecurityType.ForeignExchangeContract);
            newOrderSingle.SetField(securityType);

            var symbol = new QuickFix.Fields.Symbol(order.Security.Symbol);
            newOrderSingle.SetField(symbol);

            //set order side
            if (order.OrderSide == Constants.OrderSide.BUY)
            {
                newOrderSingle.Set(new Side(Side.BUY));
            }
            else if (order.OrderSide == Constants.OrderSide.SELL)
            {
                newOrderSingle.Set(new Side(Side.SELL));
            }

            var transactTime = new QuickFix.Fields.TransactTime(order.OrderDateTime);
            newOrderSingle.SetField(transactTime);

            var orderQty = new QuickFix.Fields.OrderQty(order.OrderSize);
            newOrderSingle.SetField(orderQty);

            var minQty = new QuickFix.Fields.MinQty(order.OrderSize);
            newOrderSingle.SetField(minQty);

            //only limit and market orders are supported.
            if (order.GetType() == typeof(LimitOrder))
            {
                newOrderSingle.Set(new OrdType('F'));
                newOrderSingle.Set(new Price(((LimitOrder)order).LimitPrice));
            }
            else if (order.GetType() == typeof(MarketOrder))
            {
                newOrderSingle.Set(new OrdType('C'));
            }

            //if (order.OrderSize != order.OrderSizeToShow)
            //{
            //    var maxShow = new QuickFix.Fields.MaxShow(order.OrderSizeToShow);
            //    newOrderSingle.SetField(maxShow);
            //}

            var tif = new QuickFix.Fields.TimeInForce(FixCommon.Converter.ConvertTif.GetFixValue(order.OrderTif));
            newOrderSingle.SetField(tif);

            var execInst = new QuickFix.Fields.ExecInst(ExecInst.BEST_EXECUTION);
            newOrderSingle.SetField(execInst);

            var currency = new QuickFix.Fields.Currency(order.OrderCurrency);
            newOrderSingle.SetField(currency);

            var handlInst = new QuickFix.Fields.HandlInst(HandlInst.AUTOMATED_EXECUTION_ORDER_PRIVATE);
            newOrderSingle.SetField(handlInst);

            return newOrderSingle;
        }

        #endregion

        #region Creat Order Cancel Request Message

        /// <summary>
        /// Creates a FIX4.2 OrderCancelRequest message for Currenex
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public QuickFix.FIX42.OrderCancelRequest OrderCancelRequest(Order order)
        {
            QuickFix.FIX42.OrderCancelRequest orderCancelRequest = new QuickFix.FIX42.OrderCancelRequest();

            QuickFix.Fields.OrigClOrdID origClOrdId = new QuickFix.Fields.OrigClOrdID(order.OrderID);
            orderCancelRequest.SetField(origClOrdId);

            QuickFix.Fields.ClOrdID clOrdId = new QuickFix.Fields.ClOrdID(DateTime.Now.ToString(("yyMMddHmsfff")));
            orderCancelRequest.SetField(clOrdId);

            QuickFix.Fields.Symbol symbol = new QuickFix.Fields.Symbol(order.Security.Symbol);
            orderCancelRequest.SetField(symbol);

            QuickFix.Fields.Side side = new QuickFix.Fields.Side(Convert.ToChar(order.OrderSide));
            orderCancelRequest.SetField(side);

            QuickFix.Fields.TransactTime transactTime = new QuickFix.Fields.TransactTime(order.OrderDateTime);
            orderCancelRequest.SetField(transactTime);

            QuickFix.Fields.OrdType ordType = new QuickFix.Fields.OrdType('F');
            orderCancelRequest.SetField(ordType);

            return orderCancelRequest;
        }
        #endregion

        #endregion

        /// <summary>
        /// Extracts the rejection details from the incoming message
        /// </summary>
        /// <param name="executionReport"></param>
        /// <returns></returns>
        private Rejection ExtractOrderRejection(ExecutionReport executionReport)
        {
            Rejection rejection = new Rejection(new Security() { Symbol = executionReport.Symbol.getValue() }, _provider,
                executionReport.TransactTime.getValue());

            rejection.OrderId = executionReport.OrderID.getValue();
            rejection.RejectioReason = executionReport.OrdRejReason.getValue().ToString();

            return rejection;
        }

        /// <summary>
        /// Read FIX properties and sets the respective parameters
        /// </summary>
        private void PopulateFixSettings()
        {
            try
            {
                // Get parameter values
                var settings = ReadFixSettingsFile.GetSettings(_fixSettingsFile);

                // Assign parameter values
                if (settings != null && settings.Count > 0)
                {
                    settings.TryGetValue("Username", out _userName);
                    settings.TryGetValue("Password", out _password);
                    settings.TryGetValue("SenderCompID", out _tradeSenderCompId);
                    settings.TryGetValue("TargetCompID", out _tradeTargetCompId);
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "PopulateFixSettings");
            }
        }
    }
}
