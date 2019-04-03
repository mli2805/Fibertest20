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

        private readonly int _mysqlTcpPort;
        private readonly string _eventSourcingScheme;
        public string DataDir { get; private set; }
        public string ConnectionString { get; private set; }

        public MySqlEventStoreInitializer(IniFile iniFile, IMyLog logFile)
        {
            _logFile = logFile;
            _mysqlTcpPort = iniFile.Read(IniSection.MySql, IniKey.MySqlTcpPort, 3306);
            var postfix = iniFile.Read(IniSection.MySql, IniKey.MySqlDbSchemePostfix, "");
            _eventSourcingScheme = "ft20graph" + postfix;

            ConnectionString = $"server=localhost;port={_mysqlTcpPort};user id=root;password=root;";
        }

        public IStoreEvents Init()
        {
            CreateDatabaseIfNotExists();
            try
            {
                var eventStore = Wireup.Init()
                    .UsingSqlPersistence("Ft20graphMySql", "MySql.Data.MySqlClient", $"{ConnectionString}database={_eventSourcingScheme}")
                    .WithDialect(new MySqlDialect())
                    .InitializeStorageEngine()
                    .Build();

                _logFile.AppendLine($"Events store: MYSQL=localhost:{_mysqlTcpPort}   Database={_eventSourcingScheme}");

                InitializeDataDir();
                return eventStore;
            }
            catch (Exception e)
            {
                _logFile.AppendLine("MySqlEventStoreInitializer exception : " + e.Message);
                return null;
            }
        }

        private void InitializeDataDir()
        {
            MySqlConnection connection = new MySqlConnection(ConnectionString);
            MySqlCommand command = new MySqlCommand("select @@datadir", connection);
            connection.Open();
            DataDir = (string)command.ExecuteScalar();
            connection.Close();
            Thread.Sleep(TimeSpan.FromMilliseconds(100));
            _logFile.AppendLine($"MySQL data folder is {DataDir}");
        }

        public long GetDataSize()
        {
            try
            {
                MySqlConnection connection = new MySqlConnection(ConnectionString);
                MySqlCommand command = new MySqlCommand(
                    "SELECT SUM(data_length + index_length) FROM information_schema.tables WHERE table_schema = \"ft20efcore\"", connection);
                connection.Open();
                var result = (decimal)command.ExecuteScalar();
                connection.Close();
                Thread.Sleep(TimeSpan.FromMilliseconds(100));
                _logFile.AppendLine($"MySQL data size is {result}");
                return (long)result;
            }
            catch (Exception e)
            {
                _logFile.AppendLine("GetDataSize: " + e.Message);
                return -1;
            }
        }

        public void Delete()
        {
            try
            {
                MySqlConnection connection = new MySqlConnection(ConnectionString);
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
                MySqlConnection connection = new MySqlConnection(ConnectionString);
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