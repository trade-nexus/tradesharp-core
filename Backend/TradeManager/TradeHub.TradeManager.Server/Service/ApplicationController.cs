using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Spring.Context.Support;
using TraceSourceLogger;
using TradeHub.Common.Core.DomainModels.OrderDomain;
using TradeHub.Common.Core.Repositories;
using TradeHub.Common.Persistence;
using TradeHub.TradeManager.CommunicationManager.Service;

namespace TradeHub.TradeManager.Server.Service
{
    public class ApplicationController
    {
        private Type _type = typeof (ApplicationController);

        /// <summary>
        /// Provides communication medium between Server and Client
        /// </summary>
        private ICommunicator _communicator;

        /// <summary>
        /// Manages Execution messages to be processed for Trade Information
        /// </summary>
        private ExecutionHandler _executionHandler;

        /// <summary>
        /// Argument Constructor
        /// </summary>
        /// <param name="communicator">Provides communication medium between Server and Client</param>
        /// <param name="executionHandler"></param>
        public ApplicationController(ICommunicator communicator, ExecutionHandler executionHandler)
        {
            // Save Instance
            _communicator = communicator;
            _executionHandler = executionHandler;

            // Subscribe Events
            RegisterCommunicatorEvents();
        }

        /// <summary>
        /// Starts Communicator to open communication medium with clients
        /// </summary>
        public void StartCommunicator()
        {
            // Check for Null Reference
            if (_communicator != null)
            {
                // Connect Communication Server
                _communicator.Connect();

                IPersistRepository<object> persistRepository = ContextRegistry.GetContext()["PersistRepository"] as IPersistRepository<object>;
                PersistencePublisher.InitializeDisruptor(true, persistRepository);
            }
        }

        /// <summary>
        /// Stop Communicator to close communication medium
        /// </summary>
        public void StopCommunicator()
        {
            // Check for Null Reference
            if (_communicator != null)
            {
                // Check if the Communicator is currently active
                if (_communicator.IsConnected())
                {
                    // Stop Communication Server
                    _communicator.Disconnect();
                }
            }
        }

        /// <summary>
        /// Subscribe Events from Communicator Instance
        /// </summary>
        private void RegisterCommunicatorEvents()
        {
            // Check for Null Reference
            if (_communicator != null)
            {
                // Makes sure that events are not hooked multiple time
                UnregisterCommunicatorEvents();

                // Register event to receive new Execution messages
                _communicator.NewExecutionReceivedEvent += OnNewExecutionReceivedEvent;
            }
        }

        /// <summary>
        /// Un-Subscribe Events from Communicator Instance
        /// </summary>
        private void UnregisterCommunicatorEvents()
        {
            // Unsubscrive event to stop receiving Execution messages
            _communicator.NewExecutionReceivedEvent -= OnNewExecutionReceivedEvent;
        }

        /// <summary>
        /// Called when new execution message is received from Communicator Server
        /// </summary>
        /// <param name="execution">Execution containing Order and Fill information</param>
        private void OnNewExecutionReceivedEvent(Execution execution)
        {
            try
            {
                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug("New Execution received " + execution, _type.FullName, "OnNewExecutionReceivedEvent");
                }

                // Forward Execution details to Execution Handler
                _executionHandler.NewExecutionArrived(execution);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "OnNewExecutionReceivedEvent");
            }
        }
    }
}
