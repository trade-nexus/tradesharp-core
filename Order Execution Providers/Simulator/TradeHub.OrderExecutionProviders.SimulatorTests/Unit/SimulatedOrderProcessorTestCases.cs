using System.Threading;
using NUnit.Framework;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.DomainModels.OrderDomain;
using TradeHub.OrderExecutionProviders.Simulator.Service;

namespace TradeHub.OrderExecutionProviders.SimulatorTests.Unit
{
    [TestFixture]
    class SimulatedOrderProcessorTestCases
    {
        private SimulatedOrderProcessor _orderProcessor;

        [SetUp]
        public void SetUp()
        {
            _orderProcessor = new SimulatedOrderProcessor();
        }

        [Test]
        [Category("Unit")]
        public void NewArrivedTestCase()
        {
            string sampleInput = "N ID";

            bool newArrived = false;
            var manualNewEvent = new ManualResetEvent(false);
            _orderProcessor.NewArrived +=
                    delegate(Order obj)
                    {
                        newArrived = true;
                        manualNewEvent.Set();
                    };

            _orderProcessor.ProcessIncomingMessage(sampleInput);
            manualNewEvent.WaitOne(3000, false);

            Assert.AreEqual(true, newArrived, "New Recieved from Simulated Order Processor");
        }

        [Test]
        [Category("Unit")]
        public void CancellationTestCase()
        {
            string sampleInput = "C ID";

            bool cancellationArrived = false;
            var manualCancellationEvent = new ManualResetEvent(false);
            _orderProcessor.CancellationArrived +=
                    delegate(Order obj)
                    {
                        cancellationArrived = true;
                        manualCancellationEvent.Set();
                    };

            _orderProcessor.ProcessIncomingMessage(sampleInput);
            manualCancellationEvent.WaitOne(3000, false);

            Assert.AreEqual(true, cancellationArrived, "Cancellation Recieved from Simulated Order Processor");
        }

        [Test]
        [Category("Unit")]
        public void ExecutionTestCase()
        {
            string sampleInput = "E ID 1.00 100";

            bool executionArrived = false;
            var manualExecutionEvent = new ManualResetEvent(false);
            _orderProcessor.ExecutionArrived +=
                    delegate(Execution obj)
                    {
                        executionArrived = true;
                        manualExecutionEvent.Set();
                    };

            _orderProcessor.ProcessIncomingMessage(sampleInput);
            manualExecutionEvent.WaitOne(3000, false);

            Assert.AreEqual(true, executionArrived, "Execution Recieved from Simulated Order Processor");
        }

        [Test]
        [Category("Unit")]
        public void RejectionTestCase()
        {
            string sampleInput = "R ID TEST";

            bool rejectionArrived = false;
            var manualRejectionEvent = new ManualResetEvent(false);
            _orderProcessor.RejectionArrived +=
                    delegate(Rejection obj)
                    {
                        rejectionArrived = true;
                        manualRejectionEvent.Set();
                    };

            _orderProcessor.ProcessIncomingMessage(sampleInput);
            manualRejectionEvent.WaitOne(3000, false);

            Assert.AreEqual(true, rejectionArrived, "Rejection Recieved from Simulated Order Processor");
        }

        [Test]
        [Category("Unit")]
        public void LocateTestCase()
        {
            string sampleInput = "L ID 2.14 100";

            bool locateArrived = false;
            var manualLocateEvent = new ManualResetEvent(false);
            _orderProcessor.LocateMessageArrived +=
                    delegate(LimitOrder obj)
                    {
                        locateArrived = true;
                        manualLocateEvent.Set();
                    };

            _orderProcessor.ProcessIncomingMessage(sampleInput);
            manualLocateEvent.WaitOne(3000, false);

            Assert.AreEqual(true, locateArrived, "Locate Message Recieved from Simulated Order Processor");
        }
        [Test]
        [Category("Unit")]
        public void PositionTestCase()
        {
            string sampleInput = "P AAPL 12 12";

            bool positionArrived = false;
            var manualPositionEvent = new ManualResetEvent(false);
            _orderProcessor.PositionArrived +=
                    delegate(Position obj)
                    {
                        positionArrived = true;
                        manualPositionEvent.Set();
                    };

            _orderProcessor.ProcessIncomingMessage(sampleInput);
            manualPositionEvent.WaitOne(3000, false);

            Assert.AreEqual(true, positionArrived, "Position Message Recieved from Simulated Order Processor");
        }
    }
}
