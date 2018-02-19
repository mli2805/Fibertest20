using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;
using Microsoft.EntityFrameworkCore;

namespace Iit.Fibertest.DatabaseLibrary
{
    public class ZonesRepository
    {
        private readonly ISettings _settings;
        private readonly IMyLog _logFile;

        public ZonesRepository(ISettings settings, IMyLog logFile)
        {
            _settings = settings;
            _logFile = logFile;
        }

        public async Task<List<Zone>> GetZonesAsync()
        {
            using (var dbContext = new FtDbContext(_settings.Options))
            {
                try
                {
                    return await dbContext.Zones.ToListAsync();
                }
                catch (Exception e)
                {
                    _logFile.AppendLine("GetZonesAsync:" + e.Message);
                    return null;
                }
            }
        }
    }
}