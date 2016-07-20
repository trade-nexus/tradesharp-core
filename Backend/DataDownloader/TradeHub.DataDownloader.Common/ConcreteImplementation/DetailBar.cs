using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TradeHub.Common.Core.DomainModels;

namespace TradeHub.DataDownloader.Common.ConcreteImplementation
{
    /// <summary>
    /// Contains Additional Information of Bar Object
    /// </summary>
    public class DetailBar:Bar
    {
        // Format on which to generate bars
        public string BarFormat { get; set; }

        // Lenght of required Bar
        public decimal BarLength { get; set; }

        // Bar Pip Size
        public decimal PipSize { get; set; }

        // Bar Seed
        public decimal BarSeed { get; set; }

        // Price Type to be used for generating Bars
        public string BarPriceType { get; set; }

        public DetailBar(Bar bar):base(bar.RequestId)
        {
            foreach (PropertyInfo prop in bar.GetType().GetProperties())
                GetType().GetProperty(prop.Name).SetValue(this, prop.GetValue(bar, null), null);             
        }
    }
}
