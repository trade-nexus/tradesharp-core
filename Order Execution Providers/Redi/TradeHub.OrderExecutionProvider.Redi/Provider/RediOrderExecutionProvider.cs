using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuickFix;
using QuickFix.Fields;
using QuickFix.FIX43;
using QuickFix.Transport;
using TraceSourceLogger;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.DomainModels.OrderDomain;
using TradeHub.Common.Core.OrderExecutionProvider;
using TradeHub.Common.Fix.Infrastructure;
using Constants = TradeHub.Common.Core.Constants;
using FixCommon = TradeHub.Common.Fix;
using Message = QuickFix.Message;

namespace TradeHub.OrderExecutionProvider.Redi.Provider
{
    public class RediOrderExecutionProvider : QuickFix.MessageCracker, IApplication, IMarketOrderProvider, ILimitOrderProvider
    {
        private Type _type = typeof(RediOrderExecutionProvider);

        private readonly string _provider;

        private bool _isConnected = false;

        private string _fixSettingsFile;

        #region Redi OrderClient Members

        private string _password = string.Empty;
        private string _userName = string.Empty;
        private string _account = string.Empty;
        private string _clientId = string.Empty;
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

        // Default Constructor
        public RediOrderExecutionProvider()
        {
            // Set provider name
            _provider = Common.Core.Constants.OrderExecutionProvider.Redi;

            _fixSettingsFile = AppDomain.CurrentDomain.BaseDirectory + @"\Config\" + "RediFIXSettings.txt";
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
                        Logger.Info("Redi Fix Order Client Started.", _type.FullName, "Start");
                    }
                }
                else
                {
                    if (!this._initiator.IsStopped)
                    {
                        if (Logger.IsInfoEnabled)
                        {
                            Logger.Info("Redi Fix Order Client Already Started.", _type.FullName, "Start");
                        }
                    }
                    else
                    {
                        this._initiator.Start();
                        if (Logger.IsInfoEnabled)
                        {
                            Logger.Info("Redi Fix Order Client Started.", _type.FullName, "Start");
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
                        Logger.Info("Redi Fix Order Client Stoped.", _type.FullName, "Stop");
                    }
                }
                else
                {
                    if (Logger.IsInfoEnabled)
                    {
                        Logger.Info("Redi Fix Order Client Already Stoped.", _type.FullName, "Stop");
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
                QuickFix.FIX42.NewOrderSingle order = NewOrderSingle(marketOrder);

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
                QuickFix.FIX42.NewOrderSingle order = NewOrderSingle(limitOrder);

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
                QuickFix.FIX42.OrderCancelRequest orderCancelRequest = OrderCancelRequest(order);

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
            if (message is QuickFix.FIX42.Logout)
            {
                OnMessageFromAdmin((QuickFix.FIX42.Logout)(message), sessionId);
            }
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
                //Note: This Segment is moved to the cracked Logout method
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
            if (message is QuickFix.FIX42.Logout)
            {
                OnMessageToAdmin((QuickFix.FIX42.Logout)(message), sessionId);
            }
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
                QuickFix.Fields.EncryptMethod encryptMethod = new QuickFix.Fields.EncryptMethod(0);
                QuickFix.Fields.HeartBtInt heartBtInt = new QuickFix.Fields.HeartBtInt(30);
                QuickFix.Fields.ResetSeqNumFlag resetSeqNumFlag = new QuickFix.Fields.ResetSeqNumFlag(true);

                message.SetField(encryptMethod);
                message.SetField(heartBtInt);
            }
            catch (Exception exception)
            {
                Logger.Error(exception.ToString(), _type.FullName, "OnMessage");
            }
        }

        #endregion

        #region Logout Handler

        /// <summary>
        /// Extract Information from the Logout Message Recieved from server
        /// </summary>
        /// <param name="message"></param>
        /// <param name="sessionId"></param>
        private void OnMessageFromAdmin(QuickFix.FIX42.Logout message, SessionID sessionId)
        {
            try
            {
                string text = string.Empty;

                try
                {
                    text = message.Text.getValue();
                }
                catch (QuickFix.FieldNotFoundException fnf) { }

                this._isConnected = false;

                if (this._tradeSenderCompId.Equals(sessionId.SenderCompID))
                {
                    this._orderSessionId = null;
                }

                if (LogoutArrived != null)
                {
                    LogoutArrived(_provider);
                }
                
                if (text.Contains("SEQUENCE NUMBER"))
                {
                    // SEQUENCE NUMBER: Seq # 1431 is Lower Than Expected # 2422
                    int firstIndex = text.IndexOf("Expected", System.StringComparison.Ordinal);
                    int secondIndex = text.IndexOf("Seq", System.StringComparison.Ordinal);
                    int requiredStartingIndex = firstIndex + 11;
                    int requiredEndingIndex = text.Length;//secondIndex - 1;

                    string sequenceNumber = text.Substring(requiredStartingIndex,
                                                           (requiredEndingIndex - requiredStartingIndex));

                    Logger.Info("Sequence Number Required: " + sequenceNumber.PadLeft(10, '0'), _type.FullName, "OnMessage");

                    //secondIndex = text.IndexOf("received", System.StringComparison.Ordinal);

                    //Log.Info((secondIndex + 9) + " | " + (text.Length  - (secondIndex + 9)));
                    //sequenceNumber = text.Substring(secondIndex + 9, (text.Length - (secondIndex + 9)));

                    //Log.Info("Sequence Number Required: " + sequenceNumber.PadLeft(10, '0'));
                    #region Change fix sequence Number

                    Session session = Session.LookupSession(sessionId);
                    session.NextSenderMsgSeqNum = Convert.ToInt32(sequenceNumber.PadLeft(10, '0'));

                    #endregion
                }

                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("Logout Method for 'From Admin'", _type.FullName, "OnMessage");
                    Logger.Info("Text: " + text, _type.FullName, "OnMessage");
                    Logger.Info(
                        " FIX OnLogout - SenderCompID : " + sessionId.SenderCompID + " -> TargetCompID : " +
                        sessionId.TargetCompID, _type.FullName, "OnMessage");
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception.ToString(), _type.FullName, "OnMessage");
            }
        }

        /// <summary>
        /// Extract Information from the Logout Message sent to server
        /// </summary>
        /// <param name="message"></param>
        /// <param name="sessionID"></param>
        private void OnMessageToAdmin(QuickFix.FIX42.Logout message, SessionID sessionID)
        {
            try
            {
                string text = string.Empty;

                try
                {
                    text = message.Text.getValue();
                }
                catch (QuickFix.FieldNotFoundException fnf) { }

                this._isConnected = false;

                if (text.Contains("MsgSeqNum too low"))
                {
                    //int firstIndex = text.IndexOf("expecting", System.StringComparison.Ordinal);
                    //int secondIndex = text.IndexOf("but", System.StringComparison.Ordinal);
                    //int requiredStartingIndex = firstIndex + 10;
                    //int requiredEndingIndex = secondIndex - 1;

                    //string sequenceNumber = text.Substring(requiredStartingIndex,
                    //                                       (requiredEndingIndex - requiredStartingIndex));

                    //Log.Info("Sequence Number Required: " + sequenceNumber.PadLeft(10, '0'));

                    int firstIndex = text.IndexOf("received", System.StringComparison.Ordinal);

                    Logger.Info((firstIndex + 9) + " | " + (text.Length - (firstIndex + 9)), _type.FullName, "OnMessage");
                    string sequenceNumber = text.Substring(firstIndex + 9, (text.Length - (firstIndex + 9)));

                    Logger.Info("Sequence Number Required: " + sequenceNumber.PadLeft(10, '0'), _type.FullName, "OnMessage");
                    #region Change fix sequence Number

                    Session session = Session.LookupSession(sessionID);
                    session.NextTargetMsgSeqNum = Convert.ToInt32(sequenceNumber.PadLeft(10, '0'));

                    #endregion
                }

                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("Logout Method for 'To Admin'", _type.FullName, "OnMessage");
                    Logger.Info("Text: " + text, _type.FullName, "OnMessage");
                    Logger.Info(
                        " FIX OnLogout - SenderCompID : " + sessionID.SenderCompID + " -> TargetCompID : " +
                        sessionID.TargetCompID, _type.FullName, "OnMessage");
                }
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
                    case '1':
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
                    case '2':
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
            Fill fill = new Fill(new Security { Symbol = executionReport.Symbol.getValue() },
                                      _provider, executionReport.ClOrdID.getValue())
            {
                ExecutionDateTime = executionReport.TransactTime.getValue(),
                ExecutionType =
                    executionReport.ExecType.getValue() == ExecType.FILL
                        ? Constants.ExecutionType.Fill
                        : Constants.ExecutionType.Partial,
                ExecutionId = executionReport.ExecID.getValue(),
                ExecutionPrice = Convert.ToDecimal(executionReport.AvgPx.getValue(), CultureInfo.InvariantCulture),
                ExecutionSize = Convert.ToInt32(executionReport.LastShares.getValue()),
                ExecutionSide = orderSide,
                AverageExecutionPrice = Convert.ToDecimal(executionReport.AvgPx.getValue(), CultureInfo.InvariantCulture),
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
        /// Creates a FIX4.2 NewOrderSingle message for Redi
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public QuickFix.FIX42.NewOrderSingle NewOrderSingle(Order order)
        {
            var newOrderSingle = new QuickFix.FIX42.NewOrderSingle();

            var clOrdId = new QuickFix.Fields.ClOrdID(order.OrderID);
            newOrderSingle.SetField(clOrdId);

            if (!string.IsNullOrEmpty(_account))
            {
                var account = new QuickFix.Fields.Account(_account);
                newOrderSingle.SetField(account);
            }

            var currency = new QuickFix.Fields.Currency(order.OrderCurrency);
            newOrderSingle.SetField(currency);

            var handlInst = new QuickFix.Fields.HandlInst(HandlInst.AUTOMATED_EXECUTION_ORDER_PRIVATE);
            newOrderSingle.SetField(handlInst);

            //only limit and market orders are supported.
            if (order.GetType() == typeof(LimitOrder))
            {
                var execInst = new QuickFix.Fields.ExecInst("h");
                newOrderSingle.SetField(execInst);

                newOrderSingle.Set(new OrdType(OrdType.LIMIT));
                newOrderSingle.Set(new Price(((LimitOrder)order).LimitPrice));
            }
            else if (order.GetType() == typeof(MarketOrder))
            {
                newOrderSingle.Set(new OrdType(OrdType.MARKET));
            }

            var orderQty = new QuickFix.Fields.OrderQty(order.OrderSize);
            newOrderSingle.SetField(orderQty);

            var exDestination = new QuickFix.Fields.ExDestination(";");
            newOrderSingle.SetField(exDestination);

            var country = new QuickFix.Fields.DeliverToLocationID("1");
            newOrderSingle.SetField(country);

            var side = new QuickFix.Fields.Side(Convert.ToChar(order.OrderSide));
            newOrderSingle.SetField(side);

            var symbol = new QuickFix.Fields.Symbol(order.Security.Symbol);
            newOrderSingle.SetField(symbol);

            var tif = new QuickFix.Fields.TimeInForce(FixCommon.Converter.ConvertTif.GetFixValue(order.OrderTif));
            newOrderSingle.SetField(tif);

            var transactTime = new QuickFix.Fields.TransactTime(order.OrderDateTime);
            newOrderSingle.SetField(transactTime);

            return newOrderSingle;
        }

        #endregion

        #region Creat Order Cancel Request Message

        /// <summary>
        /// Creates a FIX4.2 OrderCancelRequest message for Redi
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public QuickFix.FIX42.OrderCancelRequest OrderCancelRequest(Order order)
        {
            QuickFix.FIX42.OrderCancelRequest orderCancelRequest = new QuickFix.FIX42.OrderCancelRequest();

            QuickFix.Fields.ClOrdID clOrdId = new QuickFix.Fields.ClOrdID(DateTime.Now.ToString(("yyMMddHmsfff")));
            orderCancelRequest.SetField(clOrdId);

            QuickFix.Fields.OrigClOrdID origClOrdId = new QuickFix.Fields.OrigClOrdID(order.OrderID);
            orderCancelRequest.SetField(origClOrdId);

            QuickFix.Fields.Symbol symbol = new QuickFix.Fields.Symbol(order.Security.Symbol);
            orderCancelRequest.SetField(symbol);

            QuickFix.Fields.Side side = new QuickFix.Fields.Side(Convert.ToChar(order.OrderSide));
            orderCancelRequest.SetField(side);

            QuickFix.Fields.TransactTime transactTime = new QuickFix.Fields.TransactTime(order.OrderDateTime);
            orderCancelRequest.SetField(transactTime);

            QuickFix.Fields.Currency currency = new QuickFix.Fields.Currency(order.OrderCurrency);
            orderCancelRequest.SetField(currency);

            return orderCancelRequest;
        }

        #endregion

        #region Create Test Request Message

        /// <summary>
        /// Creates a FIX4.2 TestRequest Message
        /// </summary>
        /// <param name="testId"></param>
        /// <returns></returns>
        public QuickFix.FIX42.TestRequest TestRequest(string testId)
        {
            QuickFix.FIX42.TestRequest testRequest = new QuickFix.FIX42.TestRequest();

            QuickFix.Fields.TestReqID testReqId = new QuickFix.Fields.TestReqID(testId);
            testRequest.SetField(testReqId);

            return testRequest;
        }

        #endregion

        #endregion

        /// <summary>
        /// Extracts the rejection details from the incoming message
        /// </summary>
        /// <param name="executionReport"></param>
        /// <returns></returns>
        private Rejection ExtractOrderRejection(QuickFix.FIX42.ExecutionReport executionReport)
        {
            Rejection rejection = new Rejection(new Security() {Symbol = executionReport.Symbol.getValue()}, _provider,
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
