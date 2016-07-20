using System;
using ProtoBuf;

namespace TradeHub.SimulatedExchange.DomainObjects
{
    [ProtoContract]
    public class ProtobufBar
    {
        /// <summary>
        /// Close price of bar
        /// </summary>
        [ProtoMember(1)]
        public decimal Close { get; set; }

        /// <summary>
        /// High Price of Bar
        /// </summary>
        [ProtoMember(2)]
        public decimal High { get; set; }

        /// <summary>
        /// Low Price of Bar 
        /// </summary>
        [ProtoMember(3)]
        public decimal Low { get; set; }

        /// <summary>
        /// Open Price of Bar
        /// </summary>
        [ProtoMember(4)]
        public decimal Open { get; set; }

        /// <summary>
        /// Symbol of a bar
        /// </summary>
        [ProtoMember(5)]
        public string Symbol { get; set; }

        /// <summary>
        /// Name of Market Data Provider
        /// </summary>
        [ProtoMember(6)]
        public decimal MarketDataProvider { get; set; }

        /// <summary>
        /// Saves DataTime Of Bar
        /// </summary>
        [ProtoMember(7)]
        public DateTime DateTime { get; set; }
    }

}
