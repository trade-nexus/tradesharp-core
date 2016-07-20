using TradeHub.Common.Core.DomainModels;

namespace TradeHub.DataDownloader.Common.Interfaces
{
    public interface IWriter
    {
        /// <summary>
        /// Proides interface to write data to file.
        /// It could be a Tick data or Bar data
        /// </summary>
        /// <param name="dataEvent"> </param>
        void Write(MarketDataEvent dataEvent);
    }
}