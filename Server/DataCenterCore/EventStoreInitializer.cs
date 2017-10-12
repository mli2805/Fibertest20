using System;
using System.IO;
using System.Reflection;
using Iit.Fibertest.UtilsLib;
using NEventStore;
using NEventStore.Persistence.Sql.SqlDialects;

namespace Iit.Fibertest.DataCenterCore
{
    public sealed class EventStoreInitializer : IEventStoreInitializer
    {
        public IStoreEvents Init(IMyLog logFile)
        {
            var appPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var graphDbPath = Path.Combine(appPath, @"..\Db\graph.db");
            logFile.AppendLine($@"Graph Db : {graphDbPath}");

            try
            {
                var storeEvents = Wireup.Init()
                    .UsingSqlPersistence("NEventStoreSQLite", "System.Data.SQLite", $@"Data Source={graphDbPath};Version=3;New=True;")
                    .WithDialect(new SqliteDialect())
                    .InitializeStorageEngine()
                    .Build();

                logFile.AppendLine(@"EventStoreService initialized successfully");
                return storeEvents;
            }
            catch (Exception e)
            {
                logFile.AppendLine(e.Message);
                return null;
            }
        }
    }
}