namespace CustomizeMigrationTable.Migrations
{
    using System;
    using System.Data.Common;
    using System.Data.Entity;
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Migrations;
    using System.Data.Entity.Migrations.History;
    using System.Data.Entity.Migrations.Model;
    using System.Data.Entity.SqlServer;
    using System.Data.SqlClient;
    using System.Linq;

  

    internal sealed class Configuration : DbMigrationsConfiguration<MySpecialDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            SetSqlGenerator("System.Data.SqlClient", new AuditingMigrationSqlGenerator(this));            
        }
    }
}
