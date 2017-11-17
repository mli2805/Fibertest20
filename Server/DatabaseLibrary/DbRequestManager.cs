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

        public OpticalEventsList GetOpticalEvents(int afterIndex)
        {
            var result = new OpticalEventsList() {Events = new List<OpticalEvent>()};
            try
            {
                var dbContext = new MySqlContext();
                result.Events = dbContext.OpticalEvents.Where(e => e.Id > afterIndex).ToList();
                return result;
            }
            catch (Exception e)
            {
                _logFile.AppendLine("GetOpticalEvents: " + e.Message);
                return result;
            }
        }

        public NetworkEventsList GetNetworkEvents(int afterIndex)
        {
            var result = new NetworkEventsList() {Events = new List<NetworkEvent>()};
            try
            {
                var dbContext = new MySqlContext();
                result.Events = dbContext.NetworkEvents.Where(e => e.Id > afterIndex).ToList();
                return result;
            }
            catch (Exception e)
            {
                _logFile.AppendLine("GetNetworkEvents: " + e.Message);
                return result;
            }
        }
    }
}
