using System;
using System.Linq;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;
using Microsoft.EntityFrameworkCore;

namespace Iit.Fibertest.DatabaseLibrary
{
    public class SorFileRepository
    {
        private readonly ISettings _settings;
        private readonly IMyLog _logFile;

        public SorFileRepository(ISettings settings, IMyLog logFile)
        {
            _settings = settings;
            _logFile = logFile;
        }

        public async Task<int> SaveSorBytesAsync(byte[] sorBytes)
        {
            try
            {
                using (var dbContext = new FtDbContext(_settings.Options))
                {
                    var sorFile = new SorFile() { SorBytes = sorBytes };
                    dbContext.SorFiles.Add(sorFile);
                    await dbContext.SaveChangesAsync();

                    return sorFile.Id;
                }
            }
            catch (Exception e)
            {
                _logFile.AppendLine("SaveSorBytesAsync: " + e.Message);
                return -1;
            }
        }

        public async Task<byte[]> GetSorBytesAsync(int sorFileId)
        {
            try
            {
                using (var dbContext = new FtDbContext(_settings.Options))
                {
                    var result = await dbContext.SorFiles.Where(s => s.Id == sorFileId).FirstOrDefaultAsync();
                    return result?.SorBytes;
                }
            }
            catch (Exception e)
            {
                _logFile.AppendLine("GetSorBytesAsync: " + e.Message);
                return null;
            }
        }

        public async Task<int> RemoveSorBytesAsync(int sorFileId)
        {
            try
            {
                using (var dbContext = new FtDbContext(_settings.Options))
                {
                    var result = await dbContext.SorFiles.Where(s => s.Id == sorFileId).FirstOrDefaultAsync();
                    dbContext.SorFiles.Remove(result);
                    return await dbContext.SaveChangesAsync();
                }
            }
            catch (Exception e)
            {
                _logFile.AppendLine("GetSorBytesAsync: " + e.Message);
                return -1;
            }
        }

        


    }
}