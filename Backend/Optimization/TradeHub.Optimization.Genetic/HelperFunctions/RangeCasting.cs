using System;
using System.Globalization;

namespace TradeHub.Optimization.Genetic.HelperFunctions
{
    public static class RangeCasting
    {
        /// <summary>
        /// Converts input values to appropariate AForge.Range values
        /// </summary>
        public static double ConvertInputToValidRangeValues(double value, double incrementLevel)
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

        /// <summary>
        /// Convert values to User defined range
        /// </summary>
        public static double ConvertValueToUserDefinedRange(double value, double incrementLevel)
        {
            double effectiveValue = 1;
            double multiplyingFactor = 1;

            string[] effectiveStringValue = value.ToString("F16", CultureInfo.InvariantCulture.NumberFormat).Split('.');
            string[] multiplyingFactorStringValue = incrementLevel.ToString(CultureInfo.InvariantCulture.NumberFormat).Split('.');

            // Get Orignal value
            effectiveValue = Convert.ToDouble(effectiveStringValue[1]);

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

            // return value in the appropariate User defined Range
            return (effectiveValue / multiplyingFactor);
        }
    }
}
