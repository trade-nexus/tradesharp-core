using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeHub.Optimization.Genetic.Interfaces
{
    /// <summary>
    /// Interface for the Strategy Executor to be used for Optimization using Genetic Algo
    /// </summary>
    public interface IStrategyExecutor
    {
        /// <summary>
        /// Execute Strategy iteration to calculate Fitness
        /// </summary>
        /// <returns>Return Strategy's Fitness for current execution</returns>
        double ExecuteStrategy(double[] values);
    }
}
