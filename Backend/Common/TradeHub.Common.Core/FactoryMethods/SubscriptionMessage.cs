using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.ValueObjects.MarketData;

namespace TradeHub.Common.Core.FactoryMethods
{
    /// <summary>
    /// Returns Subscription objects
    /// </summary>
    public static class SubscriptionMessage
    {
        /// <summary>
        /// Creates Tick subscription message
        /// </summary>
        /// <param name="id">Unique Subscription ID</param>
        /// <param name="security">TradeHub Security containing info regarding the symbol</param>
        /// <param name="marketDataProvider">Name of market data provider to be used</param>
        /// <returns>TradeHub Subscribe object</returns>
        public static Subscribe TickSubscription(string id, Security security, string marketDataProvider)
        {
            // Create new tick subscription object
            Subscribe subscribe = new Subscribe
                {
                    Id = id,
                    Security = security,
                    MarketDataProvider = marketDataProvider
                };
            
            return subscribe;
        }

        /// <summary>
        /// Creates Tick unsubscription message
        /// </summary>
        /// <param name="id">Unique ID which was used for subscription</param>
        /// <param name="security">TradeHub Security containing info regarding the symbol</param>
        /// <param name="marketDataProvider">Name of market data provider to be used</param>
        /// <returns>TradeHub Unsubscribe object</returns>
        public static Unsubscribe TickUnsubscription(string id, Security security, string marketDataProvider)
        {
            // Create new tick unsubscription object
            Unsubscribe unsubscribe = new Unsubscribe
            {
                Id = id,
                Security = security,
                MarketDataProvider = marketDataProvider
            };

            return unsubscribe;
        }

        /// <summary>
        /// Creates Live Bar subscription message
        /// </summary>
        /// <param name="id">Unique Subscription ID</param>
        /// <param name="security">TradeHub Security containing info regarding the symbol</param>
        /// <param name="barFormat">Format on which to generate bars</param>
        /// <param name="barPriceType">Price Type to be used for generating Bars</param>
        /// <param name="barLength">Lenght of required Bar</param>
        /// <param name="pipSize">Bar Pip Size</param>
        /// <param name="barSeed">Bar Seed</param>
        /// <param name="marketDataProvider">Name of market data provider to be used</param>
        /// <returns>TradeHub BarDataRequest object</returns>
        public static BarDataRequest LiveBarSubscription(string id, Security security,string barFormat, string barPriceType,decimal barLength, 
            decimal pipSize, decimal barSeed, string marketDataProvider)
        {
            // Create new Live Bar request message
            BarDataRequest barDataRequest = new BarDataRequest
                {
                    Id = id,
                    Security = security,
                    BarFormat = barFormat,
                    BarPriceType = barPriceType,
                    BarLength = barLength,
                    PipSize = pipSize,
                    BarSeed = barSeed,
                    MarketDataProvider = marketDataProvider
                };
            return barDataRequest;
        }

        /// <summary>
        /// Creates Live Bar unsubscription message
        /// </summary>
        /// <param name="id">Unique ID which was used for subscription</param>
        /// <param name="security">TradeHub Security containing info regarding the symbol</param>
        /// <param name="barFormat">Format on which to generate bars</param>
        /// <param name="barPriceType">Price Type to be used for generating Bars</param>
        /// <param name="barLength">Lenght of required Bar</param>
        /// <param name="pipSize">Bar Pip Size</param>
        /// <param name="barSeed">Bar Seed</param>
        /// <param name="marketDataProvider">Name of market data provider to be used</param>
        /// <returns>TradeHub BarDataRequest object</returns>
        public static BarDataRequest LiveBarUnsubscription(string id, Security security,string barFormat, string barPriceType,decimal barLength, 
            decimal pipSize, decimal barSeed, string marketDataProvider)
        {
            return LiveBarSubscription(id, security, barFormat, barPriceType, barLength, pipSize, barSeed, marketDataProvider);
        }

        /// <summary>
        /// Creates Historic Data subscription message
        /// </summary>
        /// <param name="id">Unique Subscription ID</param>
        /// <param name="security">TradeHub Security containing info regarding the symbol</param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="interval"></param>
        /// <param name="barType"></param>
        /// <param name="marketDataProvider">Name of market data provider to be used</param>
        /// <returns>TradeHub HistoricDataRequest object</returns>
        public static HistoricDataRequest HistoricDataSubscription(string id, Security security, DateTime startTime, DateTime endTime,
                                uint interval, string barType, string marketDataProvider)
        {
            // Create new Historic Data request message
            HistoricDataRequest historicDataRequest = new HistoricDataRequest
                {
                    Id = id,
                    Security = security,
                    StartTime = startTime,
                    EndTime = endTime,
                    Interval = interval,
                    BarType = barType,
                    MarketDataProvider = marketDataProvider
                };

            return historicDataRequest;
        }
    }
}
