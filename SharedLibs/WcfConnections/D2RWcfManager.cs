﻿using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.RtuWcfServiceInterface;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.WcfConnections
{
    public class D2RWcfManager
    {
        private readonly IMyLog _logFile;
        private readonly WcfFactory _wcfFactory;

        public D2RWcfManager(DoubleAddress rtuAddress, IniFile iniFile, IMyLog logFile)
        {
            _logFile = logFile;
            _wcfFactory = new WcfFactory(rtuAddress, iniFile, _logFile);
        }

        public async Task<RtuInitializedDto> InitializeAsync(InitializeRtuDto dto)
        {
            var backward = new RtuWcfServiceBackward();
            var rtuDuplexConnection = _wcfFactory.CreateDuplexRtuConnection(backward);
            if (rtuDuplexConnection == null)
                return null;

            var result = await rtuDuplexConnection.InitializeAsync(backward, dto);
            return result;
        }

        public async Task<bool> StartMonitoringAsync(StartMonitoringDto dto)
        {
            var backward = new RtuWcfServiceBackward();
            var rtuDuplexConnection = _wcfFactory.CreateDuplexRtuConnection(backward);
            if (rtuDuplexConnection == null)
                return false;

            var result = await rtuDuplexConnection.StartMonitoringAsync(backward, dto);
            return result;
        }

        public async Task<bool> StopMonitoringAsync(StopMonitoringDto dto)
        {
            var backward = new RtuWcfServiceBackward();
            var rtuDuplexConnection = _wcfFactory.CreateDuplexRtuConnection(backward);
            if (rtuDuplexConnection == null)
                return false;

            var result = await rtuDuplexConnection.StopMonitoringAsync(backward, dto);
            return result;
        }

        public async Task<bool> AssignBaseRefAsync(AssignBaseRefDto dto)
        {
            var backward = new RtuWcfServiceBackward();
            var rtuDuplexConnection = _wcfFactory.CreateDuplexRtuConnection(backward);
            if (rtuDuplexConnection == null)
                return false;

            // 
            _logFile.AppendLine("Still on server, duplex channel established");
            var result = await rtuDuplexConnection.AssignBaseRefAsync(backward, dto);
            return result;
        }

        public async Task<bool> ApplyMonitoringSettingsAsync(ApplyMonitoringSettingsDto dto)
        {
            var backward = new RtuWcfServiceBackward();
            var rtuDuplexConnection = _wcfFactory.CreateDuplexRtuConnection(backward);
            if (rtuDuplexConnection == null)
                return false;

            var result = await rtuDuplexConnection.ApplyMonitoringSettingsAsync(backward, dto);
            return result;
        }

    }
}
