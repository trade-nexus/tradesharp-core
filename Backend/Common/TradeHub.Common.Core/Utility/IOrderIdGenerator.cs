using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace TradeHub.Common.Core.Utility
{
    /// <summary>
    /// Blue print for Order ID generator
    /// </summary>
    public interface IOrderIdGenerator
    {
        /// <summary>
        /// Gets next unique order id for the session
        /// </summary>
        /// <returns>string value to be used as Order ID</returns>
        string GetId(string appender = "");
    }
}
