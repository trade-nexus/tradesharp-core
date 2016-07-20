using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EasyNetQ;
using EasyNetQ.Topology;
using NUnit.Framework;
using Spring.Context.Support;
using TradeHub.Common.Core.Constants;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.ValueObjects.AdminMessages;
using TradeHub.Common.Core.ValueObjects.Heartbeat;
using TradeHub.Common.Core.ValueObjects.Inquiry;
using TradeHub.Common.Core.ValueObjects.MarketData;
using TradeHub.MarketDataEngine.Server.Service;
using Constants = TradeHub.Common.Core.Constants;

namespace TradeHub.MarketDataEngine.Server.Tests.Integration
{
    [TestFixture]
    public class ApplicationControllerTestCases
    {
        private ApplicationController _applicationController;
        private IAdvancedBus _advancedBus;
        private IQueue _strategyAdminQueue;
        private IQueue _tickQueue;
        private IQueue _inquiryQueue;
        private IQueue _liveBarQueue;
        private IQueue _historicQueue;
        private IExchange _adminExchange;
        private IQueue _logonQueue;
        private Dictionary<string, string> appInfo = new Dictionary<string, string>()
            {
                {"Admin","admin.strategy.key"},{"Tick","tick.strategy.key"},{"LiveBar","bar.strategy.key"},{"HistoricBar","historic.strategy.key"}
            };
            
        [SetUp]
        public void SetUp()
        {
            _applicationController = ContextRegistry.GetContext()["ApplicationController"] as ApplicationController;
            if (_applicationController != null) _applicationController.StartServer();

            // Initialize Advance Bus
            _advancedBus = RabbitHutch.CreateBus("host=localhost").Advanced;
            
            // Create a admin exchange
            _adminExchange = _advancedBus.ExchangeDeclare("marketdata_exchange", ExchangeType.Direct, true, false, true);

            // Create strategy admin Queue
            _strategyAdminQueue = _advancedBus.QueueDeclare("admin_queue", false, false, true, true);

            // Create strategy Tick Queue
            _tickQueue = _advancedBus.QueueDeclare("tick_queue",  false, false, true, true);

            // Create strategy Live Bar Queue
            _liveBarQueue = _advancedBus.QueueDeclare("bar_queue",  false, false, true, true);

            // Create strategy Tick Queue
            _historicQueue = _advancedBus.QueueDeclare("historic_queue",  false, false, true, true);

            // Create admin Queue
            _logonQueue = _advancedBus.QueueDeclare("marketdata_engine_logon_queue",  false, false, true, true);

            // Create inquiry Queue
            _inquiryQueue = _advancedBus.QueueDeclare("inquiry_queue", false, false, true, true);

            // Bind Strategy Admin Queue to already initialized Exchange with the specified Routing Key
            _advancedBus.Bind(_adminExchange, _strategyAdminQueue, "admin.strategy.key");

            // Bind Strategy Tick Queue to already initialized Exchange with the specified Routing Key
            _advancedBus.Bind(_adminExchange, _tickQueue, "tick.strategy.key");

            // Bind Strategy Live Bar Queue to already initialized Exchange with the specified Routing Key
            _advancedBus.Bind(_adminExchange, _liveBarQueue, "bar.strategy.key");

            // Bind Strategy Historic Queue to already initialized Exchange with the specified Routing Key
            _advancedBus.Bind(_adminExchange, _historicQueue, "historic.strategy.key");

            // Bind Admin Queue to already initialized Exchange with the specified Routing Key
            _advancedBus.Bind(_adminExchange, _inquiryQueue, "inquiry.strategy.key");

            // Bind Admin Queue to already initialized Exchange with the specified Routing Key
            _advancedBus.Bind(_adminExchange, _logonQueue, "marketdata.engine.login");

            var appInfoMessage = new Message<Dictionary<string, string>>(appInfo);
            appInfoMessage.Properties.AppId = "test_app_id";
            string routingKey = "marketdata.engine.appinfo";

            //using (var channel = _advancedBus.OpenPublishChannel())
            {
                _advancedBus.Publish(_adminExchange, routingKey, true, false, appInfoMessage);
            }
        }

        [TearDown]
        public void TearDown()
        {
            _applicationController.StopServer();
        }

        [Test]
        [Category("Integration")]
        public void RequestNewStrategyIdTestCase()
        {
            InquiryMessage inquiryMessage= new InquiryMessage(){ Type = Constants.InquiryTags.AppID};
            Message<InquiryMessage> message = new Message<InquiryMessage>(inquiryMessage);

            var manualInquiryEvent = new ManualResetEvent(false);

            bool inquiryArrived = false;

            message.Properties.AppId = "test_app_id";
            message.Properties.ReplyTo = "inquiry.strategy.key";

            _advancedBus.Consume<InquiryResponse>(
                _inquiryQueue, (msg, messageReceivedInfo) =>
                          Task.Factory.StartNew(() =>
                          {
                              inquiryArrived = true;
                              manualInquiryEvent.Set();
                          }));

            //using (var channel = _advancedBus.OpenPublishChannel())
            {
                _advancedBus.Publish(_adminExchange, "marketdata.engine.inquiry", true, false, message);
            }
            manualInquiryEvent.WaitOne(9000, false);

            Assert.AreEqual(true, inquiryArrived);
        }

        [Test]
        [Category("Integration")]
        public void ConnectNewStrategyTestCase()
        {
            Login login = new Login {MarketDataProvider = Constants.MarketDataProvider.Simulated};
            Message<Login> message = new Message<Login>(login);

            var manualLogonEvent = new ManualResetEvent(false);

            bool logonArrived = false;

            message.Properties.AppId = "test_app_id";
            message.Properties.ReplyTo = "admin.strategy.key";

            _advancedBus.Consume<string>(
                _strategyAdminQueue, (msg, messageReceivedInfo) =>
                          Task.Factory.StartNew(() =>
                              {
                                  logonArrived = true;
                                  manualLogonEvent.Set();
                              }));

            //using (var channel = _advancedBus.OpenPublishChannel())
            {
                _advancedBus.Publish(_adminExchange, "marketdata.engine.login", true, false, message);
            }
            manualLogonEvent.WaitOne(30000, false);

            Assert.AreEqual(true, logonArrived);
        }

        [Test]
        [Category("Integration")]
        public void DisconnectStrategyTestCase()
        {
            Login login = new Login { MarketDataProvider = Constants.MarketDataProvider.Blackwood };
            Message<Login> loginMessage = new Message<Login>(login);

            Logout logout = new Logout { MarketDataProvider = Constants.MarketDataProvider.Blackwood};
            Message<Logout> logoutMessage = new Message<Logout>(logout);

            var manualLogonEvent = new ManualResetEvent(false);
            var manualLogoutEvent = new ManualResetEvent(false);

            bool logonArrived = false;
            bool logoutArrived = false;

            loginMessage.Properties.AppId = "test_app_id";
            loginMessage.Properties.ReplyTo = "admin.strategy.key";

            logoutMessage.Properties.AppId = "test_app_id";
            logoutMessage.Properties.ReplyTo = "admin.strategy.key";

            //using (var channel = _advancedBus.OpenPublishChannel())
            {

                _advancedBus.Consume<string>(
                    _strategyAdminQueue, (msg, messageReceivedInfo) =>
                              Task.Factory.StartNew(() =>
                                  {
                                      if (msg.Body.Contains("Logon"))
                                      {
                                          logonArrived = true;

                                          _advancedBus.Publish(_adminExchange, "marketdata.engine.logout", true, false, logoutMessage);

                                          manualLogonEvent.Set();
                                      }
                                      else if (msg.Body.Contains("Logout"))
                                      {
                                          logoutArrived = true;
                                          manualLogoutEvent.Set();
                                      }
                                  }));


                _advancedBus.Publish(_adminExchange, "marketdata.engine.login", true, false, loginMessage);

                manualLogonEvent.WaitOne(30000, false);
                manualLogoutEvent.WaitOne(30000, false);
            }
            Assert.AreEqual(true, logonArrived, "Logon Status");
            Assert.AreEqual(true, logoutArrived, "Logout Status");
        }

        [Test]
        [Category("Console")]
        public void ConnectSimulatorTestCase()
        {
            Login login = new Login { MarketDataProvider = Constants.MarketDataProvider.Simulated };
            Message<Login> message = new Message<Login>(login);

            var manualLogonEvent = new ManualResetEvent(false);

            bool logonArrived = false;

            message.Properties.AppId = "test_app_id";
            message.Properties.ReplyTo = "admin.strategy.key";

            _advancedBus.Consume<string>(
                _strategyAdminQueue, (msg, messageReceivedInfo) =>
                          Task.Factory.StartNew(() =>
                          {
                              if (msg.Body.Contains("Logon"))
                              {
                                  logonArrived = true;
                                  manualLogonEvent.Set();
                              }
                          }));

            //using (var channel = _advancedBus.OpenPublishChannel())
            {
                _advancedBus.Publish(_adminExchange, "marketdata.engine.login", true, false, message);
            }
            manualLogonEvent.WaitOne(30000, false);

            Assert.AreEqual(true, logonArrived);
        }

        [Test]
        [Category("Console")]
        public void DisconnectSimulatorTestCase()
        {
            Login login = new Login { MarketDataProvider = Constants.MarketDataProvider.Simulated };
            Message<Login> loginMessage = new Message<Login>(login);

            Logout logout = new Logout { MarketDataProvider = Constants.MarketDataProvider.Simulated };
            Message<Logout> logoutMessage = new Message<Logout>(logout);

            var manualLogonEvent = new ManualResetEvent(false);
            var manualLogoutEvent = new ManualResetEvent(false);

            bool logonArrived = false;
            bool logoutArrived = false;

            loginMessage.Properties.AppId = "test_app_id";
            loginMessage.Properties.ReplyTo = "admin.strategy.key";

            logoutMessage.Properties.AppId = "test_app_id";
            logoutMessage.Properties.ReplyTo = "admin.strategy.key";

            //using (var channel = _advancedBus.OpenPublishChannel())
            {

                _advancedBus.Consume<string>(
                    _strategyAdminQueue, (msg, messageReceivedInfo) =>
                              Task.Factory.StartNew(() =>
                              {
                                  if (msg.Body.Contains("Logon"))
                                  {
                                      logonArrived = true;

                                      _advancedBus.Publish(_adminExchange, "marketdata.engine.logout", true, false, logoutMessage);

                                      manualLogonEvent.Set();
                                  }
                                  else if (msg.Body.Contains("Logout"))
                                  {
                                      logoutArrived = true;
                                      manualLogoutEvent.Set();
                                  }
                              }));


                _advancedBus.Publish(_adminExchange, "marketdata.engine.login", true, false, loginMessage);

                manualLogonEvent.WaitOne(30000, false);
                manualLogoutEvent.WaitOne(30000, false);
            }
            Assert.AreEqual(true, logonArrived, "Logon Status");
            Assert.AreEqual(true, logoutArrived, "Logout Status");
        }

        [Test]
        [Category("Integration")]
        public void ConnectIBTestCase()
        {
            Login login = new Login { MarketDataProvider = Constants.MarketDataProvider.InteractiveBrokers };
            Message<Login> message = new Message<Login>(login);

            var manualLogonEvent = new ManualResetEvent(false);

            bool logonArrived = false;

            message.Properties.AppId = "test_app_id";
            message.Properties.ReplyTo = "admin.strategy.key";

            _advancedBus.Consume<string>(
                _strategyAdminQueue, (msg, messageReceivedInfo) =>
                          Task.Factory.StartNew(() =>
                          {
                              if (msg.Body.Contains("Logon"))
                              {
                                  logonArrived = true;
                                  manualLogonEvent.Set();
                              }
                          }));

            //using (var channel = _advancedBus.OpenPublishChannel())
            {
                _advancedBus.Publish(_adminExchange, "marketdata.engine.login", true, false, message);
            }
            manualLogonEvent.WaitOne(30000, false);

            Assert.AreEqual(true, logonArrived);
        }

        [Test]
        [Category("Integration")]
        public void DisconnectIBTestCase()
        {
            Login login = new Login { MarketDataProvider = Constants.MarketDataProvider.InteractiveBrokers };
            Message<Login> loginMessage = new Message<Login>(login);

            Logout logout = new Logout { MarketDataProvider = Constants.MarketDataProvider.InteractiveBrokers };
            Message<Logout> logoutMessage = new Message<Logout>(logout);

            var manualLogonEvent = new ManualResetEvent(false);
            var manualLogoutEvent = new ManualResetEvent(false);

            bool logonArrived = false;
            bool logoutArrived = false;

            loginMessage.Properties.AppId = "test_app_id";
            loginMessage.Properties.ReplyTo = "admin.strategy.key";

            logoutMessage.Properties.AppId = "test_app_id";
            logoutMessage.Properties.ReplyTo = "admin.strategy.key";

            //using (var channel = _advancedBus.OpenPublishChannel())
            {

                _advancedBus.Consume<string>(
                    _strategyAdminQueue, (msg, messageReceivedInfo) =>
                              Task.Factory.StartNew(() =>
                              {
                                  if (msg.Body.Contains("Logon"))
                                  {
                                      logonArrived = true;

                                      _advancedBus.Publish(_adminExchange, "marketdata.engine.logout", true, false, logoutMessage);

                                      manualLogonEvent.Set();
                                  }
                                  else if (msg.Body.Contains("Logout"))
                                  {
                                      logoutArrived = true;
                                      manualLogoutEvent.Set();
                                  }
                              }));


                _advancedBus.Publish(_adminExchange, "marketdata.engine.login", true, false, loginMessage);

                manualLogonEvent.WaitOne(30000, false);
                manualLogoutEvent.WaitOne(30000, false);
            }
            Assert.AreEqual(true, logonArrived, "Logon Status");
            Assert.AreEqual(true, logoutArrived, "Logout Status");
        }

        [Test]
        [Category("Integration")]
        public void SubscribeStrategyTestCase()
        {
            Login login = new Login { MarketDataProvider = Constants.MarketDataProvider.Blackwood };
            Message<Login> loginMessage = new Message<Login>(login);

            Logout logout = new Logout { MarketDataProvider = Constants.MarketDataProvider.Blackwood };
            Message<Logout> logoutMessage = new Message<Logout>(logout);

            Subscribe subscribe = new Subscribe() { Security = new Security() { Symbol = "AGQ" }, MarketDataProvider = Constants.MarketDataProvider.Blackwood };
            Message<Subscribe> subscribeMessage = new Message<Subscribe>(subscribe);

            var manualLogonEvent = new ManualResetEvent(false);
            var manualLogoutEvent = new ManualResetEvent(false);
            var manualTickEvent = new ManualResetEvent(false);

            bool logonArrived = false;
            bool logoutArrived = false;
            bool tickArrived = false;

            loginMessage.Properties.AppId = "test_app_id";
            loginMessage.Properties.ReplyTo = "admin.strategy.key";

            logoutMessage.Properties.AppId = "test_app_id";
            logoutMessage.Properties.ReplyTo = "admin.strategy.key";

            subscribeMessage.Properties.AppId = "test_app_id";
            subscribeMessage.Properties.ReplyTo = "tick.strategy.key";

            //using (var channel = _advancedBus.OpenPublishChannel())
            {

                _advancedBus.Consume<string>(
                    _strategyAdminQueue, (msg, messageReceivedInfo) =>
                              Task.Factory.StartNew(() =>
                              {
                                  if (msg.Body.Contains("Logon"))
                                  {
                                      logonArrived = true;

                                      _advancedBus.Publish(_adminExchange, "marketdata.engine.subscribe", true, false, subscribeMessage);

                                      manualLogonEvent.Set();
                                  }
                                  else if (msg.Body.Contains("Logout"))
                                  {
                                      logoutArrived = true;
                                      manualLogoutEvent.Set();
                                  }
                              }));

                _advancedBus.Consume<Tick>(
                    _tickQueue, (msg, messageReceivedInfo) =>
                                 Task.Factory.StartNew(() =>
                                     {
                                         tickArrived = true;

                                         _advancedBus.Publish(_adminExchange, "marketdata.engine.logout", true, false, logoutMessage);

                                         manualTickEvent.Set();
                                     }));

                _advancedBus.Publish(_adminExchange, "marketdata.engine.login", true, false, loginMessage);

                manualLogonEvent.WaitOne(30000, false);
                manualLogoutEvent.WaitOne(30000, false);
                manualTickEvent.WaitOne(30000, false);
            }
            Assert.AreEqual(true, logonArrived, "Logon Status");
            Assert.AreEqual(true, logoutArrived, "Logout Status");
            Assert.AreEqual(true, tickArrived, "Tick Status");
        }

        [Test]
        [Category("Console")]
        public void SubscribeSimulatorTestCase()
        {
            Login login = new Login { MarketDataProvider = Constants.MarketDataProvider.Simulated };
            Message<Login> loginMessage = new Message<Login>(login);

            Logout logout = new Logout { MarketDataProvider = Constants.MarketDataProvider.Simulated };
            Message<Logout> logoutMessage = new Message<Logout>(logout);

            Subscribe subscribe = new Subscribe()
                {
                    Security = new Security() {Symbol = "IBM"},
                    MarketDataProvider = Constants.MarketDataProvider.Simulated
                };
            Message<Subscribe> subscribeMessage = new Message<Subscribe>(subscribe);

            var manualLogonEvent = new ManualResetEvent(false);
            var manualLogoutEvent = new ManualResetEvent(false);
            var manualTickEvent = new ManualResetEvent(false);

            bool logonArrived = false;
            bool logoutArrived = false;
            bool tickArrived = false;

            loginMessage.Properties.AppId = "test_app_id";
            loginMessage.Properties.ReplyTo = "admin.strategy.key";

            logoutMessage.Properties.AppId = "test_app_id";
            logoutMessage.Properties.ReplyTo = "admin.strategy.key";

            subscribeMessage.Properties.AppId = "test_app_id";
            subscribeMessage.Properties.ReplyTo = "tick.strategy.key";

            //using (var channel = _advancedBus.OpenPublishChannel())
            {

                _advancedBus.Consume<string>(
                    _strategyAdminQueue, (msg, messageReceivedInfo) =>
                              Task.Factory.StartNew(() =>
                              {
                                  if (msg.Body.Contains("Logon"))
                                  {
                                      logonArrived = true;

                                      _advancedBus.Publish(_adminExchange, "marketdata.engine.subscribe", true, false, subscribeMessage);

                                      manualLogonEvent.Set();
                                  }
                                  else if (msg.Body.Contains("Logout"))
                                  {
                                      logoutArrived = true;
                                      manualLogoutEvent.Set();
                                  }
                              }));

                _advancedBus.Consume<Tick>(
                    _tickQueue, (msg, messageReceivedInfo) =>
                                 Task.Factory.StartNew(() =>
                                 {
                                     tickArrived = true;

                                     _advancedBus.Publish(_adminExchange, "marketdata.engine.logout", true, false, logoutMessage);

                                     manualTickEvent.Set();
                                 }));

                _advancedBus.Publish(_adminExchange, "marketdata.engine.login", true, false, loginMessage);

                manualLogonEvent.WaitOne(30000, false);
                manualLogoutEvent.WaitOne(30000, false);
                manualTickEvent.WaitOne(30000, false);
            }
            Assert.AreEqual(true, logonArrived, "Logon Status");
            Assert.AreEqual(true, logoutArrived, "Logout Status");
            Assert.AreEqual(true, tickArrived, "Tick Status");
        }

        [Test]
        [Category("Console")]
        public void SubscribeSimulatorBarTestCase()
        {
            Login login = new Login { MarketDataProvider = Constants.MarketDataProvider.Simulated };
            Message<Login> loginMessage = new Message<Login>(login);

            Logout logout = new Logout { MarketDataProvider = Constants.MarketDataProvider.Simulated };
            Message<Logout> logoutMessage = new Message<Logout>(logout);

            BarDataRequest barDataRequest = new BarDataRequest()
            {
                Security = new Security() { Symbol = "IBM" },
                Id = "123456",
                MarketDataProvider = Constants.MarketDataProvider.Simulated,
                BarFormat = Constants.BarFormat.TIME,
                BarLength = 2,
                PipSize = 1.2M,
                BarPriceType = Constants.BarPriceType.ASK
            };

            Message<BarDataRequest> subscribeMessage = new Message<BarDataRequest>(barDataRequest);

            var manualLogonEvent = new ManualResetEvent(false);
            var manualLogoutEvent = new ManualResetEvent(false);
            var manualBarEvent = new ManualResetEvent(false);

            bool logonArrived = false;
            bool logoutArrived = false;
            bool barArrived = false;

            loginMessage.Properties.AppId = "test_app_id";
            loginMessage.Properties.ReplyTo = "admin.strategy.key";

            logoutMessage.Properties.AppId = "test_app_id";
            logoutMessage.Properties.ReplyTo = "admin.strategy.key";

            subscribeMessage.Properties.AppId = "test_app_id";
            subscribeMessage.Properties.ReplyTo = "bar.strategy.key";

            //using (var channel = _advancedBus.OpenPublishChannel())
            {

                _advancedBus.Consume<string>(
                    _strategyAdminQueue, (msg, messageReceivedInfo) =>
                              Task.Factory.StartNew(() =>
                              {
                                  if (msg.Body.Contains("Logon"))
                                  {
                                      logonArrived = true;

                                      _advancedBus.Publish(_adminExchange, "marketdata.engine.livebar.subscribe", true, false, subscribeMessage);

                                      manualLogonEvent.Set();
                                  }
                                  else if (msg.Body.Contains("Logout"))
                                  {
                                      logoutArrived = true;
                                      manualLogoutEvent.Set();
                                  }
                              }));

                _advancedBus.Consume<Bar>(
                    _liveBarQueue, (msg, messageReceivedInfo) =>
                                 Task.Factory.StartNew(() =>
                                 {
                                     barArrived = true;

                                     _advancedBus.Publish(_adminExchange, "marketdata.engine.logout", true, false, logoutMessage);

                                     manualBarEvent.Set();
                                 }));

                _advancedBus.Publish(_adminExchange, "marketdata.engine.login", true, false, loginMessage);

                manualLogonEvent.WaitOne(30000, false);
                manualLogoutEvent.WaitOne(30000, false);
                manualBarEvent.WaitOne(30000, false);
            }
            Assert.AreEqual(true, logonArrived, "Logon Status");
            Assert.AreEqual(true, logoutArrived, "Logout Status");
            Assert.AreEqual(true, barArrived, "Bar Status");
        }

        [Test]
        [Category("Integration")]
        public void HistoricBarRequestStrategyTestCase()
        {
            Login login = new Login { MarketDataProvider = Constants.MarketDataProvider.Blackwood };
            Message<Login> loginMessage = new Message<Login>(login);

            Logout logout = new Logout { MarketDataProvider = Constants.MarketDataProvider.Blackwood};
            Message<Logout> logoutMessage = new Message<Logout>(logout);

            HistoricDataRequest historicDataRequest = new HistoricDataRequest()
            {
                BarType = BarType.INTRADAY,
                StartTime = new DateTime(635066964000000000, DateTimeKind.Local),
                EndTime = new DateTime(635068112400000000, DateTimeKind.Local),
                Interval = 1,
                MarketDataProvider = Constants.MarketDataProvider.Blackwood,
                Security = new Security() { Symbol = "XOP" },
                Id = "5000",
            };

            Message<HistoricDataRequest> historicDataMessage = new Message<HistoricDataRequest>(historicDataRequest);

            var manualLogonEvent = new ManualResetEvent(false);
            var manualLogoutEvent = new ManualResetEvent(false);
            var manualBarEvent = new ManualResetEvent(false);

            bool logonArrived = false;
            bool logoutArrived = false;
            bool barArrived = false;

            loginMessage.Properties.AppId = "test_app_id";
            loginMessage.Properties.ReplyTo = "admin.strategy.key";

            logoutMessage.Properties.AppId = "test_app_id";
            logoutMessage.Properties.ReplyTo = "admin.strategy.key";

            historicDataMessage.Properties.AppId = "test_app_id";
            historicDataMessage.Properties.ReplyTo = "historic.strategy.key";

            //using (var channel = _advancedBus.OpenPublishChannel())
            {

                _advancedBus.Consume<string>(
                    _strategyAdminQueue, (msg, messageReceivedInfo) =>
                              Task.Factory.StartNew(() =>
                              {
                                  if (msg.Body.Contains("Logon"))
                                  {
                                      logonArrived = true;

                                      _advancedBus.Publish(_adminExchange, "marketdata.engine.historicbar", true, false, historicDataMessage);

                                      manualLogonEvent.Set();
                                  }
                                  else if (msg.Body.Contains("Logout"))
                                  {
                                      logoutArrived = true;
                                      manualLogoutEvent.Set();
                                  }
                              }));

                _advancedBus.Consume<HistoricBarData>(
                    _historicQueue, (msg, messageReceivedInfo) =>
                                 Task.Factory.StartNew(() =>
                                 {
                                     barArrived = true;

                                     _advancedBus.Publish(_adminExchange, "marketdata.engine.logout", true, false, logoutMessage);

                                     manualBarEvent.Set();
                                 }));

                _advancedBus.Publish(_adminExchange, "marketdata.engine.login", true, false, loginMessage);

                manualLogonEvent.WaitOne(30000, false);
                manualLogoutEvent.WaitOne(30000, false);
                manualBarEvent.WaitOne(30000, false);
            }
            Assert.AreEqual(true, logonArrived, "Logon Status");
            Assert.AreEqual(true, logoutArrived, "Logout Status");
            Assert.AreEqual(true, barArrived, "Bar Status");
        }

        [Test]
        [Category("Console")]
        public void HeartbeatTestCase()
        {
            Login login = new Login { MarketDataProvider = Constants.MarketDataProvider.Simulated };
            Message<Login> loginMessage = new Message<Login>(login);

            Logout logout = new Logout { MarketDataProvider = Constants.MarketDataProvider.Simulated };
            Message<Logout> logoutMessage = new Message<Logout>(logout);

            Subscribe subscribe = new Subscribe
            {
                Security = new Security() { Symbol = "IBM" },
                MarketDataProvider = Constants.MarketDataProvider.Simulated
            };

            HeartbeatMessage heartbeat = new HeartbeatMessage {ApplicationId = "test_app_id", HeartbeatInterval = 6000};

            Message<Subscribe> subscribeMessage = new Message<Subscribe>(subscribe);

            IMessage<HeartbeatMessage> heartbeatMessge = new Message<HeartbeatMessage>(heartbeat);

            var manualLogonEvent = new ManualResetEvent(false);
            var manualLogoutEvent = new ManualResetEvent(false);
            var manualTickEvent = new ManualResetEvent(false);

            bool logonArrived = false;
            bool logoutArrived = false;
            bool tickArrived = false;

            loginMessage.Properties.AppId = "test_app_id";
            loginMessage.Properties.ReplyTo = "admin.strategy.key";

            logoutMessage.Properties.AppId = "test_app_id";
            logoutMessage.Properties.ReplyTo = "admin.strategy.key";

            subscribeMessage.Properties.AppId = "test_app_id";
            subscribeMessage.Properties.ReplyTo = "tick.strategy.key";

            heartbeatMessge.Properties.AppId = "test_app_id";

            //using (var channel = _advancedBus.OpenPublishChannel())
            {

                _advancedBus.Consume<string>(
                    _strategyAdminQueue, (msg, messageReceivedInfo) =>
                              Task.Factory.StartNew(() =>
                              {
                                  if (msg.Body.Contains("Logon"))
                                  {
                                      logonArrived = true;

                                      _advancedBus.Publish(_adminExchange, "marketdata.engine.subscribe", true, false, subscribeMessage);

                                      manualLogonEvent.Set();
                                  }
                                  else if (msg.Body.Contains("Logout"))
                                  {
                                      logoutArrived = true;
                                      manualLogoutEvent.Set();
                                  }
                              }));

                _advancedBus.Consume<Tick>(
                    _tickQueue, (msg, messageReceivedInfo) =>
                                 Task.Factory.StartNew(() =>
                                 {
                                     tickArrived = true;

                                     _advancedBus.Publish(_adminExchange, "marketdata.engine.logout", true, false, logoutMessage);

                                     manualTickEvent.Set();
                                 }));

                _advancedBus.Publish(_adminExchange, "marketdata.engine.login", true, false, loginMessage);
                _advancedBus.Publish(_adminExchange, "marketdata.engine.heartbeat", true, false, heartbeatMessge);


                Thread.Sleep(5000);
                _advancedBus.Publish(_adminExchange, "marketdata.engine.heartbeat", true, false, heartbeatMessge);

                Thread.Sleep(5000);
                _advancedBus.Publish(_adminExchange, "marketdata.engine.heartbeat", true, false, heartbeatMessge);

                Thread.Sleep(5000);
                _advancedBus.Publish(_adminExchange, "marketdata.engine.heartbeat", true, false, heartbeatMessge);

                Thread.Sleep(5000);
                _advancedBus.Publish(_adminExchange, "marketdata.engine.heartbeat", true, false, heartbeatMessge);

                manualLogonEvent.WaitOne(30000, false);
                manualLogoutEvent.WaitOne(15000, false);
                manualTickEvent.WaitOne(3000, false);

                Thread.Sleep(100000);
            }


            Assert.AreEqual(true, logonArrived, "Logon Status");
            Assert.AreEqual(false, logoutArrived, "Logout Status");
            Assert.AreEqual(false, tickArrived, "Tick Status");
        }

        // Requires User interaction (Type "exit" in the Console window)
        [Test]
        [Category("Console")]
        public void ConnectSimulatorTestCase_AutoLogout()
        {
            Login login = new Login { MarketDataProvider = Constants.MarketDataProvider.Simulated };
            Message<Login> loginMessage = new Message<Login>(login);

            Logout logout = new Logout { MarketDataProvider = Constants.MarketDataProvider.Simulated };
            Message<Logout> logoutMessage = new Message<Logout>(logout);

            var manualLogonEvent = new ManualResetEvent(false);
            var manualLogoutEvent = new ManualResetEvent(false);

            bool logonArrived = false;
            bool logoutArrived = false;

            loginMessage.Properties.AppId = "test_app_id";
            loginMessage.Properties.ReplyTo = "admin.strategy.key";

            logoutMessage.Properties.AppId = "test_app_id";
            logoutMessage.Properties.ReplyTo = "admin.strategy.key";

            //using (var channel = _advancedBus.OpenPublishChannel())
            {

                _advancedBus.Consume<string>(
                    _strategyAdminQueue, (msg, messageReceivedInfo) =>
                              Task.Factory.StartNew(() =>
                              {
                                  if (msg.Body.Contains("Logon"))
                                  {
                                      logonArrived = true;
                                      manualLogonEvent.Set();
                                  }
                                  else if (msg.Body.Contains("Logout"))
                                  {
                                      logoutArrived = true;
                                      manualLogoutEvent.Set();
                                  }
                              }));


                _advancedBus.Publish(_adminExchange, "marketdata.engine.login", true, false, loginMessage);

                manualLogonEvent.WaitOne(30000, false);
                manualLogoutEvent.WaitOne(30000, false);
            }
            Assert.AreEqual(true, logonArrived, "Logon Status");
            Assert.AreEqual(true, logoutArrived, "Logout Status");
        }
    }
}