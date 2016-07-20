using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TraceSourceLogger;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.DomainModels.OrderDomain;
using TradeHub.TradeManager.Server.Domain;

namespace TradeHub.TradeManager.Server.Service
{
    /// <summary>
    /// Processes incoming Execution messages to be handled by respective Trade Factory <see cref="TradeProcessor"/> 
    /// </summary>
    public class ExecutionHandler : IDisposable
    {
        private Type _type = typeof (ExecutionHandler);

        /// <summary>
        /// Contains all active Trade Factory objects
        /// KEY = Order Execution Provider
        /// VALUE = Dictionary: KEY = Security | VALUE = Trade Processor Object <see cref="TradeProcessor"/>
        /// </summary>
        private Dictionary<string, Dictionary<Security, TradeProcessor>> _tradeProcessorMap;

        /// <summary>
        /// Contains all active Trade Processor objects
        /// KEY = Order Execution Provider
        /// VALUE = Trade Factory Object <see cref="TradeProcessor"/>
        /// </summary>
        public IReadOnlyDictionary<string, Dictionary<Security, TradeProcessor>> TradeProcessorMap
        {
            get { return _tradeProcessorMap; }
        }

        /// <summary>
        /// Default Constructor
        /// </summary>
        public ExecutionHandler()
        {
            // Initialize
            _tradeProcessorMap = new Dictionary<string, Dictionary<Security, TradeProcessor>>();
        }

        /// <summary>
        /// Called when new Execution is received
        /// </summary>
        /// <param name="execution">Order Execution Object</param>
        public void NewExecutionArrived(Execution execution)
        {
            try
            {
                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug("New Execution received " + execution, _type.FullName, "NewExecutionArrived");
                }

                // Create Object
                Dictionary<Security, TradeProcessor> tradeProcessorsBySecurityMap;

                // Get Trade Processor object for the received Execution Message's Provider
                if (!_tradeProcessorMap.TryGetValue(execution.OrderExecutionProvider, out tradeProcessorsBySecurityMap))
                {
                    if (Logger.IsDebugEnabled)
                    {
                        Logger.Debug("New Trade Processor created for:" + execution.OrderExecutionProvider, _type.FullName, "NewExecutionArrived");
                    }

                    // Initialize a new Trade Processor object
                    TradeProcessor tradeProcessorObject = new TradeProcessor(execution.Fill.Security, execution.OrderExecutionProvider);

                    // Initialize local Map
                    tradeProcessorsBySecurityMap = new Dictionary<Security, TradeProcessor>();

                    // Add to local Map
                    tradeProcessorsBySecurityMap.Add(execution.Fill.Security, tradeProcessorObject);

                    // Add to Global Map
                    _tradeProcessorMap.Add(execution.OrderExecutionProvider, tradeProcessorsBySecurityMap);

                    // Forward Execution to Trade Processor
                    tradeProcessorObject.NewExecutionArrived(execution);
                }
                else
                {
                    TradeProcessor tradeProcessorObject;

                    // Get Trade Processor object for the received Execution Message's Security
                    if (!tradeProcessorsBySecurityMap.TryGetValue(execution.Fill.Security, out tradeProcessorObject))
                    {
                        if (Logger.IsDebugEnabled)
                        {
                            Logger.Debug("New Trade Processor created for:" + execution.Fill.Security, _type.FullName, "NewExecutionArrived");
                        }

                        // Initialize a new Trade Processor object
                        tradeProcessorObject = new TradeProcessor(execution.Fill.Security, execution.OrderExecutionProvider);

                        // Add to local Map
                        tradeProcessorsBySecurityMap.Add(execution.Fill.Security, tradeProcessorObject);
                    }
                    // Forward Execution to Trade Processor
                    tradeProcessorObject.NewExecutionArrived(execution);
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "NewExecutionArrived");
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _tradeProcessorMap.Clear();
            }
        }
    }
}
