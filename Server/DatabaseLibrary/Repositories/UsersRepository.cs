using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;
using Microsoft.EntityFrameworkCore;

namespace Iit.Fibertest.DatabaseLibrary
{
    public class UsersRepository
    {
        private readonly ISettings _settings;
        private readonly IMyLog _logFile;

        public UsersRepository(ISettings settings, IMyLog logFile)
        {
            _settings = settings;
            _logFile = logFile;
        }

        public async Task<List<User>> GetUsersAsync()
        {
            using (var dbContext = new FtDbContext(_settings.Options))
            {
                try
                {
                    return await dbContext.Users.ToListAsync();
                }
                catch (Exception e)
                {
                    _logFile.AppendLine("GetUsersAsync:" + e.Message);
                    return null;
                }
            }
        }
    }
}