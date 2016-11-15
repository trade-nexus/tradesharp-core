/***************************************************************************** 
* Copyright 2016 Aurora Solutions 
* 
*    http://www.aurorasolutions.io 
* 
* Aurora Solutions is an innovative services and product company at 
* the forefront of the software industry, with processes and practices 
* involving Domain Driven Design(DDD), Agile methodologies to build 
* scalable, secure, reliable and high performance products.
* 
* TradeSharp is a C# based data feed and broker neutral Algorithmic 
* Trading Platform that lets trading firms or individuals automate 
* any rules based trading strategies in stocks, forex and ETFs. 
* TradeSharp allows users to connect to providers like Tradier Brokerage, 
* IQFeed, FXCM, Blackwood, Forexware, Integral, HotSpot, Currenex, 
* Interactive Brokers and more. 
* Key features: Place and Manage Orders, Risk Management, 
* Generate Customized Reports etc 
* 
* Licensed under the Apache License, Version 2.0 (the "License"); 
* you may not use this file except in compliance with the License. 
* You may obtain a copy of the License at 
* 
*    http://www.apache.org/licenses/LICENSE-2.0 
* 
* Unless required by applicable law or agreed to in writing, software 
* distributed under the License is distributed on an "AS IS" BASIS, 
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
* See the License for the specific language governing permissions and 
* limitations under the License. 
*****************************************************************************/


﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using AForge;
using AForge.Genetic;
using AForge.Math.Random;
using Optimera.GA;
using TraceSourceLogger;
using TradeHub.Optimization.Genetic.FitnessFunction;
using TradeHub.Optimization.Genetic.Tests.Application.HelperFunctions;
using TradeHub.Optimization.Genetic.Tests.Application.Utility;
using TradeHub.StrategyEngine.Utlility.Services;
using Parallel = System.Threading.Tasks.Parallel;

namespace TradeHub.Optimization.Genetic.Tests.Application
{
    public class OptimizationManager
    {
        private string _assemblyName = @"\StockTrader.Common.dll";
        private string _assemblyFolder = @"C:\Users\Taimoor\Desktop\GA Optimization Assemblies\StockTraderStrategy - ";
        private string _assemblyPath = @"StockTraderStrategy\StockTrader.Common.dll";

        private object[] _ctorArguments;
        private object[][] _ctorArgumentsArray;
        
        private Type _strategyType;
        private Type[] _strategyTypeArray;

        // AForge Fitness Elements
        private Population _population;
        private Population[] _populationArray;
        private AForgeFitnessFunction _fitnessFunction;
        private AForgeFitnessFunction[] _fitnessFunctionArray;
        
        // Optimera Fitness Elements
        private GA _optimeraAlgorithm;
        private OptimeraFitnessFunction _optimeraFitness;

        // Brute Force Elements
        private BruteForceOptimization _bruteForce;

        // Mathimatical Fitness function elements
        private Population _mathimaticalPopulation;
        private MathimaticalFitnessFunction _mathimaticalFitness;
        private MathimaticalFitnessFunction[] _mathimaticalFitnessArray;

        // Handles TradeHub strategy execution for single instance
        private TestStrategyExecutor _strategyExecutor;
        // Handles TradeHub strategy execution for multiple instances
        private TestStrategyExecutor[] _strategyExecutorArray;

        /// <summary>
        /// Default Constructor
        /// </summary>
        public OptimizationManager()
        {
            // Set Logging levels
            Logger.SetLoggingLevel();

            // Initialize array
            _ctorArgumentsArray = new object[10][];
            _strategyTypeArray = new Type[10];
            _populationArray = new Population[10];
            _fitnessFunctionArray= new AForgeFitnessFunction[10];
            _strategyExecutorArray = new TestStrategyExecutor[10];

            // Create Constructor Arguments
            _ctorArguments = new object[]
                {
                    // Chelen Len,   ALPHA    , Shares   , Symbol,   EMA      ,   GAMMA    ,  EPSILON 
                    (Int32) 100, (decimal) 1, (uint) 40, "ERX", (decimal) 45, (float) 0.2, (decimal) 0.002,
                    
                    // Profit Take,  Tolerance,  StartTime, EndTime, HTB Thresh, OPG Thresh   , OPG Venue,   BETA
                    (float) 0.005, (decimal) 0.005, "9:30", "9:30", (decimal) 10, (decimal) 0.04, "SDOT", (float) 0.001,

                    // Entry Slippage, Exit Slippage
                    (decimal) 0.01, (decimal) 0.01,
                    "SimulatedExchange", "SimulatedExchange"
                };

            LoadAssembly(_assemblyPath);

            //if (_strategyType != null)
            {
                Stopwatch sc=new Stopwatch();
                sc.Start();
                //// Initialize executor for single thread execution
                _strategyExecutor = new TestStrategyExecutor(_strategyType, _ctorArguments);
                sc.Stop();
                Logger.Info("Strategy Executor time=" + sc.ElapsedMilliseconds + " ms", "", "InitializeOptimizationParameters");


                Logger.Info("Initializing Strategy Executor array.", "OptimizationManager", "OptimizationManager");

                //// Initialize executor array for parallel execution
                //for (int i = 0; i < 10; i++)
                //{
                //    _strategyExecutorArray[i] =
                //        new TestStrategyExecutor(_strategyTypeArray[i], new object[]
                //            {
                //                // Chelen Len,   ALPHA    , Shares   , Symbol,   EMA      ,   GAMMA    ,  EPSILON 
                //                (Int32) 100, (decimal) 1, (uint) 40, "MSFT", (decimal) 45, (float) 0.2,
                //                (decimal) 0.002,

                //                // Profit Take,  Tolerance,  StartTime, EndTime, HTB Thresh, OPG Thresh   , OPG Venue,   BETA
                //                (float) 0.005, (decimal) 0.005, "9:30", "9:30", (decimal) 10, (decimal) 0.04,
                //                "SDOT", (float) 0.001,

                //                // Entry Slippage, Exit Slippage
                //                (decimal) 0.01, (decimal) 0.01,
                //                "SimulatedExchange", "SimulatedExchange"
                //            });
                //}
                
                //Task[] taskArrayStrategyExecutor = new Task[10];

                //for (int taskNumber = 0; taskNumber < 10; taskNumber++)
                //{
                //    // capturing taskNumber in lambda wouldn't work correctly
                //    int taskNumberCopy = taskNumber;

                //    taskArrayStrategyExecutor[taskNumber] = Task.Factory.StartNew(
                //        () =>
                //            {
                //                _strategyExecutorArray[taskNumberCopy] =
                //                    new TestStrategyExecutor(_strategyTypeArray[taskNumberCopy], new object[]
                //                        {
                //                            // Chelen Len,   ALPHA    , Shares   , Symbol,   EMA      ,   GAMMA    ,  EPSILON 
                //                            (Int32) 100, (decimal) 1, (uint) 40, "ERX", (decimal) 45, (float) 0.2,
                //                            (decimal) 0.002,

                //                            // Profit Take,  Tolerance,  StartTime, EndTime, HTB Thresh, OPG Thresh   , OPG Venue,   BETA
                //                            (float) 0.005, (decimal) 0.005, "9:30", "9:30", (decimal) 10, (decimal) 0.04
                //                            ,
                //                            "SDOT", (float) 0.001,

                //                            // Entry Slippage, Exit Slippage
                //                            (decimal) 0.01, (decimal) 0.01,
                //                            "SimulatedExchange", "SimulatedExchange"
                //                        });
                //            });
                //}

                //Task.WaitAll(taskArrayStrategyExecutor);

                //Parallel.For(0, 10,
                //             increment =>
                //             _strategyExecutorArray[increment] =
                //             new TestStrategyExecutor(_strategyTypeArray[increment], new object[]
                //                 {
                //                     // Chelen Len,   ALPHA    , Shares   , Symbol,   EMA      ,   GAMMA    ,  EPSILON 
                //                     (Int32) 100, (decimal) 1, (uint) 40, "ERX", (decimal) 45, (float) 0.2,
                //                     (decimal) 0.002,
                    
                //                     // Profit Take,  Tolerance,  StartTime, EndTime, HTB Thresh, OPG Thresh   , OPG Venue,   BETA
                //                     (float) 0.005, (decimal) 0.005, "9:30", "9:30", (decimal) 10, (decimal) 0.04,
                //                     "SDOT", (float) 0.001,

                //                     // Entry Slippage, Exit Slippage
                //                     (decimal) 0.01, (decimal) 0.01,
                //                     "SimulatedExchange", "SimulatedExchange"
                //                 }));

                Logger.Info("Initialization of Strategy Executor complete.", "OptimizationManager", "OptimizationManager");
            }
        }

        #region AForge Optimization

        /// <summary>
        /// Initializes Optimization parameters and fields to be used for AForge Optimziation Library
        /// </summary>
        private void InitializeAForgeOptimizationParameters()
        {
            Range[] alphaRangeArray = new Range[10];
            Range[] betaRangeArray = new Range[10];
            Range[] gammaRangeArray = new Range[10];
            Range[] epsilonRangeArray = new Range[10];

            #region Populate Range Array

            for (int i = 0; i < 10; i++)
            {
                alphaRangeArray[i] = new Range(float.Parse(ConvertInputToValidRangeValues(1, 0.1).ToString()),
                                             float.Parse(ConvertInputToValidRangeValues(5, 0.1).ToString()));

                betaRangeArray[i] =
                    new Range(float.Parse(ConvertInputToValidRangeValues(0.0001, 0.0001).ToString()),
                              float.Parse(ConvertInputToValidRangeValues(0.011, 0.0001).ToString()));

                gammaRangeArray[i] = new Range(float.Parse(ConvertInputToValidRangeValues(0.2, 0.1).ToString()),
                                             float.Parse(ConvertInputToValidRangeValues(10, 0.1).ToString()));

                epsilonRangeArray[i] =
                    new Range(float.Parse(ConvertInputToValidRangeValues(0.002, 0.001).ToString()),
                              float.Parse(ConvertInputToValidRangeValues(0.010, 0.001).ToString()));
            }

            #endregion

            // Initialize Fitness function to be used
            Stopwatch sc=new Stopwatch();
            sc.Start();
            _fitnessFunction = new AForgeFitnessFunction(_strategyExecutor, new Range(1,5), new Range(0.0001f,0.011f), 
                                                         new Range(0.2f,10), new Range(0.002f,0.01f));
            sc.Stop();
            Logger.Info("Fitness Function time=" + sc.ElapsedMilliseconds + " ms", "", "InitializeOptimizationParameters");
            sc.Reset();

            //_fitnessFunction = new AForgeFitnessFunction(_strategyExecutor, new Range(1, 5), new Range(0.0001f, 0.011f),
            //                                             new Range(0.2f, 10), new Range(0.002f, 0.0105f));

            // Set Fitness Mode
            _fitnessFunction.Mode = OptimizationFunction4D.Modes.Maximization;

            // Population size to be used
            const int populationSize = 45;

            // Initialize Fitness Function array
            //for (int i = 0; i < 10; i++)
            //{
            //    _fitnessFunctionArray[i] = new AForgeFitnessFunction(_strategyExecutorArray[i], alphaRangeArray[i],
            //                                                         betaRangeArray[i], gammaRangeArray[i],
            //                                                         epsilonRangeArray[i]);

            //    //// Set Fitness Mode
            //    //_fitnessFunctionArray[i].Mode = OptimizationFunction4D.Modes.Maximization;
            //}

            Logger.Info("Initializing Fitness array.", "OptimizationManager", "InitializeAForgeOptimizationParameters");

            //Task[] taskArrayFitnessFunction = new Task[10];

            //for (int taskNumber = 0; taskNumber < 10; taskNumber++)
            //{
            //    // capturing taskNumber in lambda wouldn't work correctly
            //    int taskNumberCopy = taskNumber;

            //    taskArrayFitnessFunction[taskNumber] = Task.Factory.StartNew(
            //        () =>
            //            {
            //                _fitnessFunctionArray[taskNumberCopy] = new AForgeFitnessFunction(_strategyExecutorArray[taskNumberCopy], alphaRangeArray[taskNumberCopy],
            //                                                        betaRangeArray[taskNumberCopy], gammaRangeArray[taskNumberCopy],
            //                                                        epsilonRangeArray[taskNumberCopy]);
            //            });
            //}

            //Task.WaitAll(taskArrayFitnessFunction);

            Logger.Info("Initialization complete.", "OptimizationManager", "InitializeAForgeOptimizationParameters");
            Logger.Info("Initializing Population array.", "OptimizationManager", "InitializeAForgeOptimizationParameters");

            sc.Start();
            //Create genetic population
            _population = new Population(populationSize, new SimpleStockTraderChromosome(new Range[]{new Range(1,5), new Range(0.0001f,0.011f), 
                                                         new Range(0.2f,10), new Range(0.002f,0.01f)}), _fitnessFunction, new EliteSelection());
            sc.Stop();
            Logger.Info("Population time=" + sc.ElapsedMilliseconds + " ms", "", "InitializeOptimizationParameters");

            //// Create genetic population array using 'AForge.Parallel'
            //AForge.Parallel.For(0, 10, delegate(int iteration)
            //    {
            //        _populationArray[iteration] =
            //            new Population(45, new BinaryChromosome(32), _fitnessFunctionArray[iteration],
            //                           new EliteSelection());
            //    });

            //for (int iteration = 0; iteration < 10; iteration++)
            //{
            //    _populationArray[iteration] =
            //            new Population(populationSize, new BinaryChromosome(32), _fitnessFunctionArray[iteration],
            //                           new EliteSelection());
            //}

            //// Create genetic population array using 'System.Threading.Parallel'
            //Parallel.For(0, 10, new ParallelOptions { MaxDegreeOfParallelism = 10 },
            //             iteration =>
            //             _populationArray[iteration] =
            //             new Population(45, new BinaryChromosome(32), _fitnessFunctionArray[iteration],
            //                            new EliteSelection()));
            
            //// Create genetic population array using 'Tests.Application.HelperFunction'
            //ParallelForLoop.ContiguousParallelFor(0, 10, (Action<int>)delegate(int iteration)
            //{
            //    _populationArray[iteration] =
            //            new Population(populationSize, new BinaryChromosome(32), _fitnessFunctionArray[iteration],
            //                           new EliteSelection());
            //}, 10);

            //Task[] taskArray = new Task[10];

            //for (int taskNumber = 0; taskNumber < 10; taskNumber++)
            //{
            //    // capturing taskNumber in lambda wouldn't work correctly
            //    int taskNumberCopy = taskNumber;

            //    taskArray[taskNumber] = Task.Factory.StartNew(
            //        () =>
            //        {
            //            _populationArray[taskNumberCopy] =
            //                new Population(45, new BinaryChromosome(32), _fitnessFunctionArray[taskNumberCopy],
            //                               new EliteSelection());
            //        });
            //}

            //Task.WaitAll(taskArray);

            Logger.Info("Initialization of Population array complete.", "OptimizationManager", "InitializeAForgeOptimizationParameters");

            //throw new System.NullReferenceException("Force Close");
            //_population.CrossoverRate = 0.5;
        }

        /// <summary>
        /// Executes Strategy for optimization using AForge Library
        /// </summary>
        public void ExecuteStrategyWithFourParametersAForge()
        {
            //if (_strategyType != null)
            {
                // Wait for the Strategy Instance to connect
                Thread.Sleep(5000);

                // Initialize required Optimization parameters
                InitializeAForgeOptimizationParameters();

                Logger.Info("Start Iterations:", "OptimizationManager", "ExecuteStrategyWithFourParametersAForge");
                Stopwatch sc=new Stopwatch();
                sc.Start();
                // Single thread execution
                for (int main = 0; main < 10; main++)
                {
                    for (int i = 0; i < 10; i++)
                    {
                        _population.RunEpoch();

                        Logger.Info("Iteration Count: " + i, "Optimization", "PopulationIterations");
                    }
                    Console.WriteLine();
                    Console.WriteLine(_fitnessFunction.TranslateGep(_population.BestChromosome)[0]);
                    Console.WriteLine(_fitnessFunction.TranslateGep(_population.BestChromosome)[1]);
                    Console.WriteLine(_fitnessFunction.TranslateGep(_population.BestChromosome)[2]);
                    Console.WriteLine(_fitnessFunction.TranslateGep(_population.BestChromosome)[3]);
                    Console.WriteLine("Fitness Val="+_population.FitnessMax);
                }
                sc.Stop();
                Logger.Info("Time="+sc.ElapsedMilliseconds+" ms", "OptimizationManager", "ExecuteStrategyWithFourParametersAForge");

                //Task[] taskArray = new Task[10];

                //// Parallel Executions
                //for (int mainIteration = 0; mainIteration < 3; mainIteration++)
                //{
                //    // Execute Iterations in parallel
                //    //Parallel.For(0, 10, executionIteration => _populationArray[executionIteration].RunEpoch());

                //    for (int taskNumber = 0; taskNumber < 10; taskNumber++)
                //    {
                //        // capturing taskNumber in lambda wouldn't work correctly
                //        int taskNumberCopy = taskNumber;

                //        taskArray[taskNumber] = Task.Factory.StartNew(
                //            () => _populationArray[taskNumberCopy].RunEpoch());
                //    }

                //    Task.WaitAll(taskArray);

                    //ParallelForLoop.ContiguousParallelFor(0, 10, (Action<int>) delegate(int executionIteration)
                    //    {
                    //        _populationArray[executionIteration].RunEpoch();
                    //    }, 10);

                    //for (int i = 0; i < 5; i++)
                    //{
                    //    _populationArray[i].Migrate(_populationArray[5 + i], 15, new RankSelection());
                    //}

                    // Migrate values between populations
                    //MigratePopulation(mainIteration);

                    //Logger.Info("Iteration Count: " + mainIteration, "OptimizationManager", "ExecuteStrategyWithFourParametersAForge");
                //}

                //// Print 
                Console.WriteLine(RangeCasting.ConvertValueToUserDefinedRange(_fitnessFunction.Translate(_population.BestChromosome)[0], 0.1));
                Console.WriteLine(RangeCasting.ConvertValueToUserDefinedRange(_fitnessFunction.Translate(_population.BestChromosome)[1], 0.01));
                Console.WriteLine(RangeCasting.ConvertValueToUserDefinedRange(_fitnessFunction.Translate(_population.BestChromosome)[2], 0.01));
                Console.WriteLine(RangeCasting.ConvertValueToUserDefinedRange(_fitnessFunction.Translate(_population.BestChromosome)[3], 0.1));
       
                // Log
                Logger.Info(RangeCasting.ConvertValueToUserDefinedRange(_fitnessFunction.Translate(_population.BestChromosome)[0], 0.1).ToString(), "Optimization", "BestChromosome");
                Logger.Info(RangeCasting.ConvertValueToUserDefinedRange(_fitnessFunction.Translate(_population.BestChromosome)[1], 0.0001).ToString(), "Optimization", "BestChromosome");
                Logger.Info(RangeCasting.ConvertValueToUserDefinedRange(_fitnessFunction.Translate(_population.BestChromosome)[2], 0.1).ToString(), "Optimization", "BestChromosome");
                Logger.Info(RangeCasting.ConvertValueToUserDefinedRange(_fitnessFunction.Translate(_population.BestChromosome)[3], 0.001).ToString(), "Optimization", "BestChromosome");

                //for (int i = 0; i < 10; i++)
                //{
                //    Logger.Info(RangeCasting.ConvertValueToUserDefinedRange(_fitnessFunctionArray[i].Translate(_populationArray[i].BestChromosome)[0], 0.1).ToString(), "OptimizationManager", "BestChromosome");
                //    Logger.Info(RangeCasting.ConvertValueToUserDefinedRange(_fitnessFunctionArray[i].Translate(_populationArray[i].BestChromosome)[1], 0.0001).ToString(), "OptimizationManager", "BestChromosome");
                //    Logger.Info(RangeCasting.ConvertValueToUserDefinedRange(_fitnessFunctionArray[i].Translate(_populationArray[i].BestChromosome)[2], 0.1).ToString(), "OptimizationManager", "BestChromosome");
                //    Logger.Info(RangeCasting.ConvertValueToUserDefinedRange(_fitnessFunctionArray[i].Translate(_populationArray[i].BestChromosome)[3], 0.001).ToString(), "OptimizationManager", "BestChromosome");
                //    Logger.Info(_populationArray[i].FitnessMax.ToString(), "OptimizationManager", "BestChromosome");
                //}
            }
        }

        #endregion

        #region Optimera Optimization

        /// <summary>
        /// Initializes Optimization parameters and fields to be used for Optimera Optimziation Library
        /// </summary>
        private void InitializeOptimeraOptimizationParameters()
        {
            _optimeraFitness = new OptimeraFitnessFunction(4, _strategyExecutor);

            _optimeraAlgorithm = new GA(_optimeraFitness, UpdateProgress, 1, 0.8, 0.05, 100, 10, 0.001);
        }

        /// <summary>
        /// Executes Strategy for Optimization using Optimera Library
        /// </summary>
        /// <param name="numberOfParameters"></param>
        public void ExecuteStrategyOptimera(int numberOfParameters)
        {
            if (_strategyType != null)
            {
                // Wait for the Strategy Instance to connect
                Thread.Sleep(5000);

                // Initialize required Optimization parameters
                InitializeOptimeraOptimizationParameters();

                // Start Algorithm
                _optimeraAlgorithm.Go();
            }
        }

        static void UpdateProgress(String[] s)
        {
            Console.WriteLine();
            Console.WriteLine("Timestamp: " + s[0]);
            Console.WriteLine("Generations complete: " + s[1]);
            Console.WriteLine("Model runs complete: " + s[2]);
            Console.WriteLine("Best fitness so far: " + s[3]);
            Console.WriteLine("Best genes so far: " + s[4]);
        }

        #endregion

        #region BruteForce Optimization

        /// <summary>
        /// Initializes Optimization parameters and fields to be used for Brute Force Optimization
        /// </summary>
        private void InitializeBruteForceParameters()
        {
            var tempDetails = StrategyHelper.GetConstructorDetails(_assemblyPath);
            _bruteForce = new BruteForceOptimization(tempDetails.Item2, _strategyExecutor);
        }

        /// <summary>
        /// Executes Strategy for optimization using Brute Force
        /// </summary>
        public void ExecuteBruteForceOptimization()
        {
            InitializeBruteForceParameters();

            if (_strategyType != null)
            {
                Thread.Sleep(5000);

                Tuple<int, string, string>[] parametersList = new Tuple<int, string, string>[4];

                // ALPHA
                parametersList[0] = new Tuple<int, string, string>(1, "3", "0.1"); // 1-3
                // BETA
                parametersList[1] = new Tuple<int, string, string>(14, "0.01", "0.001"); // 0.001-0.01 (~100)
                // GAMMA
                parametersList[2] = new Tuple<int, string, string>(5, "10", "0.1"); // 0.2-10 (98)
                // EPSILON
                parametersList[3] = new Tuple<int, string, string>(6, "0.010", "0.001"); // 0.002-0.010 (80)

                _bruteForce.CreateCtorCombinations(_ctorArguments, parametersList);

                _bruteForce.ExecuteIterations();
            }
        }

        #endregion

        #region Mathimatical Function Optimization

        /// <summary>
        /// Initializes Parameters required to optimize the given mathimatical funtion
        /// </summary>
        private void InitializeMathimaticalParameters()
        {
            Range wRange = new Range(float.Parse(RangeCasting.ConvertInputToValidRangeValues(1, 0.1).ToString()),
                                     float.Parse(RangeCasting.ConvertInputToValidRangeValues(10, 0.1).ToString()));

            Range xRange = new Range(float.Parse(RangeCasting.ConvertInputToValidRangeValues(1, 0.01).ToString()),
                                     float.Parse(RangeCasting.ConvertInputToValidRangeValues(10, 0.01).ToString()));

            Range yRange = new Range(float.Parse(RangeCasting.ConvertInputToValidRangeValues(1, 0.01).ToString()),
                                     float.Parse(RangeCasting.ConvertInputToValidRangeValues(10, 0.01).ToString()));

            Range zRange = new Range(float.Parse(RangeCasting.ConvertInputToValidRangeValues(1, 0.1).ToString()),
                                     float.Parse(RangeCasting.ConvertInputToValidRangeValues(10, 0.1).ToString()));

            //// Initialize Fitness function to be used
            //_mathimaticalFitness = new MathimaticalFitnessFunction(wRange, xRange, yRange, zRange);

            //// Set Fitness Mode
            //_mathimaticalFitness.Mode = OptimizationFunction4D.Modes.Maximization;

            //// Create genetic population
            //_mathimaticalPopulation = new Population(100, new BinaryChromosome(32), _mathimaticalFitness, new EliteSelection());

            _mathimaticalFitnessArray = new MathimaticalFitnessFunction[10];

            // Initialize Fitness Function array
            for (int i = 0; i < 10; i++)
            {
                _mathimaticalFitnessArray[i] = new MathimaticalFitnessFunction(wRange, xRange, yRange, zRange);

                //// Set Fitness Mode
                //_fitnessFunctionArray[i].Mode = OptimizationFunction4D.Modes.Maximization;
            }

            Logger.Info("Initializing Population array.", "OptimizationManager", "InitializeAForgeOptimizationParameters");

            //for (int i = 0; i < 10; i++)
            //{
            //    var population =
            //            new Population(100, new BinaryChromosome(32), _mathimaticalFitnessArray[i],
            //                           new EliteSelection());
            //}

            Task[] taskArray = new Task[10];

            for (int taskNumber = 0; taskNumber < 10; taskNumber++)
            {
                // capturing taskNumber in lambda wouldn't work correctly
                int taskNumberCopy = taskNumber;

                taskArray[taskNumber] = Task.Factory.StartNew(
                    () =>
                    {
                        var population =
                        new Population(100, new BinaryChromosome(32), _mathimaticalFitnessArray[taskNumberCopy],
                                       new EliteSelection());
                    });
            }

            Task.WaitAll(taskArray);

            Logger.Info("Initialization of Population array complete.", "OptimizationManager", "InitializeAForgeOptimizationParameters");

            throw new System.NullReferenceException("Force Close");
        }

        /// <summary>
        /// Optimizes the given Mathimatical function
        /// </summary>
        public void ExecuteMathimaticalOptimization()
        {
            // Initialize required Optimization parameters
            InitializeMathimaticalParameters();

            for (int i = 0; i < 50; i++)
            {
                _mathimaticalPopulation.RunEpoch();

                Logger.Info("Iteration Count: " + i, "Optimization", "ExecuteMathimaticalOptimization");
            }

            Console.WriteLine();
            Console.WriteLine("RESULT:");
            Console.WriteLine(RangeCasting.ConvertValueToUserDefinedRange(_mathimaticalFitness.Translate(_mathimaticalPopulation.BestChromosome)[0], 0.1));
            Console.WriteLine(RangeCasting.ConvertValueToUserDefinedRange(_mathimaticalFitness.Translate(_mathimaticalPopulation.BestChromosome)[1], 0.01));
            Console.WriteLine(RangeCasting.ConvertValueToUserDefinedRange(_mathimaticalFitness.Translate(_mathimaticalPopulation.BestChromosome)[2], 0.01));
            Console.WriteLine(RangeCasting.ConvertValueToUserDefinedRange(_mathimaticalFitness.Translate(_mathimaticalPopulation.BestChromosome)[3], 0.1));
        }

        #endregion

        /// <summary>
        /// Loads and Initializes "TradeHub" strategy from the given assembly
        /// </summary>
        /// <param name="path">Assembly path</param>
        private void LoadAssembly(string path)
        {
            try
            {
                #region Single Thread
                // Load Assembly file from the selected file
                Assembly assembly = Assembly.LoadFrom(path);

                var strategyDetails = StrategyHelper.GetConstructorDetails(path);

                if (strategyDetails == null)
                {
                    Console.WriteLine("NULL value in strategy details");
                    return;
                }

                // Get Strategy Type
                _strategyType = strategyDetails.Item1;
                #endregion

                #region Parallel

                //string[] assemblyPaths = new string[10];

                //// Get all paths
                //for (int i = 0; i < 10; i++)
                //{
                //    assemblyPaths[i] = _assemblyFolder + i + _assemblyName;
                //}

                //// Load Assemblies
                //for (int i = 0; i < 10; i++)
                //{
                //    //Assembly assembly = Assembly.LoadFrom(assemblyPaths[i]);

                //    //var strategyDetails = LoadCustomStrategy.GetConstructorDetails(Assembly.LoadFrom(assemblyPaths[i]));

                //    //if (strategyDetails == null)
                //    //{
                //    //    Console.WriteLine("NULL value in strategy details");
                //    //    return;
                //    //}

                //    // Get Strategy Type
                //    _strategyTypeArray[i] = (LoadCustomStrategy.GetConstructorDetails(Assembly.LoadFrom(_assemblyPath))).Item1;
                //    //_strategyTypeArray[i] = strategyDetails.Item1;
                //}
                
                //Parallel.For(0, 10, increment =>
                //{
                //    //Assembly assembly = Assembly.LoadFrom(assemblyPaths[increment]);

                //    //var strategyDetails = LoadCustomStrategy.GetConstructorDetails(assembly);

                //    //if (strategyDetails == null)
                //    //{
                //    //    Console.WriteLine("NULL value in strategy details");
                //    //    return;
                //    //}

                //    // Get Strategy Type
                //    _strategyTypeArray[increment] = (LoadCustomStrategy.GetConstructorDetails(Assembly.LoadFrom(assemblyPaths[increment]))).Item1;
                //});
                #endregion
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        /// <summary>
        /// Responsible for population migration
        /// </summary>
        /// <param name="iteration"></param>
        private void MigratePopulation(int iteration)
        {
            switch (iteration)
            {
                case 0:
                    // Migrate values between populations
                    Parallel.For(0, 5,
                                 migrationIteration =>
                                 _populationArray[migrationIteration].Migrate(_populationArray[5 + migrationIteration],
                                                                              15, new EliteSelection()));
                    break;
                case 1:
                    // Migrate values between populations
                    for (int i = 0; i < 10; i+=2)
                    {
                        _populationArray[i].Migrate(_populationArray[i+1], 15, new EliteSelection());
                    }
                    break;
                //case 2:
                //    // Migrate values between populations
                //    Parallel.For(0, 5,
                //                 migrationIteration =>
                //                 _populationArray[migrationIteration].Migrate(_populationArray[9 - migrationIteration],
                //                                                              15, new EliteSelection()));
                //    break;
            }
        }

        /// <summary>
        /// Converts input values to appropariate AForge.Range values
        /// </summary>
        private double ConvertInputToValidRangeValues(double value, double incrementLevel)
        {
            const double smallestValue = 0.0000000000000001; // 16 Decimal places
            double multiplyingFactor = 1;

            string[] multiplyingFactorStringValue = incrementLevel.ToString(CultureInfo.InvariantCulture.NumberFormat).Split('.');

            // Get Multiplying Factor
            if (multiplyingFactorStringValue.Length > 1)
            {
                // Add Zeros
                for (int i = 1; i <= multiplyingFactorStringValue[1].Length; i++)
                {
                    multiplyingFactor *= 10;
                }

                multiplyingFactor *= Convert.ToInt32(multiplyingFactorStringValue[1]);
            }

            // return value in the appropariate AForge.Range
            return (multiplyingFactor * value) * smallestValue;
        }
    }
}
