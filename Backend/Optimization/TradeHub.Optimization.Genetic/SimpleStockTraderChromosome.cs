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
using AForge.Math.Random;

namespace TradeHub.Optimization.Genetic
{
    /// <summary>
    /// Simple Stock Trader Chromosome
    /// </summary>
    public class SimpleStockTraderChromosome:ChromosomeBase
    {
        #region Fields and Properties
        private int _length;
        private Range[] _ranges;

        public Range[] Ranges
        {
            get { return _ranges; }
            set { _ranges = value; }
        }

        private double[] _values;

        public UniformGenerator[] UniformGenerators
        {
            get { return _uniformGenerators; }
        }

        public double[] Values
        {
            get { return _values; }
            set { _values = value; }
        }

        private UniformGenerator[] _uniformGenerators;
        ThreadSafeRandom _random=new ThreadSafeRandom(DateTime.Now.Millisecond);
        #endregion
        /// <summary>
        /// Parameterized Constructor
        /// </summary>
        /// <param name="ranges"></param>
        public SimpleStockTraderChromosome(Range[] ranges)
        {
            _ranges = (Range[])ranges.Clone();
            _length = _ranges.Length;
            _values=new double[_length];
            InitializeGenerators();
            Generate();
        }

        /// <summary>
        /// Parameterized Constructor
        /// </summary>
        /// <param name="chromosome"></param>
        public SimpleStockTraderChromosome(SimpleStockTraderChromosome chromosome)
        {
            _ranges = (Range[])chromosome.Ranges.Clone();
            _length = _ranges.Length;
            _values = (double[])chromosome.Values.Clone();
            InitializeGenerators();
        }

        /// <summary>
        /// Initialize random generators
        /// </summary>
        public void InitializeGenerators()
        {
            _uniformGenerators=new UniformGenerator[_length];
            for (int i = 0; i < _length; i++)
            {
                //if (_ranges[i].Min < 1 && _ranges[i].Max > 1)
                //{
                //    _uniformGenerators[i] = new UniformGenerator(new Range(0, 1), DateTime.Now.Millisecond);
                //}
                //else
                //{
                //    _uniformGenerators[i] = new UniformGenerator(_ranges[i], DateTime.Now.Millisecond);
                //}
                _uniformGenerators[i] = new UniformGenerator(_ranges[i], DateTime.Now.Millisecond);
            }
        }

        /// <summary>
        /// Clone the current chromosome
        /// </summary>
        /// <returns></returns>
        public override IChromosome Clone()
        {
            return new SimpleStockTraderChromosome(this);
        }

        /// <summary>
        /// Create new chromosome
        /// </summary>
        /// <returns></returns>
        public override IChromosome CreateNew()
        {
            return new SimpleStockTraderChromosome(this.Ranges);
        }

        /// <summary>
        /// Perform two point crossover between seletected pair
        /// </summary>
        /// <param name="pair"></param>
        public override void Crossover(IChromosome pair)
        {
            //If length is less than 3 two point crossover cannot be performed.
            if (_length < 3)
            {
               int xoverpoint = (int)Math.Ceiling(_random.NextDouble() * (_length - 1));
               SinglePointCrossover(pair,xoverpoint); 
               return;
            }
            SimpleStockTraderChromosome chromosome = (SimpleStockTraderChromosome) pair;
            int left = _random.Next(_length-1);
            int right = _random.Next(_length-1);
            while(left==right)
                right = _random.Next(_length-1);
            if (left > right)
            {
                double[] temp = new double[_length];

                // copy part of first (this) chromosome to temp
                Array.Copy(_values, temp, _length);
                // copy part of second (pair) chromosome to the first
                Array.Copy(chromosome.Values,right, _values, right, left-right);
                // copy temp to the second
                Array.Copy(temp, right, chromosome.Values, right, left - right);
            }
            else
            {
                double[] temp = new double[_length];
                // copy part of first (this) chromosome to temp
                Array.Copy(_values, temp, _length);
                // copy part of second (pair) chromosome to the first
                Array.Copy(chromosome.Values, left, _values, left, right - left);
                // copy temp to the second
                Array.Copy(temp, left, chromosome.Values, left, right - left);
            }
        }

        /// <summary>
        /// Single point crossover
        /// </summary>
        public void SinglePointCrossover(IChromosome pair, int xoverpoint)
        {
            SimpleStockTraderChromosome chromosome = (SimpleStockTraderChromosome) pair;
            Array.Copy(chromosome.Values, xoverpoint, _values, xoverpoint, _length-xoverpoint);
        }

        /// <summary>
        /// Generate Initial Population
        /// </summary>
        public override void Generate()
        {
            for (int i = 0; i < _length; i++)
            {
                //_values[i] = _uniformGenerators[i].Next();
                _values[i] = Ranges[i].Min + (Ranges[i].Max - Ranges[i].Min) * _random.NextDouble();
            }
        }

        /// <summary>
        /// Perform mutation of the selected chromosome
        /// </summary>
        public override void Mutate()
        {
            //for (int j = 0; j < 15; j++)
            //{
            //    double[] trials = new double[4];
            //    int[] direction = new int[4];
            //    for (int i = 0; i < 4; i++)
            //    {
            //        direction[i] = _random.Next(1);
            //        trials[i] = _values[i] + direction[i];
            //    }
            //    if (IsTrialFeasible(trials))
            //    {
            //        _values = trials;
            //        return;
            //    }
            //}
            int rand = _random.Next(_length-1);
            //int operatorRand = _random.Next(1);
            ////_values[rand] = _uniformGenerators[rand].Next();
            //if (operatorRand == 0)
            //{
            //    var temp = _values[rand];
            //    _values[rand] += (_ranges[rand].Max - _ranges[rand].Min) / 10;
            //    if (_values[rand] > _ranges[rand].Max)
            //    {
            //        _values[rand] = temp;
            //    }
            //}
            //else
            //{
            //    var temp = _values[rand];
            //    _values[rand] *= (_ranges[rand].Max - _ranges[rand].Min) / 10;
            //    if (_values[rand] > _ranges[rand].Max || _values[rand] < _ranges[rand].Min)
            //    {
            //        _values[rand] = temp;
            //    }
            //}
            var temp = _values[rand];
            double sigma = (_ranges[rand].Max - _ranges[rand].Min) / 6d;
            sigma = Math.Max(0, sigma * Math.Exp(GaussianMutation(0, 1)));
            _values[rand] = GaussianMutation(_values[rand], sigma);
            if (_values[rand] < _ranges[rand].Min || _values[rand] > _ranges[rand].Max)
            {
                _values[rand] = temp;
            }
        }

        /// <summary>
        /// Check if the trial is feasible
        /// </summary>
        /// <param name="trials"></param>
        /// <returns></returns>
        public bool IsTrialFeasible(double[] trials)
        {
            for (int i = 0; i < trials.Length; i++)
            {
                if (trials[i] < _ranges[i].Min || trials[i] > _ranges[i].Max)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Equal override method
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj is SimpleStockTraderChromosome)
            {
                SimpleStockTraderChromosome chromosome = obj as SimpleStockTraderChromosome;
                if (chromosome.fitness != fitness)
                {
                    return false;
                }
                for (int i = 0; i < _length; i++)
                {
                    if (_values[i] != chromosome.Values[i])
                        return false;
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Performs Gaussian Mutation
        /// </summary>
        /// <param name="mean"></param>
        /// <param name="stddev"></param>
        /// <returns></returns>
        private double GaussianMutation(double mean, double stddev)
        {
            double x1 = _random.NextDouble();
            double x2 = _random.NextDouble();

            // The method requires sampling from a uniform random of (0,1]
            // but Random.NextDouble() returns a sample of [0,1).
            // Thanks to Colin Green for catching this.
            if (x1 == 0)
                x1 = 1;
            if (x2 == 0)
                x2 = 1;

            double y1 = Math.Sqrt(-2.0 * Math.Log(x1)) * Math.Cos(2.0 * Math.PI * x2);
            return y1 * stddev + mean;
        }

        /// <summary>
        /// Clamp values if value is out of range.
        /// </summary>
        /// <param name="val"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        private double Clamp(double val, double min, double max)
        {
            if (val >= max)
                return max;
            if (val <= min)
                return min;
            return val;
        }
    }
}
