using System;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.WcfConnections
{
    public class WebC2DWcfManager : IWcfServiceWebC2D
    {
        private readonly IniFile _iniFile;
        private readonly IMyLog _logFile;
        private WcfFactory _wcfFactory;

        public WebC2DWcfManager(IniFile iniFile, IMyLog logFile)
        {
            _iniFile = iniFile;
            _logFile = logFile;
        }
        public WebC2DWcfManager SetServerAddresses(DoubleAddress newServerAddress, string username, string clientIp)
        {
            _wcfFactory = new WcfFactory(newServerAddress, _iniFile, _logFile);
            return this;
        }

        public async Task<string> GetAboutInJson(string username)
        {
            var wcfConnection = _wcfFactory.GetWebC2DChannelFactory();
            if (wcfConnection == null)
                return null;

            try
            {
                var channel = wcfConnection.CreateChannel();
                var result = await channel.GetAboutInJson(username);
                wcfConnection.Close();
                return result;
            }
            catch (Exception e)
            {
                _logFile.AppendLine("GetAboutInJson: " + e.Message);
                return null;
            }
        }

        public async Task<string> GetTreeInJson(string username)
        {
            var wcfConnection = _wcfFactory.GetWebC2DChannelFactory();
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
            var wcfConnection = _wcfFactory.GetWebC2DChannelFactory();
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
            var wcfConnection = _wcfFactory.GetWebC2DChannelFactory();
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
            var wcfConnection = _wcfFactory.GetWebC2DChannelFactory();
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

        public async Task<TreeOfAcceptableMeasParams> GetRtuAcceptableMeasParams(string username, Guid rtuId)
        {
            var wcfConnection = _wcfFactory.GetWebC2DChannelFactory();
            if (wcfConnection == null)
                return null;

            try
            {
                var channel = wcfConnection.CreateChannel();
                var result = await channel.GetRtuAcceptableMeasParams(username, rtuId);
                wcfConnection.Close();
                return result;
            }
            catch (Exception e)
            {
                _logFile.AppendLine("GetRtuAcceptableMeasParams: " + e.Message);
                return null;
            }
        }

        public async Task<RtuMonitoringSettingsDto> GetRtuMonitoringSettings(string username, Guid rtuId)
        {
            var wcfConnection = _wcfFactory.GetWebC2DChannelFactory();
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

        public async Task<RtuInitializedDto> InitializeRtuAsync(InitializeRtuDto dto)
        {
            var wcfConnection = _wcfFactory.GetWebC2DChannelFactory();
            if (wcfConnection == null)
                return null;

            try
            {
                _logFile.AppendLine($@"Sending command to initialize RTU {dto.RtuId.First6()}");
                var channel = wcfConnection.CreateChannel();
                var result = await channel.InitializeRtuAsync(dto);
                wcfConnection.Close();
                return result;
            }
            catch (Exception e)
            {
                _logFile.AppendLine("InitializeRtuAsync: " + e.Message);
                return null;
            }
        }

        #endregion

        #region Trace
        public async Task<TraceInformationDto> GetTraceInformation(string username, Guid traceId)
        {
            var wcfConnection = _wcfFactory.GetWebC2DChannelFactory();
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

        public async Task<TraceStatisticsDto> GetTraceStatistics(string username, Guid traceId, int pageNumber, int pageSize)
        {
            var wcfConnection = _wcfFactory.GetWebC2DChannelFactory();
            if (wcfConnection == null)
                return null;

            try
            {
                var channel = wcfConnection.CreateChannel();
                var result = await channel.GetTraceStatistics(username, traceId, pageNumber, pageSize);
                wcfConnection.Close();
                return result;
            }
            catch (Exception e)
            {
                _logFile.AppendLine("GetTraceStatistics: " + e.Message);
                return null;
            }
        }

        public async Task<TraceStateDto> GetTraceState(string username, string requestBody)
        {
            var wcfConnection = _wcfFactory.GetWebC2DChannelFactory();
            if (wcfConnection == null)
                return null;

            try
            {
                var channel = wcfConnection.CreateChannel();
                var result = await channel.GetTraceState(username, requestBody);
                wcfConnection.Close();
                return result;
            }
            catch (Exception e)
            {
                _logFile.AppendLine("GetTraceStatistics: " + e.Message);
                return null;
            }
        }

        public async Task<AssignBaseParamsDto> GetAssignBaseParams(string username, Guid traceId)
        {
            var wcfConnection = _wcfFactory.GetWebC2DChannelFactory();
            if (wcfConnection == null)
                return null;

            try
            {
                var channel = wcfConnection.CreateChannel();
                var result = await channel.GetAssignBaseParams(username, traceId);
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

        public async Task<OpticalEventsRequestedDto> GetOpticalEventPortion(string username, bool isCurrentEvents = true, string filterRtu = "",
            string filterTrace = "", string sortOrder = "desc", int pageNumber = 0, int pageSize = 100)
        {
            var wcfConnection = _wcfFactory.GetWebC2DChannelFactory();
            if (wcfConnection == null)
                return null;

            try
            {
                var channel = wcfConnection.CreateChannel();
                var result = await channel.GetOpticalEventPortion(username, isCurrentEvents, filterRtu, filterTrace, sortOrder, pageNumber, pageSize);
                wcfConnection.Close();
                return result;
            }
            catch (Exception e)
            {
                _logFile.AppendLine("GetOptGetOpticalEventPortionicalEventList: " + e.Message);
                return null;
            }
        }
    }
}