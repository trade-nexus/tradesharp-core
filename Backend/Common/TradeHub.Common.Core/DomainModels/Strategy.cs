using System;

namespace TradeHub.Common.Core.DomainModels
{
    /// <summary>
    /// Each object of this class represents a running strategy within the system.
    /// </summary>
    public class Strategy
    {
        /// <summary>
        /// Database id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Name of the strategy
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// DateTime when the strategy started.
        ///  </summary>
        public DateTime StartDateTime { get; set; }
    }
}
