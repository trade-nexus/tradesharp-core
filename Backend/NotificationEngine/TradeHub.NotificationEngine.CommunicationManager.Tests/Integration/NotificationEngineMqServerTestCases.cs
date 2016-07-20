using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EasyNetQ;
using EasyNetQ.Topology;
using NUnit.Framework;
using RabbitMQ.Client;
using TradeHub.Common.Core.Constants;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.DomainModels.OrderDomain;
using TradeHub.NotificationEngine.Common.Constants;
using TradeHub.NotificationEngine.Common.ValueObject;
using TradeHub.NotificationEngine.CommunicationManager.Service;
using ExchangeType = EasyNetQ.Topology.ExchangeType;

namespace TradeHub.NotificationEngine.CommunicationManager.Tests.Integration
{
    [TestFixture]
    public class NotificationEngineMqServerTestCases
    {
        private NotificationEngineMqServer _notificationEngineMqServer;

        // MQ Fields
        private IAdvancedBus _advancedBus;
        private IExchange _adminExchange;

        [SetUp]
        public void SetUp()
        {
            _notificationEngineMqServer = new NotificationEngineMqServer("NotificationEngineMqConfig.xml");

            _notificationEngineMqServer.Connect();

            // Initialize Advance Bus
            _advancedBus = RabbitHutch.CreateBus("host=localhost").Advanced;

            // Create a admin exchange
            _adminExchange = _advancedBus.ExchangeDeclare("notificationengine_exchange", ExchangeType.Direct, false, true, false);
        }

        [TearDown]
        public void TearDown()
        {
            _advancedBus.Dispose();
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

            _notificationEngineMqServer.OrderNotificationEvent += delegate(OrderNotification notificationObject)
            {
                notificationReceived = true;
                notificationManualResetEvent.Set();
            };

            IMessage<OrderNotification> message = new Message<OrderNotification>(orderNotification);
            _advancedBus.Publish(_adminExchange, "notificationengine.order.message", true, false, message);

            notificationManualResetEvent.WaitOne(10000, false);

            Assert.AreEqual(true, notificationReceived, "Notification Received");
        }
    }
}
