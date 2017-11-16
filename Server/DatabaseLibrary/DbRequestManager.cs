using System;
using System.Collections.Generic;
using System.Linq;
using Iit.Fibertest.DatabaseLibrary.DbContexts;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.DatabaseLibrary
{
    public class DbRequestManager
    {
        private readonly IMyLog _logFile;

        public DbRequestManager(IMyLog logFile)
        {
            _logFile = logFile;
        }

        public List<OpticalEvent> GetOpticalEvents(int afterIndex)
        {
            try
            {
                var dbContext = new MySqlContext();
                var opticalEvents = dbContext.OpticalEvents.Where(e => e.Id > afterIndex).ToList();
                return opticalEvents;
            }
            catch (Exception e)
            {
                _logFile.AppendLine("GetOpticalEvents: " + e.Message);
                return new List<OpticalEvent>();
            }
        }
    }
}
