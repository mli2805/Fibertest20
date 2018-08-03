using System;
using System.Threading;
using Iit.Fibertest.UtilsLib;
using MySql.Data.MySqlClient;
using NEventStore;
using NEventStore.Persistence.Sql.SqlDialects;

namespace Iit.Fibertest.DataCenterCore
{
    public sealed class MySqlEventStoreInitializer : IEventStoreInitializer
    {
        private readonly IMyLog _logFile;

        private int _mysqlTcpPort;
        private string _eventSourcingScheme;

        public MySqlEventStoreInitializer(IniFile iniFile, IMyLog logFile)
        {
            _logFile = logFile;
            _mysqlTcpPort = iniFile.Read(IniSection.MySql, IniKey.MySqlTcpPort, 3306);
            var postfix = iniFile.Read(IniSection.MySql, IniKey.MySqlDbSchemePostfix, "");
            _eventSourcingScheme = "ft20graph" + postfix;
        }


        public IStoreEvents Init()
        {
            CreateDatabaseIfNotExists();
            try
            {
                var eventStore = Wireup.Init()
                    .UsingSqlPersistence("Ft20graphMySql", "MySql.Data.MySqlClient", $"server=localhost;port={_mysqlTcpPort};user id=root;password=root;database={_eventSourcingScheme}")
                    .WithDialect(new MySqlDialect())
                    .InitializeStorageEngine()
                    .Build();

                _logFile.AppendLine($"Events store: MYSQL=localhost:{_mysqlTcpPort}   Database={_eventSourcingScheme}");
                return eventStore;
            }
            catch (Exception e)
            {
                _logFile.AppendLine("MySqlEventStoreInitializer exception : " + e.Message);
                return null;
            }
        }

        public void Delete()
        {
            try
            {
                MySqlConnection connection = new MySqlConnection($"server=localhost;port={_mysqlTcpPort};user id=root;password=root;");
                MySqlCommand command = new MySqlCommand($"drop database if exists {_eventSourcingScheme};", connection);
                connection.Open();
                command.ExecuteNonQuery();
                connection.Close();
                Thread.Sleep(TimeSpan.FromMilliseconds(100));
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
                throw;
            }
        }

        private void CreateDatabaseIfNotExists()
        {
            try
            {
                MySqlConnection connection = new MySqlConnection($"server=localhost;port={_mysqlTcpPort};user id=root;password=root;");
                MySqlCommand command = new MySqlCommand($"create database if not exists {_eventSourcingScheme};", connection);
                connection.Open();
                command.ExecuteNonQuery();
                connection.Close();
                Thread.Sleep(TimeSpan.FromMilliseconds(100));
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
                throw;
            }
        }
    }
}