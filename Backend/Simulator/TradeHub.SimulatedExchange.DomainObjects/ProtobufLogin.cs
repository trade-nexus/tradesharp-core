
using ProtoBuf;

namespace TradeHub.SimulatedExchange.DomainObjects
{
    [ProtoContract]
    public class ProtobufLogin
    {
        [ProtoMember(1)]
        public string ProviderName { get; set; }
    }
}
