using System;
using Iit.Fibertest.UtilsLib;
using NEventStore;
using NEventStore.Persistence.Sql.SqlDialects;

namespace Iit.Fibertest.DataCenterCore
{
    public sealed class MySqlEventStoreInitializer : IEventStoreInitializer
    {
        public IStoreEvents Init(IMyLog logFile)
        {
            try
            {
                var eventStore = Wireup.Init()
                    .UsingSqlPersistence("Ft20graphMySql", "MySql.Data.MySqlClient", "server=localhost;user id=root;password=root;database=ft20graph")
                    .WithDialect(new MySqlDialect())
                    .InitializeStorageEngine()
                    .Build();

                logFile.AppendLine(@"EventStoreService initialized successfully");
                return eventStore;

            }
            catch (Exception e)
            {
                logFile.AppendLine("MySqlEventStoreInitializer exception : " + e.Message);
                logFile.AppendLine("Empty schema ft20graph need to be created manually beforehand");
                return null;
            }
        }
    }
}