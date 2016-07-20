
using EasyNetQ;

namespace TradeHub.SimulatedExchange.Common
{
    class EasyNetQbus
    {
        private IBus _queueBus;
        //public static IAdvancedBus RabbitBus;
        private IBus QueueBus
        {
            get { return _queueBus ?? (_queueBus = RabbitHutch.CreateBus("host=localhost")); }
        }

        //public static IExchange SimulatedOrderExchange; 
        //        public EasyNetQbus()
        //        {
        //            RabbitBus = RabbitBus ?? RabbitHutch.CreateBus("host=127.0.0.1:5678;requestedHeartbeat=10").Advanced;
        //            SimulatedOrderExchange = Exchange.DeclareDirect("SimulatedOrderExchange");
        //        }
    }
}
