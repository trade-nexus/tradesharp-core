using System;
using System.Reflection;
using FluentMigrator;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Initialization;

namespace TradeHub.Installer.DbMigration
{
    /// <summary>
    /// Database migration handler
    /// </summary>
    public class Migrator
    {
        readonly string _connectionString;
        readonly string _dbType;

        public Migrator(string connectionString, string dbType)
        {
            _connectionString = connectionString;
            _dbType = dbType;
        }

        private class MigrationOptions : IMigrationProcessorOptions
        {
            public bool PreviewOnly { get; set; }
            public int Timeout { get; set; }
            public string ProviderSwitches { get; set; }
        }

        /// <summary>
        /// Migrate updwards or downwards
        /// </summary>
        /// <param name="runnerAction"></param>
        public void Migrate(Action<IMigrationRunner> runnerAction)
        {
            var options = new MigrationOptions { PreviewOnly = false, Timeout = 0 };
            var factory = new FluentMigrator.Runner.Processors.MigrationProcessorFactoryProvider().GetFactory(_dbType);
            var assembly = Assembly.GetExecutingAssembly();

            var announcer = new TextWriterAnnouncer(s => System.Diagnostics.Debug.WriteLine(s));
            var migrationContext = new RunnerContext(announcer);
            var processor = factory.Create(_connectionString, announcer, options);
            var runner = new MigrationRunner(assembly, migrationContext, processor);
            runnerAction(runner);
        }
    }
}
