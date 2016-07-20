using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Unity;
using Spring.Context.Support;
using TraceSourceLogger;
using TradeHub.StrategyRunner.Infrastructure.Service;
using TradeHub.StrategyRunner.Infrastructure.ValueObjects;
using TradeHub.StrategyRunner.UserInterface.Common;
using TradeHub.StrategyRunner.UserInterface.OptimizationShell;
using TradeHub.StrategyRunner.UserInterface.SearchModule.Utility;
using TradeHub.StrategyRunner.UserInterface.SearchModule.View;
using GeneticShell = TradeHub.StrategyRunner.UserInterface.GeneticAlgoShell.GeneticAlgoShell;

namespace TradeHub.StrategyRunner.UserInterface.SearchModule.ViewModel
{
    /// <summary>
    /// Contains Search View Functionality
    /// </summary>
    public class SearchViewModel : ViewModelBase
    {
        private Type _type = typeof (SearchViewModel);

        /// <summary>
        /// Holds refernce of the User selected strategy assembly
        /// </summary>
        private Assembly _strategyAssembly;

        /// <summary>
        /// Holds reference of user selected custom strategy
        /// </summary>
        private Type _strategyType;

        /// <summary>
        /// Save the name of the file selected by user
        /// </summary>
        private string _fileName;

        /// <summary>
        /// Save constuctor parameter info for the selected strategy
        /// </summary>
        private ParameterInfo[] _parmatersInfo;

        /// <summary>
        /// Holds reference to the Constuctor View Window
        /// </summary>
        private ConstructorView _constructorView;

        /// <summary>
        /// Holds reference to the Optimization Shell Window
        /// </summary>
        private Shell _optimizationShell;

        /// <summary>
        /// Holds reference to the Genetic Algo Shell Window
        /// </summary>
        private GeneticShell _geneticShell;

        /// <summary>
        /// Saves the number of parameters loaded
        /// </summary>
        private int _parametersCount = 0;

        /// <summary>
        /// Keeps track of successfully loaded Constructor Parameters for the latest selected Assembly
        /// </summary>
        private Dictionary<int, string[]> _selectedConstuctorParameters; 

        /// <summary>
        /// Command to Search for User Strategy.
        /// </summary>
        public ICommand SearchStrategy { get; set; }

        /// <summary>
        /// Command to Export Order in CSV file
        /// </summary>
        public ICommand ExportCommand { get; set; }

        /// <summary>
        /// Gets/Sets Assembly file of the User Strategy
        /// </summary>
        public Assembly StrategyAssembly
        {
            get { return _strategyAssembly; }
            set { _strategyAssembly = value; }
        }

        /// <summary>
        /// Holds reference of user selected custom strategy
        /// </summary>
        public Type StrategyType
        {
            get { return _strategyType; }
            set { _strategyType = value; }
        }

        /// <summary>
        /// Save the name of the file selected by user
        /// </summary>
        public string FileName
        {
            get { return _fileName; }
            set
            {
                _fileName = value;
                RaisePropertyChanged("FileName");
            }
        }

        /// <summary>
        /// Default Constructor
        /// </summary>
        public SearchViewModel()
        {

            _selectedConstuctorParameters = new Dictionary<int, string[]>();
            SearchStrategy = new DelegateCommand(LoadStrategy);
            ExportCommand = new DelegateCommand(ExportOrders);

            #region Event Aggregator

            EventSystem.Subscribe<StrategyConstructorInfo>(DisplayConstructorParameterNames);
            EventSystem.Subscribe<List<string[]>>(DisplayConstructorParameterValues);
            EventSystem.Subscribe<VerfiyParameters>(VerfiySelectedParameterValues);

            #endregion
        }

        /// <summary>
        /// Loads the selected Strategy Type
        /// </summary>
        private void LoadStrategy()
        {
            try
            {
                IUnityContainer container = new UnityContainer();

                // Open File Dialog Service to browse file
                var fileDialogService = container.Resolve<FileDialogService>();

                if (fileDialogService.OpenFileDialog(".dll", "Library Files") == true)
                {
                    if (Logger.IsDebugEnabled)
                    {
                        Logger.Debug("File select browser opened.", _type.FullName, "LoadStrategy");
                    }

                    // Get the name of the selected file
                    string fileName = fileDialogService.FileName;

                    // Load Assembly file from the selected file
                    Assembly assembly = Assembly.LoadFrom(fileName);

                    if (assembly != null)
                    {
                        // Save File name
                        FileName = assembly.FullName.Substring(0, assembly.FullName.IndexOf(",", System.StringComparison.Ordinal));

                        // Save Assembly reference
                        _strategyAssembly = assembly;

                        if (Logger.IsInfoEnabled)
                        {
                            Logger.Info("Successfully loaded User Strategy: " + assembly.FullName, _type.FullName, "LoadStrategy");
                        }

                        // Clear previously saved parameters
                        _selectedConstuctorParameters.Clear();
                        // Reset Count
                        _parametersCount = 0;

                        // Create new Strategy Runner LoadStrategy Object
                        LoadStrategy loadStrategy= new LoadStrategy(_strategyAssembly);

                        // Publish Event to notify Listeners.
                        EventSystem.Publish<LoadStrategy>(loadStrategy);
                        return;
                    }

                    if (Logger.IsInfoEnabled)
                    {
                        Logger.Info("Unable to load User Strategy", _type.FullName, "LoadStrategy");
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "LoadStrategy");
            }
        }

        /// <summary>
        /// Exports Order in CSV file
        /// </summary>
        private void ExportOrders()
        {
            if (Logger.IsInfoEnabled)
            {
                Logger.Info("Exporting orders to CSV on user command", _type.FullName, "ExportOrders");
            }

            // Publish Event to notify Listeners.
            EventSystem.Publish<string>("ExportOrders");
        }

        /// <summary>
        /// Displays the required constructor parameter names for the selected strategy
        /// </summary>
        /// <param name="strategyConstructorInfo">Contains details of selected strategy constructor</param>
        private void DisplayConstructorParameterNames(StrategyConstructorInfo strategyConstructorInfo)
        {
            try
            {
                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug("Displaying constructor details."  + strategyConstructorInfo.ParameterInfo,
                                                                    _type.FullName, "DisplayConstructorParameterNames");
                }

                // Save Selected strategy Type (class which implements TradeHubStrategy.cs)
                _strategyType = strategyConstructorInfo.StrategyType;
                // Save loaded Constructor parameters info
                _parmatersInfo = strategyConstructorInfo.ParameterInfo;

                // Get View to display details
                var context = ContextRegistry.GetContext();
                _constructorView = context.GetObject("ConstructorView") as ConstructorView;

                if (_constructorView != null)
                {
                    // Set List View Header values
                    _constructorView.SetGridColumnHeader(strategyConstructorInfo.ParameterInfo);

                    // Display Details
                    _constructorView.Show();
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "DisplayConstructorParameterNames");
            } 
        }

        /// <summary>
        /// Displays the selected strategy constructor parameter values
        /// </summary>
        /// <param name="parametersList">Parameters read in string format</param>
        private void DisplayConstructorParameterValues(List<string[]> parametersList)
        {
            try
            {
                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug("Displaying constructor values.",_type.FullName, "DisplayConstructorParameterValues");
                }

                if (_constructorView != null)
                {
                    foreach (string[] parameters in parametersList)
                    {
                        if (_parmatersInfo.Length != parameters.Length)
                        {
                            if (Logger.IsDebugEnabled)
                            {
                                Logger.Debug("Parameters count doesnot match the required number. " + parameters, _type.FullName, "DisplayConstructorParameterValues");
                            }
                            break;
                        }

                        // Add to local map
                        _selectedConstuctorParameters.Add(_parametersCount, parameters);

                        // Set List View Header values
                        _constructorView.SetGridColumnValues(parameters);

                        // Inceremnt count for each successfully added set of parameters
                        _parametersCount++;
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "DisplayConstructorParameterValues");
            } 
        }

        /// <summary>
        /// Verfies the Selected set of Constructor Parameters for the Loaded Custom Strategy Assembly
        /// </summary>
        /// <param name="verfiyParameters">Contains info regarding the parameters to be verified</param>
        private void VerfiySelectedParameterValues(VerfiyParameters verfiyParameters)
        {
            try
            {
                // Verify the ctor args if provided to run strategy
                if (verfiyParameters.SelectedIndex.Equals(-1))
                {
                    var ctrArgs = VerfiySelectedArguments(verfiyParameters.CtrArgs);

                    // Start normal strategy execution
                    if (ctrArgs != null)
                    {
                        RunStrategy(ctrArgs);
                    }
                }
                // Verify the ctor args if provided to run strategy optimization using GA
                else if (verfiyParameters.SelectedIndex.Equals(-2))
                {
                    var ctrArgs = VerfiySelectedArguments(verfiyParameters.CtrArgs);

                    // Start normal strategy execution
                    if (ctrArgs != null)
                    {
                        RunStrategyOptimizationGeneticAlgo(ctrArgs);
                    }
                }
                // Verfiy the selected args from the given index to run strategy optimization using Brute Force
                else
                {
                    var ctrArgs = VerfiySelectedArgumentsOnGivenIndex(verfiyParameters.SelectedIndex);

                    // Start brute force optimizataion of the strategy
                    if (ctrArgs != null)
                    {
                        RunStrategyOptimizationBruteForce(ctrArgs);
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "VerfiySelectedParameterValues");
            }
        }

        /// <summary>
        /// Starts normal execution of the strategy
        /// </summary>
        /// <param name="ctrArgs">Arguments to be used to execute strategy</param>
        private void RunStrategy(object[] ctrArgs)
        {

            // Create new object to use with Event Aggregator
            InitializeStrategy initializeStrategy = new InitializeStrategy(_strategyType, ctrArgs);

            // Publish Event to Notify Listeners
            EventSystem.Publish<InitializeStrategy>(initializeStrategy);
        }

        /// <summary>
        /// Starts optimization of the strategy using Brute Force
        /// </summary>
        /// <param name="ctrArgs">Arguments to be used to execute strategy</param>
        private void RunStrategyOptimizationBruteForce(object[] ctrArgs)
        {
            // Get View to display details for optimization engine
            var context = ContextRegistry.GetContext();
            _optimizationShell = context.GetObject("OptimizationShell") as Shell;

            if (_optimizationShell != null)
            {
                _optimizationShell.Show();

                // Create new object to use with Event Aggregator
                OptimizationParametersBruteForce optimizeStrategy = new OptimizationParametersBruteForce(_strategyType, ctrArgs, _parmatersInfo);

                // Publish Event to Notify Listeners
                EventSystem.Publish<OptimizationParametersBruteForce>(optimizeStrategy);
            }
        }

        /// <summary>
        /// Starts optimization of the startegy using Genetic Algorithm
        /// </summary>
        /// <param name="ctrArgs">Arguments to be used to execute strategy</param>
        private void RunStrategyOptimizationGeneticAlgo(object[] ctrArgs)
        {
            // Get View to display details for optimization engine
            var context = ContextRegistry.GetContext();
            _geneticShell = context.GetObject("GeneticAlgoShell") as GeneticShell;

            if (_geneticShell != null)
            {
                _geneticShell.Show();

                // Get custom attributes from the given strategy
                var customAttributes = GetCustomAttributes(_strategyType);

                if (customAttributes != null)
                {
                    // Create new object to use with Event Aggregator
                    OptimizationParametersGeneticAlgo optimizeStrategy = new OptimizationParametersGeneticAlgo(ctrArgs,
                                                                                                   _strategyType,
                                                                                                   customAttributes);

                    // Publish Event to Notify Listeners
                    EventSystem.Publish<OptimizationParametersGeneticAlgo>(optimizeStrategy);
                }
                else
                {
                    Logger.Info("No Custom Attributes were found for GA optimization", _type.FullName, "RunStrategyOptimizationGeneticAlgo");
                }
            }
        }

        /// <summary>
        /// Verifies the arguments on selected index to initialize the strategy
        /// </summary>
        /// <param name="index">Index of the arguments to verify</param>
        private object[] VerfiySelectedArgumentsOnGivenIndex(int index)
        {
            try
            {
                string[] parameters;
                if (_selectedConstuctorParameters.TryGetValue(index, out parameters))
                {
                    object[] ctrArgs = new object[parameters.Length];

                    foreach (ParameterInfo parameterInfo in _parmatersInfo)
                    {
                        object value;

                        // Convert string value to required format
                        value = LoadCustomStrategy.GetParametereValue(parameters[parameterInfo.Position], parameterInfo.ParameterType.Name);
                        if (value == null)
                        {
                            if (Logger.IsInfoEnabled)
                            {
                                Logger.Info("Parameter was not in the correct format. Rquired: " + parameterInfo.ParameterType.Name +
                                            " Provided value: " + parameters[parameterInfo.Position],
                                            _type.FullName, "VerfiySelectedArgumentsOnGivenIndex");
                            }
                            return null;
                        }
                        // Add to value to arguments array
                        ctrArgs[parameterInfo.Position] = value;
                    }

                    // Return verified arguments
                    return ctrArgs;
                }

                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("Specified parameters could not be found.", _type.FullName, "VerfiySelectedParameterValues");
                }

                return null;
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "VerfiySelectedArgumentsOnGivenIndex");
                return null;
            }
        }

        /// <summary>
        /// Verifies the selected arguments to initialize the strategy
        /// </summary>
        /// <param name="selectedArgs">Constructor Arguments</param>
        private object[] VerfiySelectedArguments(object[] selectedArgs)
        {
            try
            {
                object[] ctrArgs = new object[selectedArgs.Length];

                foreach (ParameterInfo parameterInfo in _parmatersInfo)
                {
                    object value;

                    // Convert string value to required format
                    value = LoadCustomStrategy.GetParametereValue(selectedArgs[parameterInfo.Position].ToString(), parameterInfo.ParameterType.Name);
                    if (value == null)
                    {
                        if (Logger.IsInfoEnabled)
                        {
                            Logger.Info("Parameter was not in the correct format. Rquired: " + parameterInfo.ParameterType.Name +
                                        " Provided value: " + selectedArgs[parameterInfo.Position],
                                        _type.FullName, "VerfiySelectedArguments");
                        }
                        return null;
                    }
                    // Add to value to arguments array
                    ctrArgs[parameterInfo.Position] = value;
                }

                // Return verified arguments
                return ctrArgs;
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "VerfiySelectedArguments");
                return null;
            }
        }

        /// <summary>
        /// Returns Custom attributes used in the user strategy
        /// </summary>
        private Dictionary<int, Tuple<string, Type>> GetCustomAttributes(Type strategyType)
        {
            try
            {
                // Contains custom defined attributes in the given assembly
                Dictionary<int, Tuple<string, Type>> customAttributes = null;

                // Get Custom Attributes
                if (strategyType != null)
                {
                    // Get custom attributes from the given assembly
                    customAttributes = LoadCustomStrategy.GetCustomAttributes(strategyType);
                }

                return customAttributes;
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "GetCustomAttributes");
                return null;
            }
        }
    }
}
