using System;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Iit.Fibertest.DbLibrary.DbContexts;
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
        private readonly IFibertestDbContext _dbContext;

        public DbManager(IniFile iniFile, IMyLog logFile, IFibertestDbContext dbContext)
        {
            _iniFile = iniFile;
            _logFile = logFile;
            _dbContext = dbContext;
        }

        public void Apply(AssignBaseRef cmd)
        {

        }

        public Task<ClientRegisteredDto> CheckUserPassword(RegisterClientDto dto)
        {
            var result = new ClientRegisteredDto();

            try
            {
                var users = _dbContext.Users.ToList();
                if (users.FirstOrDefault(u => u.Name == dto.UserName && u.Password == dto.Password) == null)
                {
                    result.ReturnCode = ReturnCode.NoSuchUserOrWrongPassword;
                    return Task.FromResult(result);
                }
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
                result.ReturnCode = ReturnCode.DbError;
                result.ExceptionMessage = e.Message;
                return Task.FromResult(result);
            }

            result.ReturnCode = ReturnCode.ClientRegisteredSuccessfully;
            return Task.FromResult(result);
        }
    }
}
