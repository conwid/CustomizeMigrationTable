using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Migrations;
using System.Data.Entity.Migrations.History;
using System.Data.Entity.Migrations.Model;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomizeMigrationTable
{
    public class AuditingMigrationSqlGenerator : SqlServerMigrationSqlGenerator
    {
        private readonly DbMigrationsConfiguration config;
        private readonly string historyTableName;
        public AuditingMigrationSqlGenerator(DbMigrationsConfiguration config)
        {
            this.config = config;
            this.historyTableName = GetHistoryTableName();
        }

        private string GetHistoryTableName()
        {
            using (var originalCtx = (DbContext)Activator.CreateInstance(config.ContextType))
            using (var histCtx = config.GetHistoryContextFactory("System.Data.SqlClient")(originalCtx.Database.Connection, null))
            {
                var metadata = ((IObjectContextAdapter)histCtx).ObjectContext.MetadataWorkspace;
                var objectItemCollection = ((ObjectItemCollection)metadata.GetItemCollection(DataSpace.OSpace));

                // Get the entity type from the model that maps to the CLR type
                var entityType = metadata
                        .GetItems<EntityType>(DataSpace.OSpace)
                        .Single(e => typeof(HistoryRow).IsAssignableFrom(objectItemCollection.GetClrType(e)));

                // Get the entity set that uses this entity type
                var entitySet = metadata
                    .GetItems<EntityContainer>(DataSpace.CSpace)
                    .Single()
                    .EntitySets
                    .Single(s => s.ElementType.Name == entityType.Name);

                // Find the mapping between conceptual and storage model for this entity set
                var mapping = metadata.GetItems<EntityContainerMapping>(DataSpace.CSSpace)
                        .Single()
                        .EntitySetMappings
                        .Single(s => s.EntitySet == entitySet);

                // Find the storage entity set (table) that the entity is mapped
                var table = mapping
                    .EntityTypeMappings.Single()
                    .Fragments.Single()
                    .StoreEntitySet;

                // Return the table name from the storage entity set
                return string.Join(".",
                       (string)table.MetadataProperties["Schema"].Value ?? "dbo",
                       (string)table.MetadataProperties["Table"].Value ?? table.Name
                    );
            }
        }

        private bool IsHistoryTable(CreateTableOperation createTableOperation)
        {
            return createTableOperation.Name == historyTableName;            
        }

        private CreateTableOperation AddCustomHistoryTableColumns(CreateTableOperation createTableOperation)
        {
            createTableOperation.Columns.Add(
                new ColumnModel(PrimitiveTypeKind.DateTime)
                {
                    Name = "MigratingUser",
                    DefaultValueSql = "CURRENT_USER"
                });
            createTableOperation.Columns.Add(
                new ColumnModel(PrimitiveTypeKind.String)
                {
                    Name = "MigrationDate",
                    DefaultValueSql = "GETDATE()"
                });
            return createTableOperation;
        }

        protected override void Generate(CreateTableOperation createTableOperation)
        {
            if (IsHistoryTable(createTableOperation))
            {
                AddCustomHistoryTableColumns(createTableOperation);
            }
            base.Generate(createTableOperation);
        }
    }
}
