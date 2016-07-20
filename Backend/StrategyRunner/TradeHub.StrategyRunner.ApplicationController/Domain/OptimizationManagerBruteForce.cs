using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using TraceSourceLogger;
using TradeHub.Common.Core.Utility;
using TradeHub.StrategyRunner.ApplicationController.Service;
using TradeHub.StrategyRunner.Infrastructure.Entities;
using TradeHub.StrategyRunner.Infrastructure.Service;
using TradeHub.StrategyRunner.Infrastructure.ValueObjects;
using TradeHub.StrategyRunner.UserInterface.Common;

namespace TradeHub.StrategyRunner.ApplicationController.Domain
{
    public class OptimizationManagerBruteForce
    {
        private Type _type = typeof (OptimizationManagerBruteForce);
        private AsyncClassLogger _asyncClassLogger;

        /// <summary>
        /// Contains ctor arguments to be used for multiple iteration
        /// </summary>
        private List<object[]> _ctorArguments;

        /// <summary>
        /// Keeps tracks of all the running strategies
        /// KEY = Unique string to identify each strategy instance
        /// Value = <see cref="StrategyExecutor"/>
        /// </summary>
        private ConcurrentDictionary<string, StrategyExecutor> _strategiesCollection;

        /// <summary>
        /// Save constuctor parameter info for the selected strategy
        /// </summary>
        private System.Reflection.ParameterInfo[] _parmatersDetails;

        /// <summary>
        /// Contains ctor arguments to be used for multiple iteration
        /// </summary>
        public List<object[]> CtorArguments
        {
            get { return _ctorArguments; }
            set { _ctorArguments = value; }
        }

        /// <summary>
        /// Default Argument
        /// </summary>
        public OptimizationManagerBruteForce()
        {
            //_asyncClassLogger = ContextRegistry.GetContext()["StrategyRunnerLogger"] as AsyncClassLogger;
            _asyncClassLogger = new AsyncClassLogger("OptimizationManagerBruteForce");

            // Initialize
            _ctorArguments = new List<object[]>();
            _strategiesCollection = new ConcurrentDictionary<string, StrategyExecutor>();

            // Subscribe to event aggregator
            EventSystem.Subscribe<OptimizeStrategyBruteForce>(StartOptimization);
        }

        /// <summary>
        /// Start Strategy Optimization
        /// </summary>
        /// <param name="optimizationParameters">Contains info for the parameters to be used for optimization</param>
        private void StartOptimization(OptimizeStrategyBruteForce optimizationParameters)
        {
            try
            {
                if (_asyncClassLogger.IsInfoEnabled)
                {
                    _asyncClassLogger.Info("Getting argument combinations", _type.FullName, "StartOptimization");
                }

                // Clear all previous information
                _strategiesCollection.Clear();

                // Save Parameter Details
                _parmatersDetails = optimizationParameters.ParmatersDetails;

                // Get all ctor arguments to be used for optimization
                CreateCtorCombinations(optimizationParameters.CtorArgs, optimizationParameters.ConditionalParameters);

                // Initialize Stratgey for each set of arguments
                foreach (object[] ctorArgument in _ctorArguments)
                {
                    // Get new Key.
                    string key = ApplicationIdGenerator.NextId();

                    // Save Strategy details in new Strategy Executor object
                    StrategyExecutor strategyExecutor = new StrategyExecutor(key, optimizationParameters.StrategyType, ctorArgument);

                    // Register Event
                    strategyExecutor.StatusChanged += OnStrategyExecutorStatusChanged;

                    // Add to local map
                    _strategiesCollection.AddOrUpdate(key, strategyExecutor, (ky, value) => strategyExecutor);
                }

                // Start executing each instance
                StartStrategyExecution();
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "StartOptimization");
            }
        }

        /// <summary>
        /// Strats executing individual strategy instances created for each iteration
        /// </summary>
        [MethodImpl(MethodImplOptions.Synchronized)] 
        private void StartStrategyExecution()
        {
            try
            {
                if (_asyncClassLogger.IsDebugEnabled)
                {
                    _asyncClassLogger.Debug("Starting strategy instance for optimization.", _type.FullName, "StratStrategyExecution");
                }

                if (_strategiesCollection.Count > 0)
                {
                    // Get the iteration to be executed;
                    var strategyExecutor = _strategiesCollection.ElementAt(0).Value;

                    // Execute strategy if its not already executing/executed
                    if (strategyExecutor.StrategyStatus.Equals(Infrastructure.Constants.StrategyStatus.None))
                    {
                        strategyExecutor.ExecuteStrategy();
                    }
                }
                // Execute each instance on a separate thread
                // Parallel.ForEach(_strategiesCollection.Values,
                //                strategyExecutor => Task.Factory.StartNew(strategyExecutor.ExecuteStrategy));
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "StratStrategyExecution");
            }
        }

        /// <summary>
        /// Creates all possible ctor combinations
        /// </summary>
        /// <param name="ctorArgs">ctor arguments to create combinations with</param>
        /// <param name="conditionalParameters">contains info for the conditional parameters</param>
        public void CreateCtorCombinations(object[] ctorArgs, Tuple<int, string, string>[] conditionalParameters)
        {
            try
            {
                var itemsCount = conditionalParameters.Length;
                // Get all posible optimizations
                GetAllIterations(ctorArgs.Clone() as object[], conditionalParameters, itemsCount - 1);
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "CreateCtorCombinations");
            }
        }

        /// <summary>
        /// Gets all possible combinations for the given parameters
        /// </summary>
        /// <param name="args">ctor arguments to create combinations with</param>
        /// <param name="conditionalParameters">contains info for the conditional parameters</param>
        /// <param name="conditionalIndex">index of conditional parameter to be used for iterations</param>
        private void GetAllIterations(object[] args, Tuple<int,string,string>[] conditionalParameters, int conditionalIndex)
        {
            try
            {
                // get index of parameter to be incremented
                int index = conditionalParameters[conditionalIndex].Item1;

                // Get end value for the parameter
                decimal endPoint;
                if (! decimal.TryParse(conditionalParameters[conditionalIndex].Item2, out endPoint))
                {
                    return;
                }

                // Get increment value to be used 
                decimal increment;
                if (!decimal.TryParse(conditionalParameters[conditionalIndex].Item3, out increment))
                {
                    return;
                }

                // Get Orignal Value
                decimal orignalValue = Convert.ToDecimal(args[index]);

                // Iterate through all combinations
                for (decimal i = 0; ; i += increment)
                {
                    // Modify parameter value
                    var parameter = orignalValue + i;

                    if (parameter > endPoint) break;

                    // Convert string value to required format
                    var value = LoadCustomStrategy.GetParametereValue(parameter.ToString(), _parmatersDetails[index].ParameterType.Name);

                    // Update arguments array
                    args[index] = value;

                    // Check if the combination is already present
                    if (!ValueAdded(args, _ctorArguments, index))
                    {
                        // Add the updated arguments to local map
                        _ctorArguments.Add(args.Clone() as object[]);

                        // Get further iterations if 
                        if (conditionalIndex > 0)
                        {
                            GetAllIterations(args.Clone() as object[], conditionalParameters, conditionalIndex - 1);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "IterateParameters");
            }
        }

        /// <summary>
        /// Called when there is a change in strategy status
        /// </summary>
        /// <param name="status">indicates whether the strategy is running or stopped</param>
        /// <param name="key">Unique Key to identify the Strategy</param>
        [MethodImpl(MethodImplOptions.Synchronized)] 
        private void OnStrategyExecutorStatusChanged(bool status, string key)
        {
            try
            {
                if (_asyncClassLogger.IsInfoEnabled)
                {
                    _asyncClassLogger.Info("Strategy: " + key + " is running: " + status, _type.FullName, "OnStrategyExecutorStatusChanged");
                }
                
                // Get Statistics if the strategy is completed
                if (!status)
                {
                    StrategyExecutor strategyExecutor;
                    if (_strategiesCollection.TryRemove(key, out strategyExecutor))
                    {
                        StringBuilder parametersInfo = new StringBuilder();
                        
                        foreach (object ctorArgument in strategyExecutor.CtorArguments)
                        {
                            parametersInfo.Append(ctorArgument.ToString());
                            parametersInfo.Append(" | ");
                        }

                        // Create new object to be used with Event Aggregator
                        var optimizationStatistics =
                            new OptimizationStatistics(strategyExecutor.Statistics, parametersInfo.ToString());

                        // Unhook Event
                        strategyExecutor.StatusChanged -= OnStrategyExecutorStatusChanged;

                        // Stop Strategy
                        strategyExecutor.StopStrategy();
                        EventSystem.Publish<OptimizationStatistics>(optimizationStatistics);

                        // Close all connections
                        strategyExecutor.Close();

                        // Publish event to notify listeners
                        EventSystem.Publish<OptimizationStatistics>(optimizationStatistics);

                        // Execute next iteration
                        StartStrategyExecution();
                    }
                }
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "OnStrategyExecutorStatusChanged");
            }
        }

        /// <summary>
        /// Checks if the value is already added in given list
        /// </summary>
        /// <param name="newValue">Value to verfiy</param>
        /// <param name="localMap">Local map to check for given value</param>
        /// <param name="index">Index on which to verify the value</param>
        private bool ValueAdded(object[] newValue, List<object[]> localMap, int index)
        {
            if (localMap.Count > 0)
            {
                var lastElement = localMap.Last();
                if (lastElement != null)
                {
                    if (lastElement[index].Equals(newValue[index]))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
