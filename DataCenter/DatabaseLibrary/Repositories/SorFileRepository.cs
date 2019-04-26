using System;
using System.Collections.Generic;
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

        public async Task<int> AddSorBytesAsync(byte[] sorBytes)
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
                _logFile.AppendLine("AddSorBytesAsync: " + e.Message);
                return -1;
            }
        }

        public async Task<List<int>> AddMultipleSorBytesAsync(List<byte[]> sors)
        {
            try
            {
                using (var dbContext = new FtDbContext(_settings.Options))
                {
                    var sorFiles = sors.Select(s => new SorFile() { SorBytes = s }).ToList();
                    dbContext.SorFiles.AddRange(sorFiles);
                    await dbContext.SaveChangesAsync();

                    return sorFiles.Select(s => s.Id).ToList();
                }
            }
            catch (Exception e)
            {
                _logFile.AppendLine("AddMultipleSorBytesAsync: " + e.Message);
                return null;
            }
        }

        public async Task<int> UpdateSorBytesAsync(int sorFileId, byte[] sorBytes)
        {
            try
            {
                using (var dbContext = new FtDbContext(_settings.Options))
                {
                    var record = await dbContext.SorFiles.Where(s => s.Id == sorFileId).FirstOrDefaultAsync();
                    record.SorBytes = sorBytes;
                    return await dbContext.SaveChangesAsync();
                }
            }
            catch (Exception e)
            {
                _logFile.AppendLine("UpdateSorBytesAsync: " + e.Message);
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
                _logFile.AppendLine("RemoveSorBytesAsync: " + e.Message);
                return -1;
            }
        }

        public async Task<int> RemoveManySorAsync(int[] sorIds)
        {
            try
            {
                using (var dbContext = new FtDbContext(_settings.Options))
                {
                    var sors = dbContext.SorFiles.Where(s => sorIds.Contains(s.Id)).ToList();
                    dbContext.SorFiles.RemoveRange(sors);
                    var recordsAffected = await dbContext.SaveChangesAsync();
                    return recordsAffected;
                }
            }
            catch (Exception e)
            {
                _logFile.AppendLine("RemoveManySorAsync: " + e.Message);
                return -1;
            }
        }

    }
}