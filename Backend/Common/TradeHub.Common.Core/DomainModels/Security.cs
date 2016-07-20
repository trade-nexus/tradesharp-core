using System;
using System.Text;

namespace TradeHub.Common.Core.DomainModels
{
    /// <summary>
    /// This is the base class of all securities in the system.
    /// </summary>
    [Serializable()]
    public class Security
    {
        // Indentifies Type of Security class used
        public virtual string SecurityType
        {
            get { return Constants.MarketData.SecurityTypes.Base; }
            set { ; }
        }
        
        public int Id { get; set; }
        // Name of the security
        public virtual string Symbol { get; set; }

        // International Securities Identification Numbering 
        public virtual string Isin { get; set; }

        /// <summary>
        /// Overrides GetHashCode
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return Symbol.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            // Again just optimization
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;

            // Actually check the type, should not throw exception from Equals override
            if (obj.GetType() != this.GetType()) return false;

            // Call the implementation from IEquatable
            return Equals((Security)obj);
        }

        public bool Equals(Security other)
        {
            // First two lines are just optimizations
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return Symbol.Equals(other.Symbol);
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append("Security :: ");
            stringBuilder.Append(" Type: " + SecurityType);
            stringBuilder.Append(" | Symbol: " + Symbol);
            stringBuilder.Append(" | ISIN: " + Isin);
            return stringBuilder.ToString();
        }
    }
}