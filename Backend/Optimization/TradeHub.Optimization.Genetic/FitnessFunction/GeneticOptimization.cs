using System;
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
