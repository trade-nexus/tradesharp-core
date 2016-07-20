using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeHub.Optimization.Genetic.Tests.Application.HelperFunctions;

namespace TradeHub.Optimization.Genetic.Tests.Application
{
    class Program
    {
        static void Main(string[] args)
        {
            //const double userValue = 9;
            //const double increment = 0.01;

            //double aForgeConvertedValue = RangeCasting.ConvertInputToValidRangeValues(userValue, increment);
            //double acheivedValue = RangeCasting.ConvertValueToUserDefinedRange(aForgeConvertedValue, increment);

            //Console.WriteLine("User value: " + userValue);
            //Console.WriteLine("Increment: " + increment);
            //Console.WriteLine("AForge value: " + aForgeConvertedValue.ToString("F16", CultureInfo.InvariantCulture.NumberFormat));
            //Console.WriteLine("Acheievd value: " + acheivedValue);

            // Create Instance
           // Stopwatch sc=new Stopwatch();
           // sc.Start();
            OptimizationManager optimization = new OptimizationManager();

            //// Start Optimization of Mathimatical Function
            //optimization.ExecuteMathimaticalOptimization();

            // Start Optimization using AForge
            optimization.ExecuteStrategyWithFourParametersAForge();
           // sc.Stop();
            //Console.WriteLine("Executed in "+sc.ElapsedMilliseconds+"ms");

            //// Start Optimization using Optimera
            //optimization.ExecuteStrategyOptimera(4);

            //// Start Optimization using Brute force
            //optimization.ExecuteBruteForceOptimization();

            Console.WriteLine();
            Console.WriteLine("Press ENTER to terminate...");
            Console.ReadLine();
        }
    }
}