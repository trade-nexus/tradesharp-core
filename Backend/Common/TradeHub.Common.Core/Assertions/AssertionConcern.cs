using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeHub.Common.Core.Assertions
{
    /// <summary>
    /// Provides the way of dealing errors in arguments
    /// </summary>
    public static class AssertionConcern
    {
        public static void AssertArgumentNotNull(object object1, string message)
        {
            if (object1 == null)
            {
                throw new InvalidOperationException(message);
            }
        }

        public static void AssertArgumentRange(decimal value, decimal minimum, decimal maximum, string message)
        {
            if (value < minimum || value > maximum)
            {
                throw new InvalidOperationException(message);
            }
        }

        public static void AssertEmptyString(string value, string message)
        {
            if (value == null || value.Equals(string.Empty))
            {
                throw new InvalidOperationException(message);
            }
        }

        public static void AssertGreaterThanZero(decimal value, string message)
        {
            if (value <= 0)
            {
                throw new InvalidOperationException(message);
            }
        }

        public static void AssertNullOrEmptyString(string value, string message)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentNullException(message);
            }
        }
    }
}
