## TradeSharp ##

A C# based data feed and broker neutral Algorithmic Trading Platform that lets trading firms or individuals automate any rules based trading strategies in stocks, forex and ETFs.

Find more about TradeSharp [here](https://www.tradesharp.se/).


***


### Installations (Tools and Requirements) ###
1. Microsoft Visual Studio 2012 or higher
1. .NET Framework 4.5.1
1. MySQL
1. Git
1. Wix
1. RabbitMQ
    * Download and install erlang: http://www.erlang.org/download.html
    * Restart System
    * Download and install RabbitMQ: http://www.rabbitmq.com/download.html
    * For Installation Problems: http://www.rabbitmq.com/install-windows.html
1. Resharper  (Visual Studio extension for .NET Developers )
    * Download and Install Resharper: https://www.jetbrains.com/resharper/
1. SourceTree  (GIT client for windows)
    * Download and Install SourceTree: https://www.sourcetreeapp.com/


### Code Cloning ###
1. Clone TradeSharp backend (tradesharp-core):
    * git clone https://github.com/TradeNexus/tradesharp-core
1. Clone TradeSharp frontend (tradesharp-ui):
    * git clone https://github.com/TradeNexus/tradesharp-ui


### Setting Up Database ###
1. Create a new database named Tradehub.
1. Settings:
    * **UserName**:  root
    * **Password**:  root
    * **Host**:  localhost
1. Run sql script: TradeHubDBScript.sql located in **<Path to tradesharp-ui code cloned>\database\**

To load data from workbench 

1. Open MySQL Workbench
1. Click on “**Local Instance MySQL**”  and enter password when prompted.
1. From top menu bar, click on  **Server -> Import Data.**
1. Select import from self-contained file and provide path to ‘**TradeHubDBScript.sql**’ file .
1. Select “**TradeHub**” as target schema.
1. Press ‘**Start Import**’ button.
1. Restart workbench to ensure tables are present in db.

### Opening Code With Visual Studio ###
1. Run Visual Studio.
1. Click on **File -> Open -> Project/Solution…**
1. In the new windows that opens, navigate to the tradesharp-core code cloned.  **tradesharp-core -> Backend**
1. Select **TradeHub.sln.**
1. Run another instance of Visual Studio.
1. Click on **File -> Open -> Project/Solution…**
1. In the new windows that opens, navigate to tradesharp-ui code cloned and select **TradeHubGui.sln**


### Enabling Test Code to Simulate Service Start ###
1. In Visual Studio instance with tradesharp-ui code, open **TradeHubGui\ViewModel\ServicesViewModel.cs**
1. Find **PopulateServices()** method and then locate comments
    **“//NOTE: Test code to simulate Service Start”
    “// BEGIN:”**
1. Uncomment the block of code that follows till “// :END” comment.
1. Locate comment “// Disbaled for testing” in the same method near the end and comment the line of code “InitializeServices();”
1. Find **StopService()** method in the same file and repeat steps 2-3.


### Running Application ###
1. Compiling Code (When running the code for the first time or after making changes)
    * Click on **Build->Clean** Solution from top menu bar.
    * Click on **Build->Build** Solution
1. Starting the Backend
    * In the Visual Studio instance with TradeSharp Backend code, locate and expand  MarketDateEngine in the solution explorer.
    * Right click on **TradeHub.MarketDataEngine.Server.Console**
    * Click on **Debug->Start new instance**.
    * If everything is fine a new console window should open up.
    * Locate and expand **OrderExecutionEngine**.
    * Right click on **TradeHub.OrderExecutionEngine.Server.Console**
    * Click on **Debug->Start new instance**.
    * If everything is fine a new console window should open up.
1. Starting the tradesharp-ui
    * In Visual Studio instance with tradesharp-ui code,right click on TradeHubGui project and ‘**Set as startup project**’
    * click on ‘**Start**’ from the top menu bar. 


### Viewing Logs ###
1. Log Locations
    * TradeSharp  Logs in ProgramData folder.  **(C:\ProgramData\TradeSharp Logs).**
    * Note: ProgramData folder may be hidden.
1. Some log files are created in component Debug folders. For example after running MarketDataEngine Console, log file will be created in **<Path to tradesharp-core code cloned>\Backend\MarketDataEngine\TradeHub.MarketDataEngine.Server.Console\bin\Debug\**.

