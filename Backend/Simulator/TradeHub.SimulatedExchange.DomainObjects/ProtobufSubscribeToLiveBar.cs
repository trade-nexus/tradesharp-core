
using ProtoBuf;

namespace TradeHub.SimulatedExchange.DomainObjects
{
    [ProtoContract]
    public class ProtobufSubscribeToLiveBar
    {
        [ProtoMember(1)]
        public string Id { get; set; }

        [ProtoMember(2)]
        public string Symbol { get; set; }

        [ProtoMember(3)]
        public string MarketDataProvider { get; set; }
    }
}
