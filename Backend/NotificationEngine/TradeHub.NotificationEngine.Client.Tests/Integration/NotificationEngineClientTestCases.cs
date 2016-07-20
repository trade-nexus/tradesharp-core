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
using TradeHub.Common.Core.DomainModels.OrderDomain;
using TradeHub.NotificationEngine.Client.Service;
using TradeHub.NotificationEngine.Common.Constants;
using TradeHub.NotificationEngine.Common.ValueObject;
using TradeHub.NotificationEngine.CommunicationManager.Service;

namespace TradeHub.NotificationEngine.Client.Tests.Integration
{
    [TestFixture]
    class NotificationEngineClientTestCases
    {
        private NotificationEngineMqServer _notificationEngineMqServer;
        private NotificationEngineClient _notificationEngineClient;

        [SetUp]
        public void SetUp()
        {
            _notificationEngineMqServer = new NotificationEngineMqServer("NotificationEngineMqConfig.xml");
            _notificationEngineMqServer.Connect();

            _notificationEngineClient = ContextRegistry.GetContext()["NotificationEngineClient"] as NotificationEngineClient;
            if (_notificationEngineClient != null) _notificationEngineClient.StartCommunicator();
        }

        [TearDown]
        public void TearDown()
        {
            if (_notificationEngineClient != null) _notificationEngineClient.StopCommunicator();
            _notificationEngineMqServer.Disconnect();
        }

        [Test]
        [Category("Integration")]
        public void NewNotification_SendNotificationToServer_NotificationReceivedByServer()
        {
            Thread.Sleep(5000);

            bool notificationReceived = false;
            var notificationManualResetEvent = new ManualResetEvent(false);

            // Create Order Object
            OrderNotification orderNotification = new OrderNotification(NotificationType.Email, OrderNotificationType.Accepted);
            MarketOrder marketOrder= new MarketOrder("Test Provider");

            orderNotification.SetOrder(marketOrder);

            _notificationEngineMqServer.OrderNotificationEvent += delegate(OrderNotification notificationObject)
            {
                var notificationObjectReceived = notificationObject;
                notificationReceived = true;
                notificationManualResetEvent.Set();
            };

            _notificationEngineClient.SendNotification(orderNotification);

            notificationManualResetEvent.WaitOne(10000, false);

            Assert.AreEqual(true, notificationReceived, "Notification Received");
        }
    }
}
