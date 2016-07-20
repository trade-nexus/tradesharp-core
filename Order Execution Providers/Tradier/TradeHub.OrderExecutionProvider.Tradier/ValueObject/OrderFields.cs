using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeHub.OrderExecutionProvider.Tradier.ValueObject
{
    /// <summary>
    /// Tradier order representation
    /// </summary>
    public class OrderFields
    {
        public int id { get; set; }
        public string type { get; set; }
        public string symbol { get; set; }
        public string side { get; set; }
        public int quantity { get; set; }
        public string status { get; set; }
        public string duration { get; set; }
        public double price { get; set; }
        public int avg_fill_price { get; set; }
        public int exec_quantity { get; set; }
        public decimal last_fill_price { get; set; }
        public int last_fill_quantity { get; set; }
        public int remaining_quantity { get; set; }
        public string create_date { get; set; }
        public string transaction_date { get; set; }
        public string @class { get; set; }
        public int num_legs { get; set; }
        public string strategy { get; set; }
        public string option_symbol { get; set; }

        public override string ToString()
        {
            StringBuilder stringBuilder=new StringBuilder();
            stringBuilder.Append("ID=" + id);
            stringBuilder.Append(", type=" + type);
            stringBuilder.Append(", symbol=" + symbol);
            stringBuilder.Append(", side=" + side);
            stringBuilder.Append(", quantity=" + quantity);
            stringBuilder.Append(", status=" + status);
            stringBuilder.Append(", price=" + price);
            stringBuilder.Append(", avg_fill_price=" + avg_fill_price);
            stringBuilder.Append(", exec_quantity=" + exec_quantity);
            stringBuilder.Append(", last_fill_price=" + last_fill_price);
            stringBuilder.Append(", last_fill_quantity=" + last_fill_quantity);
            stringBuilder.Append(", remaining_quantity=" + remaining_quantity);
            stringBuilder.Append(", create_date=" + create_date);
            stringBuilder.Append(", transaction_date=" + transaction_date);
            stringBuilder.Append(", class=" + @class);
            stringBuilder.Append(", num_legs=" + num_legs);
            stringBuilder.Append(", strategy=" + strategy);
            stringBuilder.Append(", option_symbol=" + option_symbol);
            return stringBuilder.ToString();
        }
    }
}
