using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TradeHub.Common.Core.CustomAttributes;
using TradeHub.StrategyRunner.Infrastructure.Utility;

namespace TradeHub.StrategyRunner.Infrastructure.Service
{
    /// <summary>
    /// Load the User defined custom strategy
    /// </summary>
    public static class LoadCustomStrategy
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
            foreach (Type type in assembly.GetTypes())
            {
                if (GetCustomAttributes(type).Count > 0)
                {
                    
                }
                
                if (type.GetCustomAttributes(typeof(TradeHubAttributes), true).Length > 0)
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
                foreach (Attribute attr in field.GetCustomAttributes(typeof(TradeHubAttributes),true))
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
                        customAttributes.Add(index,new Tuple<string, Type>(description, typeTemp));
                    }
                }
            }
            // return custom attributes information
            return customAttributes;
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
        /// Converts given value in the required format (TYPE)
        /// </summary>
        /// <param name="input">value to be converted</param>
        /// <param name="type">format to be converted into</param>
        /// <returns></returns>
        public static object GetParametereValue(string input, string type)
        {
            return ObjectInitializer.CastObject(input, type);
        }
    }
}
