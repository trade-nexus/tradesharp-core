using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeHub.Common.Core.DomainModels;

namespace TradeHub.Infrastructure.FileWriter.Interface
{
    public interface IWriter
    {
        /// <summary>
        /// Allows writing ticks in intended format
        /// </summary>
        /// <param name="tick"> </param>
        void Write(Tick tick);

        /// <summary>
        /// Allows writing bars in intended format
        /// </summary>
        /// <param name="bar"></param>
        /// <param name="barFormat"></param>
        /// <param name="barPriceType"></param>
        /// <param name="barLength"></param>
        void Write(Bar bar, string barFormat, string barPriceType, string barLength);

        /// <summary>
        /// Allows writing hitorical bars in intended format
        /// </summary>
        /// <param name="historicBarData"> </param>
        void Write(HistoricBarData historicBarData);
    }
}
