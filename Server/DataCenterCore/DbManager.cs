using System.Data.SQLite;
using System.IO;
using System.Reflection;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.DataCenterCore
{


    public class DbInitializer
    {
        private readonly IMyLog _logFile;
        private string _dbPath;

        public DbInitializer(IMyLog logFile)
        {
            _logFile = logFile;
        }


        private void Create()
        {
            SQLiteConnection.CreateFile(_dbPath);
        }
        public void Init()
        {
            var appPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            _dbPath = appPath == null ? @"c:\fibertest.sqlite3" : Path.Combine(appPath, @"..\Db\fibertest.sqlite3");
            _logFile.AppendLine($@"Db : {_dbPath}");

            var connectionString = $@"Data Source={_dbPath}; Version=3; FailIfMissing=True; Foreign Keys=True;";

            if (!File.Exists(_dbPath))
                Create();    
        }
    }

    public class DbManager
    {
        private readonly IniFile _iniFile;
        private readonly IMyLog _logFile;

        public DbManager(IniFile iniFile, IMyLog logFile)
        {
            _iniFile = iniFile;
            _logFile = logFile;
        }

        public void Apply(AssignBaseRef cmd)
        {
          
        }

        public ReturnCode CheckUserPassword(string username, string password)
        {

            return ReturnCode.ClientRegisteredSuccessfully;
        }
    }
}
