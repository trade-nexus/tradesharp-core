using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using TradeHub.Common.Core.Constants;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.DomainModels.OrderDomain;
using TradeHub.NotificationEngine.Common.Constants;
using TradeHub.NotificationEngine.Common.ValueObject;
using TradeHub.NotificationEngine.NotificationCenter.Services;

namespace TradeHub.NotificationEngine.Server.Tests.Integration
{
    [TestFixture]
    class NotificationControllerTestCases
    {
        private NotificationController _notificationController;

        [SetUp]
        public void SetUp()
        {
            _notificationController = new NotificationController();
        }

        [TearDown]
        public void TearDown()
        {
        }

        [Test]
        [Category("Integration")]
        public void NewNotification_SendEmailNotificationToReceiver()
        {
            OrderNotification orderNotification= new OrderNotification(NotificationType.Email, OrderNotificationType.Accepted);

            Order order = new Order("1234",OrderSide.BUY,100,OrderTif.DAY,"",new Security(){Symbol = "TC"},  OrderExecutionProvider.Simulated);
            orderNotification.SetOrder(order);

            _notificationController.NewNotificationArrived(orderNotification);
        }
    }
}
