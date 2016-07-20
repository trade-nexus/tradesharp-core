using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeHub.Common.Core.Constants;
using TradeHub.Common.Core.DomainModels;

namespace TradeHub.SimulatedExchange.Common.ValueObjects
{
    public class MarketDataObject
    {
        /// <summary>
        /// Indicated whether the object contains valid Tick or Bar
        /// </summary>
        private bool _isTick = false;

        /// <summary>
        /// TradeHub Tick object
        /// </summary>
        private Tick _tick = new Tick(new Security(), MarketDataProvider.SimulatedExchange);

        /// <summary>
        /// TradeHub Bar objecct
        /// </summary>
        private Bar _bar = new Bar(new Security(), MarketDataProvider.SimulatedExchange, "");

        /// <summary>
        /// Indicated whether the object contains valid Tick or Bar
        /// </summary>
        public bool IsTick
        {
            get { return _isTick; }
            set { _isTick = value; }
        }

        /// <summary>
        /// TradeHub Tick object
        /// </summary>
        public Tick Tick
        {
            get { return _tick; }
            set { _tick = value; }
        }

        /// <summary>
        /// TradeHub Bar objecct
        /// </summary>
        public Bar Bar
        {
            get { return _bar; }
            set { _bar = value; }
        }
    }
}
