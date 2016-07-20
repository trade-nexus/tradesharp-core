using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using TradeHub.Common.Core.Constants;
using TradeHub.Common.Core.DomainModels;
using TradeHub.PositionEngine.Client.Service;
using TradeHub.PositionEngine.Configuration.Service;
using TradeHub.PositionEngine.ProviderGateway.Service;
using TradeHub.PositionEngine.Service;

namespace TradeHub.PositionEngine.Client.Tests
{
    [TestFixture]
    class PositionEngineClientTest
    {
        private PositionEngineClient _positionEngineClient;
        private ApplicationController _applicationController;

        [SetUp]
        public void Setup()
        {
            _applicationController = new ApplicationController(new PositionEngineMqServer("PEMQConfig.xml"),
                new PositionMessageProcessor());
            _applicationController.StartServer();
            _positionEngineClient=new PositionEngineClient();
        }

        [TearDown]
        public void Close()
        {
            _positionEngineClient.Shutdown();
            _applicationController.StopServer();
        }

        [Test]
        [Category("Integration")]
        public void AppIDTestCase()
        {
            Thread.Sleep(2000);
            _positionEngineClient.Initialize(OrderExecutionProvider.Simulated);
            ManualResetEvent manualAppIDEvent = new ManualResetEvent(false); ;

            manualAppIDEvent.WaitOne(3000, false);

            Assert.NotNull(true, _positionEngineClient.AppId, "App ID");
            Assert.AreEqual("A00", _positionEngineClient.AppId, "App ID Value");
        }

        [Test]
        [Category("Integration")]
        public void PositionTestCase()
        {
            Thread.Sleep(2000);
            bool PositionArrived = false;
            ManualResetEvent manualPositionEvent=new ManualResetEvent(false);
            manualPositionEvent.WaitOne(4000, false);
            //_positionEngineClient.SubscribeProviderPosition();
           _positionEngineClient.PositionArrived += delegate
            {
                PositionArrived = true;
                manualPositionEvent.Set();
            };
            Assert.AreEqual(true,PositionArrived,"PositionArrived");
        }
    }
}
