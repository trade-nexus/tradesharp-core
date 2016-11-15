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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AForge.Genetic;

namespace TradeHub.Optimization.Genetic.FitnessFunction
{
    /// <summary>
    /// Genetic optimization class
    /// </summary>
    public abstract class GeneticOptimization : IFitnessFunction
    {
        /// <summary>
        /// Optimization modes.
        /// </summary>
        public enum Modes
        {
            /// <summary>
            /// Search for function's maximum value.
            /// </summary>
            Maximization,
            /// <summary>
            /// Search for function's minimum value.
            /// </summary>
            Minimization
        }

        #region Fields
        // Optimization mode
        private Modes _mode = Modes.Maximization;

        #endregion

        #region Properties
        /// <summary>
        /// Optimization mode.
        /// </summary>
        /// <remarks>Defines optimization mode - what kind of extreme to search.</remarks> 
        public Modes Mode
        {
            get { return _mode; }
            set { _mode = value; }
        }

        #endregion

        /// <summary>
        /// Evaluates chromosome.
        /// </summary>
        /// <param name="chromosome">Chromosome to evaluate.</param>
        /// <returns>Returns chromosome's fitness value.</returns>
        public double Evaluate( IChromosome chromosome )
        {
            //TranslateGep(chromosome);
            // do native translation first
            double[] rangeParameters = Translate( chromosome );
            
            // get function value
            double functionValue = OptimizationFunction(rangeParameters);

            // return fitness value
            return ( _mode == Modes.Maximization ) ? functionValue : 1 / functionValue;
        }

        /// <summary>
        /// Translate Chromosome
        /// </summary>
        /// <param name="chromosome"></param>
        /// <returns></returns>
        public double[] Translate(IChromosome chromosome)
        {
            SimpleStockTraderChromosome chr = (SimpleStockTraderChromosome) chromosome;
            return chr.Values;
        }

        /// <summary>
        /// Function to be optimized
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public abstract double OptimizationFunction(double[] values);
    }
}
