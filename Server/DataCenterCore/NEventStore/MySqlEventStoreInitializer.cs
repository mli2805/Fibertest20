using System;
using Iit.Fibertest.UtilsLib;
using MySql.Data.MySqlClient;
using NEventStore;
using NEventStore.Persistence.Sql.SqlDialects;

namespace Iit.Fibertest.DataCenterCore
{
    public sealed class MySqlEventStoreInitializer : IEventStoreInitializer
    {
        public IStoreEvents Init(IMyLog logFile)
        {
            CreateDatabaseIfNotExists(logFile);
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
                return null;
            }
        }

        private void CreateDatabaseIfNotExists(IMyLog logFile)
        {
            try
            {
                MySqlConnection connection = new MySqlConnection("server=localhost;user id=root;password=root;");
                MySqlCommand command = new MySqlCommand("create database if not exists ft20graph;", connection);
                connection.Open();
                command.ExecuteNonQuery();
                connection.Close();
            }
            catch (Exception e)
            {
                logFile.AppendLine(e.Message);
                throw;
            }
        }
    }
}