using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using TradeHub.Common.Core.Constants;
using TradeHub.StrategyEngine.Common.Utility;
using TradeHub.StrategyEngine.Utlility.Utility;

namespace TradeHub.StrategyEngine.Utlility.Services
{
    /// <summary>
    /// Provides helping functionalities related to strategy
    /// </summary>
    public static class StrategyHelper
    {
        /// <summary>
        /// Vealidate startegy dll
        /// </summary>
        /// <param name="assemblyPath">Path of the assembly</param>
        /// <returns></returns>
        public static bool ValidateStrategy(string assemblyPath)
        {
            return LoadCustomStrategy.VerifyStrategy(assemblyPath);
        }

        /// <summary>
        /// Add assembly to relative directory
        /// </summary>
        /// <param name="assemblyPath"></param>
        /// <returns></returns>
        public static bool CopyAssembly(string assemblyPath)
        {
            if (!Directory.Exists(DirectoryStructure.STRATEGY_LOCATION))
            {
                Directory.CreateDirectory(DirectoryStructure.STRATEGY_LOCATION);
            }
            string fileName = Path.GetFileNameWithoutExtension(assemblyPath);
            
            if (Directory.Exists(DirectoryStructure.STRATEGY_LOCATION + "\\" + fileName))
            {
                throw new InvalidOperationException("Direcotry already exists");
            }
            
            DirectoryInfo destionationDirectory =
                Directory.CreateDirectory(DirectoryStructure.STRATEGY_LOCATION + "\\" + fileName);

            string[] files = Directory.GetFiles(Path.GetDirectoryName(assemblyPath));
            foreach (string file in files)
            {
                File.Copy(file, destionationDirectory.FullName + "\\" + Path.GetFileName(file));
            }
            return true;
        }

        /// <summary>
        /// Remove assembly from relative directory
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static bool RemoveAssembly(string fileName)
        {
            // Create complete strategy directory path from file name
            string strategyDirectory = DirectoryStructure.STRATEGY_LOCATION + "\\" + fileName;
            
            // Get all files in the directory to be deleted
            string[] files = Directory.GetFiles(DirectoryStructure.STRATEGY_LOCATION + "\\" + fileName);

            // Delete all files
            foreach (string file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }

            if (Directory.Exists(strategyDirectory))
            {
                Directory.Delete(strategyDirectory, true);
                return !Directory.Exists(strategyDirectory);
            }
             return false;
        }

        /// <summary>
        /// Returns Name of the '.dll' file selected
        /// </summary>
        /// <param name="assemblyPath"></param>
        /// <returns></returns>
        public static string GetStrategyFileName(string assemblyPath)
        {
            return Path.GetFileNameWithoutExtension(assemblyPath);
        }

        /// <summary>
        /// Get All Strategies Names
        /// </summary>
        /// <returns></returns>
        public static List<string> GetAllStrategiesName()
        {
            List<string> directoryNames = new List<string>();
            if (!Directory.Exists(DirectoryStructure.STRATEGY_LOCATION)) return directoryNames;

            string[] directories = Directory.GetDirectories(DirectoryStructure.STRATEGY_LOCATION);
            foreach (string directory in directories)
            {
                directoryNames.Add(directory.Split('\\').Last());
            }
            return directoryNames;
        }

        /// <summary>
        /// Get All Strategies Paths
        /// </summary>
        /// <returns></returns>
        public static List<string> GetAllStrategiesPath()
        {
            List<string> paths=new List<string>();
            var strategies = GetAllStrategiesName();
            foreach (var strategy in strategies)
            {
                paths.Add(GetStrategyPath(strategy));
            }
            return paths;
        }

        /// <summary>
        /// Get strategy path
        /// </summary>
        /// <param name="strategyName"></param>
        /// <returns></returns>
        public static string GetStrategyPath(string strategyName)
        {
            return string.Format(DirectoryStructure.STRATEGY_LOCATION + "\\{0}\\{1}.dll", strategyName, strategyName);
        }

        /// <summary>
        /// Get constructor details
        /// </summary>
        /// <param name="assemblyName"></param>
        /// <returns></returns>
        public static Tuple<Type, ParameterInfo[]> GetConstructorDetails(string assemblyName)
        {
            return LoadCustomStrategy.GetConstructorDetails(assemblyName);
        }

        /// <summary>
        /// Create instance of strategy
        /// </summary>
        /// <param name="type"></param>
        /// <param name="ctrArgs"></param>
        /// <returns></returns>
        public static object CreateStrategyInstance(Type type, object[] ctrArgs)
        {
            return LoadCustomStrategy.CreateStrategyInstance(type, ctrArgs);
        }

        /// <summary>
        /// Get custom attributes of type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Dictionary<int, Tuple<string, Type>> GetCustomAttributes(Type type)
        {
            return LoadCustomStrategy.GetCustomAttributes(type);
        }

        /// <summary>
        /// Gets Tradehub strategy Class Type
        /// </summary>
        /// <param name="assemblyPath">assemblyPath</param>
        public static Type GetStrategyClassType(string assemblyPath)
        {
            return LoadCustomStrategy.GetStrategyClassType(assemblyPath);
        }

        /// <summary>
        /// Get class summary
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetCustomClassSummary(Type type)
        {
            return LoadCustomStrategy.GetCustomClassSummary(type);
        }

        /// <summary>
        /// Returns Parameters details i.e Parameter names with there Types
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, Type> GetParameterDetails(Type assemblyType)
        {
            return LoadCustomStrategy.GetParameterDetails(assemblyType);
        }

        /// <summary>
        /// Get parameter values
        /// </summary>
        /// <param name="input"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object GetParametereValue(string input, string type)
        {
            return LoadCustomStrategy.GetParametereValue(input, type);
        }
    }
}
