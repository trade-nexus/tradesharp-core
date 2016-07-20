using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Spring.Context.Support;
using TraceSourceLogger;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.ValueObjects.AdminMessages;
using TradeHub.Common.Core.ValueObjects.Inquiry;
using TradeHub.Common.Core.ValueObjects.MarketData;
using TradeHub.MarketDataEngine.Client.Constants;
using TradeHub.MarketDataEngine.Client.Service;
using TradeHub.MarketDataEngine.Configuration.Service;
using TradeHub.MarketDataEngine.MarketDataProviderGateway.Service;
using TradeHub.MarketDataEngine.Server.Service;
using TradeHubConstants = TradeHub.Common.Core.Constants;

namespace TradeHub.MarketDataEngine.Client.Tests.Integration
{
    [TestFixture]
    public class MarketDataEngineClientTest
    {
        private MarketDataEngineClient _marketDataEngineClient;
        private ApplicationController _applicationController;
         
        [SetUp]
        public void Setup()
        {
            _applicationController = ContextRegistry.GetContext()["ApplicationController"] as ApplicationController;
           // MqServer mqServer=new MqServer("RabbitMQ.xml",10000, 120000);
           // _applicationController = new ApplicationController(mqServer,new MessageProcessor(new LiveBarGenerator(new BarFactory.Service.BarFactory())));
            if (_applicationController != null) _applicationController.StartServer();
            _marketDataEngineClient = new MarketDataEngineClient();
        }

        [TearDown]
        public void Close()
        {
            _marketDataEngineClient.Shutdown();
            _applicationController.StopServer();
        }

        [Test]
        [Category("Integration")]
        public void AppIDTestCase()
        {
            Thread.Sleep(2000);
            _marketDataEngineClient.Start();
            ManualResetEvent manualAppIDEvent = new ManualResetEvent(false); ;

            manualAppIDEvent.WaitOne(3000, false);

            Assert.NotNull(true, _marketDataEngineClient.AppId, "App ID");
            Assert.AreEqual("A00", _marketDataEngineClient.AppId, "App ID Value");
        }

        [Test]
        [Category("Integration")]
        public void ConnectivityTestCase()
        {

            bool logonArrived = false;
            bool logoutArrived = false;

            ManualResetEvent manualLogonEvent = new ManualResetEvent(false);
            ManualResetEvent manualLogoutEvent = new ManualResetEvent(false);
            ManualResetEvent manualConnectedEvent = new ManualResetEvent(false);

            _marketDataEngineClient.ServerConnected += delegate()
            {
                _marketDataEngineClient.SendLoginRequest(new Login() { MarketDataProvider = TradeHubConstants.MarketDataProvider.Simulated });
                manualConnectedEvent.Set();
            };

            _marketDataEngineClient.LogonArrived +=
                    delegate(string obj)
                    {
                        logonArrived = true;
                        _marketDataEngineClient.SendLogoutRequest(new Logout { MarketDataProvider = TradeHubConstants.MarketDataProvider.Simulated });
                        manualLogonEvent.Set();
                    };

            _marketDataEngineClient.LogoutArrived +=
                    delegate(string obj)
                    {
                        logoutArrived = true;
                        manualLogoutEvent.Set();
                    };

            _marketDataEngineClient.Start();

            manualConnectedEvent.WaitOne(30000, false);
            manualLogonEvent.WaitOne(30000, false);
            manualLogoutEvent.WaitOne(30000, false);

            Assert.AreEqual(true, logonArrived, "Logon Arrived");
            Assert.AreEqual(true, logoutArrived, "Logout Arrived");
        }

        [Test]
        [Category("Integration")]
        public void MarketDataProviderInfoTestCase()
        {

            bool logonArrived = false;
            bool logoutArrived = false;
            bool inquiryResponseArrived = false;

            ManualResetEvent manualLogonEvent = new ManualResetEvent(false);
            ManualResetEvent manualLogoutEvent = new ManualResetEvent(false);
            ManualResetEvent manualConnectedEvent = new ManualResetEvent(false);
            ManualResetEvent manualInquiryResponseEvent = new ManualResetEvent(false);

            _marketDataEngineClient.ServerConnected += delegate()
            {
                _marketDataEngineClient.SendLoginRequest(new Login() { MarketDataProvider = TradeHubConstants.MarketDataProvider.Simulated });
                manualConnectedEvent.Set();
            };

            _marketDataEngineClient.LogonArrived +=
                    delegate(string obj)
                    {
                        logonArrived = true;

                        _marketDataEngineClient.SendInquiryRequest(TradeHubConstants.MarketDataProvider.Simulated);
                        manualLogonEvent.Set();
                    };

            _marketDataEngineClient.InquiryResponseArrived +=
                    delegate(MarketDataProviderInfo obj)
                    {
                        inquiryResponseArrived = true;
                        Console.WriteLine(obj);
                        _marketDataEngineClient.SendLogoutRequest(new Logout { MarketDataProvider = TradeHubConstants.MarketDataProvider.Simulated });
                        manualInquiryResponseEvent.Set();
                        
                    };

            _marketDataEngineClient.LogoutArrived +=
                    delegate(string obj)
                    {
                        logoutArrived = true;
                        manualLogoutEvent.Set();
                    };

            _marketDataEngineClient.Start();

            manualConnectedEvent.WaitOne(30000, false);
            manualLogonEvent.WaitOne(30000, false);
            manualInquiryResponseEvent.WaitOne(30000, false);
            manualLogoutEvent.WaitOne(30000, false);

            Assert.AreEqual(true, logonArrived, "Logon Arrived");
            Assert.AreEqual(true, inquiryResponseArrived, "Inquiry Response Arrived");
            Assert.AreEqual(true, logoutArrived, "Logout Arrived");
        }

        [Test]
        [Category("Integration")]
        public void MarketDataProviderTickTestCase()
        {

            bool logonArrived = false;
            bool logoutArrived = false;
            bool inquiryResponseArrived = false;
            int count = 1;
            Stopwatch stopwatch=new Stopwatch();

            ManualResetEvent manualLogonEvent = new ManualResetEvent(false);
            ManualResetEvent manualLogoutEvent = new ManualResetEvent(false);
            ManualResetEvent manualConnectedEvent = new ManualResetEvent(false);
            ManualResetEvent manualInquiryResponseEvent = new ManualResetEvent(false);
            ManualResetEvent manualTickArrivedEvent=new ManualResetEvent(false);

            _marketDataEngineClient.ServerConnected += delegate()
            {
                _marketDataEngineClient.SendLoginRequest(new Login() { MarketDataProvider = TradeHubConstants.MarketDataProvider.Simulated });
                manualConnectedEvent.Set();
            };

            _marketDataEngineClient.LogonArrived +=
                    delegate(string obj)
                    {
                        logonArrived = true;

                        _marketDataEngineClient.SendInquiryRequest(TradeHubConstants.MarketDataProvider.Simulated);
                        manualLogonEvent.Set();
                    };

            _marketDataEngineClient.InquiryResponseArrived +=
                    delegate(MarketDataProviderInfo obj)
                    {
                        inquiryResponseArrived = true;
                        Console.WriteLine(obj);
                       // _marketDataEngineClient.SendLogoutRequest(new Logout { MarketDataProvider = TradeHubConstants.MarketDataProvider.Simulated });
                        Subscribe subscribe=new Subscribe();
                        subscribe.MarketDataProvider = TradeHubConstants.MarketDataProvider.Simulated;
                        subscribe.Security=new Security(){Symbol = "IBM"};
                        _marketDataEngineClient.SendTickSubscriptionRequest(subscribe);
                        manualInquiryResponseEvent.Set();

                    };
            _marketDataEngineClient.TickArrived += delegate(Tick tick)
            {
                if (count == 1)
                {
                    stopwatch.Start();
                }
                if (count == 1000000)
                {
                    stopwatch.Stop();
                    Console.WriteLine("1000000 Ticks recevied in "+stopwatch.ElapsedMilliseconds+" ms");
                    _marketDataEngineClient.SendLogoutRequest(new Logout { MarketDataProvider = TradeHubConstants.MarketDataProvider.Simulated });
                    manualTickArrivedEvent.Set();
                  //  Logger.Info("Recevied Tick=" + tick, "", "MarketDataProviderBarTestCase");
                   
                }
                count++;


            };

            _marketDataEngineClient.LogoutArrived +=
                    delegate(string obj)
                    {
                        logoutArrived = true;
                        manualLogoutEvent.Set();
                    };

            _marketDataEngineClient.Start();

            manualConnectedEvent.WaitOne(30000, false);
            manualLogonEvent.WaitOne(30000, false);
            manualInquiryResponseEvent.WaitOne(30000, false);
            manualLogoutEvent.WaitOne(30000, false);
            manualTickArrivedEvent.WaitOne(500000);

            Assert.AreEqual(true, logonArrived, "Logon Arrived");
            Assert.AreEqual(true, inquiryResponseArrived, "Inquiry Response Arrived");
            Assert.AreEqual(1000001, count, "Tick arrived");
            Assert.AreEqual(true, logoutArrived, "Logout Arrived");
            Console.WriteLine("1000000 Ticks recevied in " + stopwatch.ElapsedMilliseconds + " ms");
            Console.WriteLine(1000000/stopwatch.ElapsedMilliseconds*1000 +"msg/sec");
        }

        [Test]
        [Category("Integration")]
        public void MarketDataProviderBarTestCase()
        {

            bool logonArrived = false;
            bool logoutArrived = false;
            bool inquiryResponseArrived = false;
            int count = 1;
            Stopwatch stopwatch = new Stopwatch();

            ManualResetEvent manualLogonEvent = new ManualResetEvent(false);
            ManualResetEvent manualLogoutEvent = new ManualResetEvent(false);
            ManualResetEvent manualConnectedEvent = new ManualResetEvent(false);
            ManualResetEvent manualInquiryResponseEvent = new ManualResetEvent(false);
            ManualResetEvent manualBarArrivedEvent = new ManualResetEvent(false);

            _marketDataEngineClient.ServerConnected += delegate()
            {
                _marketDataEngineClient.SendLoginRequest(new Login() { MarketDataProvider = TradeHubConstants.MarketDataProvider.Simulated });
                manualConnectedEvent.Set();
            };

            _marketDataEngineClient.LogonArrived +=
                    delegate(string obj)
                    {
                        logonArrived = true;

                        _marketDataEngineClient.SendInquiryRequest(TradeHubConstants.MarketDataProvider.Simulated);
                        manualLogonEvent.Set();
                    };

            _marketDataEngineClient.InquiryResponseArrived +=
                    delegate(MarketDataProviderInfo obj)
                    {
                        inquiryResponseArrived = true;
                        Console.WriteLine(obj);
                        // _marketDataEngineClient.SendLogoutRequest(new Logout { MarketDataProvider = TradeHubConstants.MarketDataProvider.Simulated });
                        BarDataRequest barDataRequest=new BarDataRequest();
                        barDataRequest.Id = "A00";
                        barDataRequest.Security = new Security() {Symbol = "ERX"};
                        barDataRequest.PipSize = 0.0001m;
                        barDataRequest.BarLength = 30;
                        barDataRequest.BarPriceType = TradeHubConstants.BarPriceType.LAST;
                        barDataRequest.MarketDataProvider = TradeHubConstants.MarketDataProvider.Simulated;
                        barDataRequest.BarFormat = TradeHubConstants.BarFormat.TIME;
                        barDataRequest.BarSeed = 0;
                        _marketDataEngineClient.SendLiveBarSubscriptionRequest(barDataRequest);
                        manualInquiryResponseEvent.Set();

                    };
            _marketDataEngineClient.LiveBarArrived += delegate(Bar bar)
            {
               _marketDataEngineClient.SendLogoutRequest(new Logout { MarketDataProvider = TradeHubConstants.MarketDataProvider.Simulated });
                manualBarArrivedEvent.Set();
                count++;
                Logger.Info("Recevied Bar=" + bar, "", "MarketDataProviderBarTestCase");
                
            };


            _marketDataEngineClient.LogoutArrived +=
                    delegate(string obj)
                    {
                        logoutArrived = true;
                        manualLogoutEvent.Set();
                    };

            _marketDataEngineClient.Start();

            manualConnectedEvent.WaitOne(30000, false);
            manualLogonEvent.WaitOne(30000, false);
            manualInquiryResponseEvent.WaitOne(30000, false);
            manualLogoutEvent.WaitOne(30000, false);
            manualBarArrivedEvent.WaitOne(500000);

            Assert.AreEqual(true, logonArrived, "Logon Arrived");
            Assert.AreEqual(true, inquiryResponseArrived, "Inquiry Response Arrived");
            Assert.AreEqual(2, count, "Bar arrived");
            Assert.AreEqual(true, logoutArrived, "Logout Arrived");
           
        }
    }


}
