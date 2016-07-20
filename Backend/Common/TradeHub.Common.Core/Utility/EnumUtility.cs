using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TraceSourceLogger;

namespace TradeHub.Common.Core.Utility
{
    /// <summary>
    /// Provides utility functions for the given Enum
    /// </summary>
    public static class EnumUtility
    {
        private static Type _type = typeof(EnumUtility);

        public static string GetEnumDescription(Enum value)
        {
            try
            {
                FieldInfo fi = value.GetType().GetField(value.ToString());

                DescriptionAttribute[] attributes =
                    (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

                if (attributes != null && attributes.Length > 0)
                    return attributes[0].Description;
                else
                    return value.ToString();
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "GetEnumDescription");
                return string.Empty;
            }
        }

        /// <summary>
        /// Returns all enum values
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IEnumerable<T> GetValues<T>()
        {
            return Enum.GetValues(typeof(T)).Cast<T>();
        }
    }
}
