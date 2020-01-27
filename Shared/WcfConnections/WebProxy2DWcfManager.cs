﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfServiceForWebProxyInterface;

namespace Iit.Fibertest.WcfConnections
{
    public class WebProxy2DWcfManager : IWcfServiceForWebProxy
    {
        private readonly IniFile _iniFile;
        private readonly IMyLog _logFile;
        private WcfFactory _wcfFactory;

        public WebProxy2DWcfManager(IniFile iniFile, IMyLog logFile)
        {
            _iniFile = iniFile;
            _logFile = logFile;
        }
        public void SetServerAddresses(DoubleAddress newServerAddress, string username, string clientIp)
        {
            _wcfFactory = new WcfFactory(newServerAddress, _iniFile, _logFile);
        }

        public async Task<UserDto> LoginWebClient(string username, string password)
        {
            var wcfConnection = _wcfFactory.GetWebProxy2DChannelFactory();
            if (wcfConnection == null)
                return null;

            try
            {
                var channel = wcfConnection.CreateChannel();
                var result = await channel.LoginWebClient(username, password);
                wcfConnection.Close();
                return result;
            }
            catch (Exception e)
            {
                _logFile.AppendLine("LoginWebClient: " + e.Message);
                return null;
            }
        }

        public async Task<string> GetTreeInJson(string username)
        {
            var wcfConnection = _wcfFactory.GetWebProxy2DChannelFactory();
            if (wcfConnection == null)
                return null;

            try
            {
                var channel = wcfConnection.CreateChannel();
                var result = await channel.GetTreeInJson(username);
                wcfConnection.Close();
                return result;
            }
            catch (Exception e)
            {
                _logFile.AppendLine("GetTreeInJson: " + e.Message);
                return null;
            }
        }

        #region RTU
        public async Task<RtuInformationDto> GetRtuInformation(string username, Guid rtuId)
        {
            var wcfConnection = _wcfFactory.GetWebProxy2DChannelFactory();
            if (wcfConnection == null)
                return null;

            try
            {
                var channel = wcfConnection.CreateChannel();
                var result = await channel.GetRtuInformation(username, rtuId);
                wcfConnection.Close();
                return result;
            }
            catch (Exception e)
            {
                _logFile.AppendLine("GetRtuInformation: " + e.Message);
                return null;
            }
        }
        
        public async Task<RtuNetworkSettingsDto> GetRtuNetworkSettings(string username, Guid rtuId)
        {
            var wcfConnection = _wcfFactory.GetWebProxy2DChannelFactory();
            if (wcfConnection == null)
                return null;

            try
            {
                var channel = wcfConnection.CreateChannel();
                var result = await channel.GetRtuNetworkSettings(username, rtuId);
                wcfConnection.Close();
                return result;
            }
            catch (Exception e)
            {
                _logFile.AppendLine("GetRtuNetworkSettings: " + e.Message);
                return null;
            }
        } 
        
        public async Task<RtuStateDto> GetRtuState(string username, Guid rtuId)
        {
            var wcfConnection = _wcfFactory.GetWebProxy2DChannelFactory();
            if (wcfConnection == null)
                return null;

            try
            {
                var channel = wcfConnection.CreateChannel();
                var result = await channel.GetRtuState(username, rtuId);
                wcfConnection.Close();
                return result;
            }
            catch (Exception e)
            {
                _logFile.AppendLine("GetRtuState: " + e.Message);
                return null;
            }
        }
        
        public async Task<RtuMonitoringSettingsDto> GetRtuMonitoringSettings(string username, Guid rtuId)
        {
            var wcfConnection = _wcfFactory.GetWebProxy2DChannelFactory();
            if (wcfConnection == null)
                return null;

            try
            {
                var channel = wcfConnection.CreateChannel();
                var result = await channel.GetRtuMonitoringSettings(username, rtuId);
                wcfConnection.Close();
                return result;
            }
            catch (Exception e)
            {
                _logFile.AppendLine("GetRtuMonitoringSettings: " + e.Message);
                return null;
            }
        }

        public async Task<MonitoringSettingsAppliedDto> PostRtuMonitoringSettings(string username, Guid id, RtuMonitoringSettingsDto dto)
        {
            var wcfConnection = _wcfFactory.GetWebProxy2DChannelFactory();
            if (wcfConnection == null)
                return null;

            try
            {
                var channel = wcfConnection.CreateChannel();
                var result = await channel.PostRtuMonitoringSettings(username, id, dto);
                wcfConnection.Close();
                return result;
            }
            catch (Exception e)
            {
                _logFile.AppendLine("PostRtuMonitoringSettings: " + e.Message);
                return null;
            }
        }

        #endregion

        #region Trace
        public async Task<TraceInformationDto> GetTraceInformation(string username, Guid traceId)
        {
            var wcfConnection = _wcfFactory.GetWebProxy2DChannelFactory();
            if (wcfConnection == null)
                return null;

            try
            {
                var channel = wcfConnection.CreateChannel();
                var result = await channel.GetTraceInformation(username, traceId);
                wcfConnection.Close();
                return result;
            }
            catch (Exception e)
            {
                _logFile.AppendLine("GetTraceInformation: " + e.Message);
                return null;
            }
        }

        public async Task<TraceStatisticsDto> GetTraceStatistics(string username, Guid traceId)
        {
            var wcfConnection = _wcfFactory.GetWebProxy2DChannelFactory();
            if (wcfConnection == null)
                return null;

            try
            {
                var channel = wcfConnection.CreateChannel();
                var result = await channel.GetTraceStatistics(username, traceId);
                wcfConnection.Close();
                return result;
            }
            catch (Exception e)
            {
                _logFile.AppendLine("GetTraceStatistics: " + e.Message);
                return null;
            }
        }
        #endregion

        public async Task<List<OpticalEventDto>> GetOpticalEventList(string username, bool isCurrentEvents = true, string filterRtu = "",
            string filterTrace = "", string sortOrder = "desc", int pageNumber = 0, int pageSize = 100)
        {
            var wcfConnection = _wcfFactory.GetWebProxy2DChannelFactory();
            if (wcfConnection == null)
                return null;

            try
            {
                var channel = wcfConnection.CreateChannel();
                var result = await channel.GetOpticalEventList(username, isCurrentEvents, filterRtu, filterTrace, sortOrder, pageNumber, pageSize);
                wcfConnection.Close();
                return result;
            }
            catch (Exception e)
            {
                _logFile.AppendLine("GetOpticalEventList: " + e.Message);
                return null;
            }
        }
    }
}