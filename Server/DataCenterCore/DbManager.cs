using System;
using System.Linq;
using System.Threading.Tasks;
using Iit.Fibertest.DatabaseLibrary.DbContexts;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.DataCenterCore
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
