using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using TradeHub.Common.Core.CustomAttributes;
using TradeHub.StrategyEngine.Common.Utility;
using TradeHub.StrategyEngine.TradeHub;

namespace TradeHub.StrategyEngine.Utlility.Utility
{
    /// <summary>
    /// Load the User defined custom strategy
    /// </summary>
    internal static class LoadCustomStrategy
    {
        /// <summary>
        /// Provides Constructor details for the provided assembly
        /// </summary>
        /// <param name="assemblyName">Name of the assembly to be loaded</param>
        /// <returns>Tuple: TradeHub Strategy Type, Constructor Parameter details</returns>
        public static Tuple<Type, ParameterInfo[]> GetConstructorDetails(string assemblyName)
        {
            // Get Assembly from the selected file
            Assembly assembly = Assembly.LoadFrom(assemblyName);

            // Get strategy details
            return GetConstructorDetails(assembly);
        }

        /// <summary>
        /// Provides Constructor details for the provided assembly
        /// </summary>
        /// <param name="assembly">User Specified Assembly</param>
        /// <returns>Tuple: TradeHub Strategy Type, Constructor Parameter details</returns>
        public static Tuple<Type, ParameterInfo[]> GetConstructorDetails(Assembly assembly)
        {
            //Querying Class Attributes
            foreach (Type type in assembly.GetExportedTypes())
            {
                if (type.GetCustomAttributes(typeof (TradeHubAttributes), true).Length > 0)
                {
                    ConstructorInfo[] ctor = type.GetConstructors();
                    ConstructorInfo constructorInfo = ctor[0];

                    var ctorParam = constructorInfo.GetParameters();

                    return new Tuple<Type, ParameterInfo[]>(type, ctorParam);
                }
            }

            return null;
        }

        /// <summary>
        /// Creates a new instance of the User Strategy
        /// </summary>
        /// <param name="type">User Strategy</param>
        /// <param name="ctrArgs">Constructor arguments</param>
        public static object CreateStrategyInstance(Type type, object[] ctrArgs)
        {
            return Activator.CreateInstance(type, ctrArgs);
        }

        /// <summary>
        /// Gets Custom Properties from the given Type
        /// </summary>
        /// <param name="type">Class Type implementing TradeHubStrategy</param>
        public static Dictionary<int, Tuple<string, Type>> GetCustomAttributes(Type type)
        {
            // Dictionary to hold custom attributes for the given class instance
            var customAttributes = new Dictionary<int, Tuple<string, Type>>();

            // Traverse all properties
            foreach (PropertyInfo field in type.GetProperties())
            {
                // Get available custom attributes
                foreach (Attribute attr in field.GetCustomAttributes(typeof (TradeHubAttributes), true))
                {
                    // Cast to see if its a valid TradeHub Attribute
                    var tradeHubAtt = attr as TradeHubAttributes;
                    if (tradeHubAtt != null)
                    {
                        // Get Attribute Index
                        int index = tradeHubAtt.Index;
                        // Get attribute description
                        string description = tradeHubAtt.Description;
                        // Get attribute Type
                        Type typeTemp = tradeHubAtt.Value;

                        // Add to dictionary
                        customAttributes.Add(index, new Tuple<string, Type>(description, typeTemp));
                    }
                }
            }
            // return custom attributes information
            return customAttributes;
        }

        /// <summary>
        /// Gets Tradehub strategy Class Type
        /// </summary>
        /// <param name="assemblyPath">assemblyPath</param>
        public static Type GetStrategyClassType(string assemblyPath)
        {
            Type strategyType = null;

            // Get Assembly from the selected file
            Assembly assembly = Assembly.LoadFrom(assemblyPath);

            //Querying Class Attributes
            foreach (Type type in assembly.GetExportedTypes())
            {
                if (type.GetCustomAttributes(typeof(TradeHubAttributes), true).Length > 0)
                {
                    strategyType = type;
                    break;
                }
            }

            return strategyType;
        }

        /// <summary>
        /// Gets Custom Class Attributes
        /// </summary>
        /// <param name="type">Class Type implementing TradeHubStrategy</param>
        public static string GetCustomClassSummary(Type type)
        {
            //Querying Class Attributes
            foreach (Attribute attr in type.GetCustomAttributes(true))
            {
                var tempAttribute = attr as TradeHubAttributes;
                if (tempAttribute != null)
                {
                    return tempAttribute.Description;
                }
            }
            return null;
        }

        /// <summary>
        /// Returns Parameters details i.e Parameter names with there Types
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, Type> GetParameterDetails(Type assemblyType)
        {
            // Initialize Dictionary to hold details
            Dictionary<string, Type> details = new Dictionary<string, Type>();

            if (assemblyType.GetCustomAttributes(typeof (TradeHubAttributes), true).Length > 0)
            {
                ConstructorInfo[] ctor = assemblyType.GetConstructors();
                ConstructorInfo constructorInfo = ctor[0];

                // Get Constructor Parameters
                var ctorParam = constructorInfo.GetParameters();

                // Traverse all Parameters
                foreach (var parameterInfo in ctorParam)
                {
                    // Add details to the Dictionary
                    details.Add(parameterInfo.Name, parameterInfo.ParameterType);
                }
            }

            // Return information populated
            return details;
        }

        /// <summary>
        /// Converts given value in the required format (TYPE)
        /// </summary>
        /// <param name="input">value to be converted</param>
        /// <param name="type">format to be converted into</param>
        /// <returns></returns>
        public static object GetParametereValue(string input, string type)
        {
            return ObjectInitializer.CastObject(input, type);
        }

        /// <summary>
        /// Verify that given assembly derives from TradeHub strategy class
        /// </summary>
        /// <param name="assemblyPath"></param>
        /// <returns></returns>
        public static bool VerifyStrategy(string assemblyPath)
        {
            Assembly assembly = Assembly.LoadFrom(assemblyPath);

            //Querying Class Attributes
            foreach (Type type in assembly.GetExportedTypes())
            {
                if (type.GetCustomAttributes(typeof (TradeHubAttributes), true).Length > 0)
                {
                    if (type.IsSubclassOf(typeof (TradeHubStrategy)))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
