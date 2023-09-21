using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;


namespace Iit.Fibertest.DatabaseLibrary
{
    public class SorFileRepository
    {
        private readonly IParameterizer _parameterizer;
        private readonly IMyLog _logFile;

        public SorFileRepository(IParameterizer parameterizer, IMyLog logFile)
        {
            _parameterizer = parameterizer;
            _logFile = logFile;
        }

        public async Task<int> AddSorBytesAsync(byte[] sorBytes)
        {
            try
            {
                using (var dbContext = new FtDbContext(_parameterizer.Options))
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
                using (var dbContext = new FtDbContext(_parameterizer.Options))
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

        public async Task<string> UpdateSorBytesAsync(int sorFileId, byte[] sorBytes)
        {
            try
            {
                using (var dbContext = new FtDbContext(_parameterizer.Options))
                {
                    var record = await dbContext.SorFiles.Where(s => s.Id == sorFileId).FirstOrDefaultAsync();
                    record.SorBytes = sorBytes;
                    await dbContext.SaveChangesAsync();
                    return null;
                }
            }
            catch (Exception e)
            {
                _logFile.AppendLine("UpdateSorBytesAsync: " + e.Message);
                return e.Message;
            }
        }

        public async Task<byte[]> GetSorBytesAsync(int sorFileId)
        {
            try
            {
                using (var dbContext = new FtDbContext(_parameterizer.Options))
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
                using (var dbContext = new FtDbContext(_parameterizer.Options))
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

        public async Task<int> RemoveByMySqlCommand(int[] sorIds)
        {
            await Task.Delay(0);
            StringBuilder sb = new StringBuilder();
            foreach (var id in sorIds)
                sb.Append($"DELETE FROM `sorfiles` WHERE `Id`='{id}';");

            try
            {
                using (MySqlConnection conn = new MySqlConnection(_parameterizer.MySqlConnectionString))
                {
                    conn.Open();
                    using (MySqlCommand command = new MySqlCommand(sb.ToString(), conn){CommandTimeout = 0})
                    {
                        return command.ExecuteNonQuery();
                    }

                }
            }
            catch (Exception e)
            {
                _logFile.AppendLine("RemoveByMySqlCommand: " + e.Message);
                return -1;
            }
        }

        public async Task<int> RemoveSorsByPortionsAsync(int[] sorIds)
        {
            var portionSize = 2000;
            var index = 0;
            var recordsAffected = 0;

            try
            {
                using (var dbContext = new FtDbContext(_parameterizer.Options))
                {
                    while (index * portionSize < sorIds.Length)
                    {
                        var currentPortionSize = (index + 1) * portionSize < sorIds.Length ? portionSize : sorIds.Length - index * portionSize;
                        var portion = new int[currentPortionSize];
                        Array.ConstrainedCopy(sorIds, index * portionSize, portion, 0, currentPortionSize);

                        var sors = dbContext.SorFiles.Where(s => portion.Contains(s.Id));
                        dbContext.SorFiles.RemoveRange(sors);
                        recordsAffected += await dbContext.SaveChangesAsync();
                        index++;
                        _logFile.AppendLine($"{recordsAffected} sor files removed");
                    }

                    return recordsAffected;
                }
            }
            catch (Exception e)
            {
                _logFile.AppendLine("RemoveBatchSorByPortions: " + e.Message);
                return -1;
            }

        }

        public async Task<int> RemoveManySorAsync(int[] sorIds)
        {
            try
            {
                using (var dbContext = new FtDbContext(_parameterizer.Options))
                {
                    var sors = dbContext.SorFiles.Where(s => sorIds.Contains(s.Id)); // does not occupy memory
                    dbContext.SorFiles.RemoveRange(sors); // Occupies memory !!!!!!!!!!!!!!!!!!
                    var recordsAffected = await dbContext.SaveChangesAsync();
                    _logFile.AppendLine($"{recordsAffected} sor files removed");
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