using Iit.Fibertest.DatabaseLibrary.DbContexts;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.DatabaseLibrary
{

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

      
        
    }
}
