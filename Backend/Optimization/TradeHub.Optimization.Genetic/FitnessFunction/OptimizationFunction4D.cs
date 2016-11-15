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
using AForge;
using AForge.Genetic;
using TraceSourceLogger;

namespace TradeHub.Optimization.Genetic.FitnessFunction
{
    /// <summary>
    /// Responsible for optimizing functions involving 4(Double) Parameters
    /// </summary>
    public abstract class OptimizationFunction4D : IFitnessFunction
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

        // Optimization ranges
        private Range _rangeW = new Range(0, 1);
        private Range _rangeX = new Range(0, 1);
        private Range _rangeY = new Range(0, 1);
        private Range _rangeZ = new Range(0, 1);

        // Optimization mode
        private Modes _mode = Modes.Maximization;

        #endregion

        #region Properties

        /// <summary>
        /// W variable's optimization range.
        /// </summary>
        public Range RangeW
        {
            get { return _rangeW; }
            set { _rangeW = value; }
        }

        /// <summary>
        /// X variable's optimization range.
        /// </summary>
        public Range RangeX
        {
            get { return _rangeX; }
            set { _rangeX = value; }
        }

        /// <summary>
        /// Y variable's optimization range.
        /// </summary>
        public Range RangeY
        {
            get { return _rangeY; }
            set { _rangeY = value; }
        }

        /// <summary>
        /// Z variable's optimization range.
        /// </summary>
        public Range RangeZ
        {
            get { return _rangeZ; }
            set { _rangeZ = value; }
        }

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
        /// Argument Constructor
        /// </summary>
        /// <param name="rangeW">Specifies W variable's range.</param>
        /// <param name="rangeX">Specifies X variable's range.</param>
        /// <param name="rangeY">Specifies Y variable's range.</param>
        /// <param name="rangeZ">Specifies Z variable's range.</param>
        protected OptimizationFunction4D(Range rangeW, Range rangeX, Range rangeY, Range rangeZ)
        {
            _rangeW = rangeW;
            _rangeX = rangeX;
            _rangeY = rangeY;
            _rangeZ = rangeZ;
        }
        
        /// <summary>
        /// Evaluates chromosome.
        /// </summary>
        /// <param name="chromosome">Chromosome to evaluate.</param>
        /// <returns>Returns chromosome's fitness value.</returns>
        public double Evaluate( IChromosome chromosome )
        {
            //TranslateGep(chromosome);
            // do native translation first
            double[] rangeParameters = TranslateGep( chromosome );
            
            // get function value
            double functionValue = OptimizationFunction(rangeParameters[0], rangeParameters[1], rangeParameters[2], rangeParameters[3]);

            //Logger.Error("Parameters, Alpha:"+rangeParameters[0]+", Beta:"+rangeParameters[1]+", Gamma:"+rangeParameters[2]+", Epsilon:"+rangeParameters[3]+", Fitness="+functionValue,"","");

            // return fitness value
            return ( _mode == Modes.Maximization ) ? functionValue : 1 / functionValue;
        }

        /// <summary>
        /// Translates genotype to phenotype 
        /// </summary>
        /// <param name="chromosome">Chromosome, which genoteype should be translated to phenotype</param>
        /// <returns>Returns chromosome's fenotype - the actual solution encoded by the chromosome</returns> 
        public double[] Translate( IChromosome chromosome )
        {
            // get chromosome's value
            ulong val = ((BinaryChromosome) chromosome).Value;
            // chromosome's length
            int length = ((BinaryChromosome) chromosome).Length;
            //Logger.Error("Chr Val="+chromosome.ToString()+", ulong="+val,"","");
            // All parameters will carry same length
            int parameterLength = length/4;

            // Maximum value : equal to component mask
            ulong maxValue = 0xFFFFFFFFFFFFFFFF >> (64 - parameterLength);

            // W component
            double wPart = val & maxValue;
            // X component;
            double xPart = (val >> parameterLength) & maxValue;
            // Y component;
            double yPart = (val >> (2 * parameterLength) & maxValue);
            // Z component;
            double zPart = val >> (3*(parameterLength));

            // translate to optimization's funtion space
            double[] parameterArray = new double[4];

            //parameterArray[0] = Value(val,_rangeW.Min,_rangeW.Max);
            //parameterArray[1] = Value(val, _rangeX.Min, _rangeX.Max);
            //parameterArray[2] = Value(val, _rangeY.Min, _rangeY.Max);
            //parameterArray[3] = Value(val, _rangeZ.Min, _rangeZ.Max);


            parameterArray[0] = wPart * _rangeW.Length / maxValue + _rangeW.Min;
            parameterArray[1] = xPart * _rangeX.Length / maxValue + _rangeX.Min;
            parameterArray[2] = yPart * _rangeY.Length / maxValue + _rangeY.Min;
            parameterArray[3] = zPart * _rangeZ.Length / maxValue + _rangeZ.Min;

            return parameterArray;
        }

        public double[] TranslateGep(IChromosome chromosome)
        {
            SimpleStockTraderChromosome chr = (SimpleStockTraderChromosome) chromosome;
            return chr.Values;
        }

        public double Value(ulong val, double min, double max)
        {
            return ((val - ulong.MinValue)/(ulong.MaxValue - ulong.MinValue))*(max - min) + min;
        }

        /// <summary>
        /// Function to optimize.
        /// </summary>
        /// <param name="w">Function W input value.</param>
        /// <param name="x">Function X input value.</param>
        /// <param name="y">Function Y input value.</param>
        /// <param name="z">Function Z input value.</param>
        /// <returns>Returns function output value.</returns>
        /// <remarks>The method should be overloaded by inherited class to
        /// specify the optimization function.</remarks>
        public abstract double OptimizationFunction(double w, double x, double y, double z);
    }
}
