using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TradeHub.Installer.DbMigration;

namespace TradeHub.Installer.Configuration
{
    public static class MigrationsConfig
    {
        /// <summary>
        /// Start Migration
        /// </summary>
        public static void StartMigration(string connectionString)
        {
            try
            {
                var migrator = new Migrator(connectionString, "mysql");
                migrator.Migrate(runner => runner.MigrateUp());
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
            
        }
    }
}
