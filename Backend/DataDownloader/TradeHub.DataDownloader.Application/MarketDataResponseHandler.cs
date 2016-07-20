using System;
using System.Collections.Generic;
using System.Linq;
using TraceSourceLogger;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.ValueObjects.MarketData;
using TradeHub.DataDownloader.BinaryFileWriter;
using TradeHub.DataDownloader.Common.ConcreteImplementation;
using TradeHub.DataDownloader.Common.Interfaces;
using TradeHub.DataDownloader.CsvFileWriter;

namespace TradeHub.DataDownloader.ApplicationCenter
{
    /// <summary>
    /// Main purpose of this class is to 
    /// decide where to write Tick Or Bar Data.
    /// As a constructor parameter it 
    /// takes list of all the writes available.
    /// </summary>
    public class MarketDataResponseHandler
    {
        private static Type _oType = typeof (MarketDataResponseHandler);

        private IWriter _writerCsv;
        private IWriter _writerBinary;
        //TODO: Get Dictionary Rapper From Taimoor
        public IDictionary<string, ProviderPermission> ProviderPermissionDictionary { get; private set; }
        public IDictionary<string, SecurityPermissions> SecurityPermissionDictionary { get; private set; }
        public IList<BarDataRequest> BarDataRequests;
        private object _lock = new object();

        /// <summary>
        /// List of available writers is passed to constructor.   
        /// </summary>
        /// <param name="writers"></param>
        public MarketDataResponseHandler(IList<IWriter> writers)
        {
            try
            {
                foreach (var writer in writers)
                {
                    if (writer is FileWriterBinany && _writerBinary == null)
                    {
                        _writerBinary = writer;
                    }
                    else if (writer is FileWriterCsv && _writerCsv == null)
                    {
                        _writerCsv = writer;
                    }
                }
                ProviderPermissionDictionary = new Dictionary<string, ProviderPermission>();
                SecurityPermissionDictionary = new Dictionary<string, SecurityPermissions>();
                BarDataRequests=new List<BarDataRequest>();
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _oType.FullName, "ResponseHandler");
            }
        }

        /// <summary>
        /// Method To Write Bar data
        /// </summary>
        /// <param name="bar"></param>
        public void HandleBarArrived(Bar bar)
        {
            try
            {
                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug(bar.ToString(), _oType.FullName, "HandleBarArrived");
                }
                // Check if application have subscribe this bar or not
                // NOTE: Check Latency 

                var providerPermissions = ProviderPermissionDictionary[bar.MarketDataProvider];
                var securityPermissions = SecurityPermissionDictionary[bar.Security.Symbol];
                if (providerPermissions.WriteCsv && securityPermissions.WriteBars && _writerCsv != null)
                {
                    _writerCsv.Write(ChangeBarToDetailBar(bar));
                }
                if (providerPermissions.WriteBinary && securityPermissions.WriteBars && _writerBinary != null)
                {
                    _writerBinary.Write(bar);
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _oType.FullName, "HandleBarArrived");
            }
        }

        /// <summary>
        /// Methord to Write Tick Data
        /// </summary>
        /// <param name="tick"></param>
        public void HandleTickArrived(Tick tick)
        {
            lock (_lock)
            {
                try
                {
                    var providerPermissions = ProviderPermissionDictionary[tick.MarketDataProvider];
                    var securityPermissions = SecurityPermissionDictionary[tick.Security.Symbol];
                    if (providerPermissions.WriteCsv && (securityPermissions.WriteQuote || securityPermissions.WriteTrade) && _writerCsv != null)
                    {
                        _writerCsv.Write(ChangeTickAccordingToPermissions(tick, securityPermissions.WriteQuote, securityPermissions.WriteTrade));
                    }
                    if (providerPermissions.WriteBinary && (securityPermissions.WriteQuote || securityPermissions.WriteTrade) && _writerBinary != null)
                    {
                        _writerBinary.Write(tick);
                    }
                }
                catch (Exception exception)
                {
                    Logger.Error(exception, _oType.FullName, "HandleTickArrived");
                }
            }
        }

        /// <summary>
        /// When New Provider Is Connected
        /// Saving its Permissions in a dictionary
        /// </summary>
        /// <param name="permissions">Contains Permission Of Certain Provider</param>
        public void OnProviderConnected(ProviderPermission permissions)
        {
            if (Logger.IsDebugEnabled)
            {
                Logger.Info(permissions.ToString(), _oType.FullName, "OnProviderConnected");
            }
            try
            {
                // Add permission if they don't exist in the local map
                if (!ProviderPermissionDictionary.ContainsKey(permissions.MarketDataProvider))
                {
                    ProviderPermissionDictionary.Add(permissions.MarketDataProvider, permissions);
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _oType.FullName, "OnProviderConnected");
            }
        }


        /// <summary>
        /// When Provider Is disconnected
        /// Removing its Permission from Dictionary
        /// </summary>
        /// <param name="permissions">Contains Permission Of Certain Provider</param>
        public void OnProviderDisconnect(ProviderPermission permissions)
        {
            if (Logger.IsDebugEnabled)
            {
                Logger.Info(permissions.ToString(), _oType.FullName, "OnProviderDisconnect");
            }
            try
            {
                ProviderPermissionDictionary.Remove(permissions.MarketDataProvider);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _oType.FullName, "OnProviderDisconnect");
            }
        }

        /// <summary>
        /// When New symbol is Subscribed
        /// Saving its Permissions in a dictionary
        /// </summary>
        /// <param name="securityPermissions"></param>
        public void OnSymbolSubscribed(SecurityPermissions securityPermissions)
        {
            if (Logger.IsDebugEnabled)
            {
                Logger.Info(securityPermissions.ToString(), _oType.FullName, "OnSymbolSubscribed");
            }
            try
            {
                SecurityPermissionDictionary.Add(securityPermissions.Security.Symbol, securityPermissions);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _oType.FullName, "OnSymbolSubscribed");
            }
        }

        /// <summary>
        /// When symbol is unSubscribed
        /// Removing its Permissions in a dictionary
        /// </summary>
        public void OnSymbolUnSubscribed(Security security)
        {
            if (Logger.IsDebugEnabled)
            {
                Logger.Info(security.ToString(), _oType.FullName, "OnSymbolUnSubscribed");
            }
            try
            {
                SecurityPermissionDictionary.Remove(security.Symbol);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _oType.FullName, "OnSymbolUnSubscribed");
            }
        }

        /// <summary>
        /// Overwrites Previous Writer Permission
        /// </summary>
        /// <param name="providerPermission"></param>
        public void ChangeProviderPermissions(ProviderPermission providerPermission)
        {
            try
            {
                ProviderPermission currentPermission;

                // Update exsiting permissions if they exist
                if (ProviderPermissionDictionary.TryGetValue(providerPermission.MarketDataProvider, out currentPermission))
                {
                    ProviderPermissionDictionary[providerPermission.MarketDataProvider] = providerPermission;
                }
                else
                {
                    // Add permissions to the local map
                    ProviderPermissionDictionary.Add(providerPermission.MarketDataProvider, providerPermission);
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _oType.FullName, "ChangeProviderPermissions");
            }
        }

        /// <summary>
        /// Change Tick Accorind to Permissions
        /// </summary>
        /// <param name="tick"></param>
        /// <param name="writeQuote"></param>
        /// <param name="writeTrade"></param>
        /// <returns></returns>
        private Tick ChangeTickAccordingToPermissions(Tick tick, bool writeQuote, bool writeTrade)
        {
            try
            {
                Tick newTick = (Tick) tick.Clone();
                if(!writeQuote)
                {
                    newTick.AskPrice = newTick.AskSize = newTick.BidPrice = newTick.BidSize = 0M;
                }
                if (!writeTrade)
                {
                    newTick.LastPrice = newTick.LastSize = 0M;
                }
                return newTick;
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _oType.FullName, "ChangeTickAccordingToPermissions");
                return tick;
            }
        }

        /// <summary>
        /// Chnage Permission of Symbol 
        /// </summary>
        /// <param name="securityPermissions"></param>
        public void ChangeSecurityPermission(SecurityPermissions securityPermissions)
        {
            try
            {
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info(securityPermissions.ToString(), _oType.FullName, "ChangeSecurityPermission");
                }
                if(SecurityPermissionDictionary[securityPermissions.Security.Symbol]!=null)
                {
                    SecurityPermissionDictionary[securityPermissions.Security.Symbol] = securityPermissions;
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _oType.FullName, "ChangeSecurityPermission");
            }
        }

        /// <summary>
        /// Saves Historic Bar data to Medium
        /// </summary>
        public void SaveHistoricBarData(HistoricBarData obj)
        {
            try
            {
                if (Logger.IsDebugEnabled)
                {
                    Logger.Info(obj.ToString(), _oType.FullName, "SaveHistoricBarData");
                }
                _writerCsv.Write(obj);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _oType.FullName, "SaveHistoricBarData");
            }
        }

        /// <summary>
        /// Addes New bar Request to List
        /// </summary>
        /// <param name="barDataRequest"></param>
        public void NewBarRequestArrived(BarDataRequest barDataRequest)
        {
            try
            {
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("Added Bar Request Message " + barDataRequest, _oType.FullName, "NewBarRequestArrived");
                }
                // Check to make sure that an Bar Request with the same id dont exist
                if (!BarDataRequests.Any(x => x.Id==barDataRequest.Id))
                {
                    BarDataRequests.Add(barDataRequest);
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _oType.FullName, "NewBarRequestArrived");
            }
            
        }

        /// <summary>
        /// Removes the Request from List
        /// </summary>
        /// <param name="barDataRequest"></param>
        public void RemoveBarRequest(BarDataRequest barDataRequest)
        {
            try
            {
                // Findes the BarDataRequest of the same id and removes it from the list.
                var request = BarDataRequests.Single(x => x.Id == barDataRequest.Id);
                BarDataRequests.Remove(request);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _oType.FullName, "RemoveBarRequest");
            }
        }

        /// <summary>
        /// Takes an bar Object and maps it onto DetailBar Object
        /// </summary>
        /// <param name="bar"></param>
        /// <returns></returns>
        public DetailBar ChangeBarToDetailBar(Bar bar)
        {
            try
            {
                DetailBar detailBar=new DetailBar(bar);
                var request=BarDataRequests.Single(x => x.Id == detailBar.RequestId);
                detailBar.PipSize = request.PipSize;
                detailBar.BarFormat = request.BarFormat;
                detailBar.BarLength = request.BarLength;
                detailBar.BarPriceType = request.BarPriceType;
                return detailBar;
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _oType.FullName, "ChangeBarToDetailBar");
                return null;
            }
        }

    }
}