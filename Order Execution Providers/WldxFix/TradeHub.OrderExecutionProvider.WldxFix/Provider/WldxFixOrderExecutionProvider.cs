using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuickFix;
using QuickFix.Fields;
using QuickFix.FIX42;
using QuickFix.Transport;
using TraceSourceLogger;
using TradeHub.Common.Core.Constants;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.DomainModels.OrderDomain;
using TradeHub.Common.Core.OrderExecutionProvider;
using TradeHub.OrderExecutionProvider.WldxFix.Utility;
using Account = QuickFix.Fields.Account;
using Message = QuickFix.Message;

namespace TradeHub.OrderExecutionProvider.WldxFix.Provider
{
    /// <summary>
    /// Wldx fix order provider
    /// </summary>
    public class WldxFixOrderExecutionProvider : MessageCracker, IApplication,IMarketOrderProvider, ILimitOrderProvider
    {
        private string _provider = "WldxFix";
        private Type _type = typeof (WldxFixOrderExecutionProvider);
        private IInitiator _initiator = null;
        private SessionID _orderSessionID = null;
        private string _fixSettingsFile;
        private string _account;
        private string _onBehalfOfCompId;
        private List<Order> _orders;
        private ParameterReader _parameterReader;

        /// <summary>
        /// Constructor
        /// </summary>
        public WldxFixOrderExecutionProvider()
        {
            _parameterReader = new ParameterReader("WldxFixOrderParams.xml");
            _orders=new List<Order>();
            InitializeParameters();
        }

        /// <summary>
        /// Initialize Parameter
        /// </summary>
        private void InitializeParameters()
        {
            _account = _parameterReader.GetParameterValue("Account");
            _onBehalfOfCompId = _parameterReader.GetParameterValue("OnBehalfOfCompID");
            _fixSettingsFile = AppDomain.CurrentDomain.BaseDirectory + @"\Config\" + "FixSettings.txt";
        }

        #region IMarket & Limit Order Provider Region

        public event Action<Order> NewArrived;
        public event Action<Execution> ExecutionArrived;
        public event Action<Rejection> RejectionArrived;
        public event Action<string> LogonArrived;
        public event Action<string> LogoutArrived;
        public event Action<Rejection> OrderRejectionArrived;
        public event Action<LimitOrder> OnLocateMessage;
        public event Action<Position> OnPositionMessage;
        public event Action<Order> CancellationArrived;

        public bool Start()
        {
            try
            {
                if (this._initiator == null)
                {
                    SessionSettings settings = new SessionSettings(this._fixSettingsFile);
                    IApplication application = this;
                    FileStoreFactory storeFactory = new FileStoreFactory(settings);
                    FileLogFactory logFactory = new FileLogFactory(settings);
                    IMessageStoreFactory messageFactory = new FileStoreFactory(settings);

                    this._initiator = new QuickFix.Transport.SocketInitiator(application, storeFactory, settings, logFactory);
                    this._initiator.Start();

                    if (Logger.IsInfoEnabled)
                    {
                        Logger.Info("Fix Order Client Started.", _type.FullName, "Start");
                    }
                }
                else
                {
                    if (!this._initiator.IsStopped)
                    {
                        if (Logger.IsInfoEnabled)
                        {
                            Logger.Info("Fix Order Client Already Started.", _type.FullName, "Start");
                        }
                    }
                    else
                    {
                        this._initiator.Start();
                        if (Logger.IsInfoEnabled)
                        {
                            Logger.Info("Fix Order Client Started.", _type.FullName, "Start");
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

        public bool Stop()
        {
            try
            {
                if (!this._initiator.IsStopped)
                {
                    this._initiator.Stop();
                    if (Logger.IsInfoEnabled)
                    {
                        Logger.Info("Fix Order Client Stoped.", _type.FullName, "Stop");
                    }
                }
                else
                {
                    if (Logger.IsInfoEnabled)
                    {
                        Logger.Info("Fix Order Client Already Stoped.", _type.FullName, "Stop");
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

        public bool IsConnected()
        {
            return _isConnected;
        }

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
                NewOrderSingle order = CreateFixOrder(marketOrder);
                order.Header.SetField(new OnBehalfOfCompID(_onBehalfOfCompId), true);
                _orders.Add(marketOrder);
                Session.SendToTarget(order, _orderSessionID);
                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug("Market Order Sent, Order=" + marketOrder, _type.FullName, "SendMarketOrder");
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "SendLimitOrder");
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
                NewOrderSingle order = CreateFixOrder(limitOrder);
                order.Header.SetField(new OnBehalfOfCompID(_onBehalfOfCompId), true);
                _orders.Add(limitOrder);
                Session.SendToTarget(order, _orderSessionID);
                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug("Limit Order Sent, Order=" + limitOrder, _type.FullName, "SendLimitOrder");
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
                OrderCancelRequest orderCancelRequest=CreateOrderCancelRequest(order);
                orderCancelRequest.Header.SetField(new OnBehalfOfCompID(_onBehalfOfCompId), true);
                Session.SendToTarget(orderCancelRequest, _orderSessionID);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "CancelLimitOrder");
            }
        }

        #endregion

        #region FIX Region
        private bool _isConnected = false;

        /// <summary>
        /// Create FIX Order
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        private NewOrderSingle CreateFixOrder(Order order)
        {
            QuickFix.FIX42.NewOrderSingle orderSingle=new NewOrderSingle();
            //set id
            orderSingle.Set(new ClOrdID(order.OrderID));
            //set account
            orderSingle.Set(new Account(_account));
            //set handle inst
            orderSingle.Set(new HandlInst(HandlInst.AUTOMATED_EXECUTION_ORDER_PRIVATE));
            //set symbol
            orderSingle.Set(new Symbol(order.Security.Symbol));
            //set locate required
            orderSingle.Set(new LocateReqd(LocateReqd.NO));
            //set time in force
            orderSingle.Set(new TimeInForce(TimeInForce.DAY));
            //set destination
            orderSingle.Set(new ExDestination(order.Exchange));
            //set date time
            orderSingle.Set(new TransactTime(order.OrderDateTime));
            //set order size
            orderSingle.Set(new OrderQty(order.OrderSize));
            //set order side
            if (order.OrderSide == OrderSide.BUY)
            {
                orderSingle.Set(new Side(Side.BUY));
            }
            else if (order.OrderSide == OrderSide.SELL)
            {
                orderSingle.Set(new Side(Side.SELL));
            }
            else if (order.OrderSide == OrderSide.SHORT)
            {
                orderSingle.Set(new Side(Side.SELL));
                orderSingle.Set(new LocateReqd(LocateReqd.YES));
            }

            //only limit and market orders are supported.
            if (order.GetType() == typeof (LimitOrder))
            {
                orderSingle.Set(new OrdType(OrdType.LIMIT));
                orderSingle.Set(new Price(((LimitOrder)order).LimitPrice));
            }
            else if (order.GetType() == typeof(MarketOrder))
            {
                orderSingle.Set(new OrdType(OrdType.MARKET));
            }
            return orderSingle;
        }

        /// <summary>
        /// Order cancel request
        /// </summary>
        /// <param name="order"></param>
        private OrderCancelRequest CreateOrderCancelRequest(Order order)
        {
            OrderCancelRequest cancelRequest=new OrderCancelRequest();
            cancelRequest.Set(new OrigClOrdID(order.OrderID));
            cancelRequest.Set(new ClOrdID(DateTime.Now.ToString(("yyMMddHmsfff"))));
            cancelRequest.Set(new OrderQty(order.OrderSize));
            cancelRequest.Set(new Symbol(order.Security.Symbol));
            cancelRequest.Set(new TransactTime(order.OrderDateTime));
            if (order.OrderSide == OrderSide.BUY)
            {
                cancelRequest.Set(new Side(Side.BUY));
            }
            else if (order.OrderSide == OrderSide.SELL)
            {
                cancelRequest.Set(new Side(Side.SELL));
            }
            return cancelRequest;
        }

        /// <summary>
        /// Add username and password before sending the message
        /// </summary>
        /// <param name="message"></param>
        /// <param name="sessionID"></param>
        private void OnMessage(QuickFix.FIX42.Logon message, SessionID sessionID)
        {
            if (Logger.IsInfoEnabled)
            {
                Logger.Info("On Logon Message", _type.FullName, "OnMessage");
            }
            try
            {
                ResetSeqNumFlag resetSeqNumFlag = new ResetSeqNumFlag(true);
                message.Header.SetField(new OnBehalfOfCompID(_onBehalfOfCompId), true);
                message.EncryptMethod.setValue(EncryptMethod.NONE);
                message.ResetSeqNumFlag = resetSeqNumFlag;
            }
            catch (Exception exception)
            {
                Logger.Error(exception.ToString(), _type.FullName, "OnMessage");
            }
        }

        /// <summary>
        /// Recevies order cancellation message
        /// </summary>
        /// <param name="reject"></param>
        /// <param name="sessionID"></param>
        private void OnMessage(QuickFix.FIX42.OrderCancelReject reject, SessionID sessionID)
        {
            if (Logger.IsInfoEnabled)
            {
                Logger.Info("On Order Cancel Reject, OrderId=" + reject.OrigClOrdID.getValue(), _type.FullName, "OnMessage");
            }
            Order getOrder =
                        (from order in _orders where order.OrderID == reject.OrigClOrdID.getValue() select order)
                            .FirstOrDefault();
            if (getOrder != null)
            {
                Rejection rejection = new Rejection(getOrder.Security,_provider);
                rejection.OrderId = getOrder.OrderID;
                if (OrderRejectionArrived != null)
                {
                    OrderRejectionArrived(rejection);
                }
            }
        }

        /// <summary>
        /// Execution Report
        /// </summary>
        /// <param name="message"></param>
        /// <param name="sessionID"></param>
        private void OnMessage(QuickFix.FIX42.ExecutionReport message, SessionID sessionID)
        {
            try
            {
                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug(
                        "Execution Report, OrderId=" + message.ClOrdID.getValue() + ", Exec Type=" +
                        message.ExecType.getValue(), _type.FullName, "OnMessage");
                }
                Order getOrder =
                    (from order in _orders where order.OrderID == message.ClOrdID.getValue() select order)
                        .FirstOrDefault();
                switch (message.ExecType.getValue())
                {
                    case ExecType.NEW:
                        if (getOrder != null)
                        {
                            if (NewArrived != null)
                            {
                                NewArrived((Order)getOrder.Clone());
                            }
                        }
                        break;
                    case ExecType.REJECTED:
                        if (getOrder != null)
                        {
                            Rejection rejection = new Rejection(getOrder.Security, getOrder.OrderExecutionProvider,
                                message.TransactTime.getValue());
                            rejection.OrderId = getOrder.OrderID;
                            if (OrderRejectionArrived != null)
                            {
                                OrderRejectionArrived(rejection);
                            }
                        }
                        break;
                    case ExecType.CANCELED:
                        if (getOrder != null)
                        {
                            if (CancellationArrived != null)
                            {
                                CancellationArrived((Order)getOrder.Clone());
                            }
                            _orders.Remove(getOrder);
                        }
                        break;
                    case ExecType.PARTIAL_FILL:
                        if (getOrder != null)
                        {
                            Fill fill = new Fill(new Security() {Symbol = message.Symbol.getValue()},
                                getOrder.OrderExecutionProvider, getOrder.OrderID);
                            fill.ExecutionId = message.ExecID.getValue();
                            fill.ExecutionType = ExecutionType.Partial;
                            fill.LeavesQuantity = Convert.ToInt32(message.LeavesQty.getValue());
                            fill.CummalativeQuantity = Convert.ToInt32(message.CumQty.getValue());
                            fill.ExecutionSize = Convert.ToInt32(message.LastShares.getValue());
                            fill.AverageExecutionPrice = Convert.ToInt32(message.AvgPx.getValue());
                            fill.ExecutionPrice = message.LastPx.getValue();
                            fill.ExecutionDateTime = message.TransactTime.getValue();
                            fill.ExecutionSide = getOrder.OrderSide;
                            Order orderClone = (Order) getOrder.Clone();
                            orderClone.OrderStatus = OrderStatus.PARTIALLY_EXECUTED;
                            Execution execution = new Execution(fill, orderClone);
                            execution.OrderExecutionProvider = _provider;
                            if (ExecutionArrived != null)
                            {
                                ExecutionArrived(execution);
                            }
                        }
                        break;
                    case ExecType.FILL:
                        if (getOrder != null)
                        {
                            Fill fill = new Fill(new Security() {Symbol = message.Symbol.getValue()},
                                getOrder.OrderExecutionProvider, getOrder.OrderID);
                            fill.ExecutionId = message.ExecID.getValue();
                            fill.ExecutionType = ExecutionType.Fill;
                            fill.LeavesQuantity = Convert.ToInt32(message.LeavesQty.getValue());
                            fill.CummalativeQuantity = Convert.ToInt32(message.CumQty.getValue());
                            fill.ExecutionSize = Convert.ToInt32(message.LastShares.getValue());
                            fill.AverageExecutionPrice = Convert.ToInt32(message.AvgPx.getValue());
                            fill.ExecutionPrice = message.LastPx.getValue();
                            fill.ExecutionDateTime = message.TransactTime.getValue();
                            fill.ExecutionSide = getOrder.OrderSide;
                            Order orderClone = (Order)getOrder.Clone();
                            orderClone.OrderStatus = OrderStatus.EXECUTED;
                            Execution execution = new Execution(fill, orderClone);
                            execution.OrderExecutionProvider = _provider;
                            if (ExecutionArrived != null)
                            {
                                ExecutionArrived(execution);
                            }
                            _orders.Remove(getOrder);
                        }
                        break;
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception,_type.FullName,"OnMessage");
            }
        }

        /// <summary>
        /// Reject
        /// </summary>
        /// <param name="message"></param>
        /// <param name="sessionID"></param>
        private void OnMessage(QuickFix.FIX42.Reject message, SessionID sessionID)
        {
            if (Logger.IsInfoEnabled)
            {
                Logger.Info("Message Rejected, Sequence Number=" + message.RefSeqNum.getValue(), _type.FullName, "OnMessage");
            }
        }

        /// <summary>
        /// FIX42 Message cracker
        /// </summary>
        /// <param name="message"></param>
        /// <param name="sessionID"></param>
        public void Crack(Message message, SessionID sessionID)
        {
            if (message is QuickFix.FIX42.Reject)
            {
                OnMessage((QuickFix.FIX42.Reject)(message), sessionID);
            }
            else if (message is QuickFix.FIX42.ExecutionReport)
            {
                OnMessage((QuickFix.FIX42.ExecutionReport)(message), sessionID);
            }
            else if (message is QuickFix.FIX42.OrderCancelReject)
            {
                OnMessage((QuickFix.FIX42.OrderCancelReject)(message), sessionID);
            }
            else if (message is QuickFix.FIX42.Logon)
            {
                OnMessage((QuickFix.FIX42.Logon)(message), sessionID);
            }
        }

        public void ToAdmin(Message message, SessionID sessionID)
        {
            Crack(message, sessionID);
        }

        public void FromAdmin(Message message, SessionID sessionID)
        {
            Crack(message, sessionID);
        }

        public void ToApp(Message message, SessionID sessionID)
        {
            Crack(message, sessionID);
        }

        public void FromApp(Message message, SessionID sessionID)
        {
            Crack(message, sessionID);
        }

        public void OnCreate(SessionID sessionID)
        {

        }

        public void OnLogout(SessionID sessionID)
        {
            try
            {
                this._isConnected = false;
                if (LogoutArrived != null)
                {
                    LogoutArrived(_provider);
                }
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info(
                        "FIX Order Client OnLogout - SenderCompID : " + sessionID.SenderCompID + " -> TargetCompID : " +
                        sessionID.TargetCompID, _type.FullName, "OnLogout");
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception.ToString(), _type.FullName, "OnLogout");
            }
        }

        public void OnLogon(SessionID sessionID)
        {
            try
            {
                _isConnected = true;
                _orderSessionID = sessionID;
                
                if (LogonArrived != null)
                {
                    LogonArrived(_provider);
                }
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info(
                        " FIX Order Client OnLogon - SenderCompID : " + sessionID.SenderCompID + " -> TargetCompID : " +
                        sessionID.TargetCompID, _type.FullName, "OnLogon");
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception.ToString(), _type.FullName, "OnLogon");
            }
        }
        #endregion
    }
}
