using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentMigrator;

namespace TradeHub.Installer.DbMigration.Migrations
{
    /// <summary>
    /// Initial database setup and migration
    /// </summary>
    [Migration(1)]
    public class InitialMigration:Migration
    {
        public override void Down()
        {
            throw new NotImplementedException();
        }

        public override void Up()
        {
            Execute.Script("TradeHubDBScript.Sql");
        }
    }
}
