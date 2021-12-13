using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;
using Newtonsoft.Json;

namespace Iit.Fibertest.WcfConnections
{
    public class DesktopC2DWcfManager : IWcfServiceDesktopC2D
    {
        private static readonly JsonSerializerSettings JsonSerializerSettings =
            new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            };

        private readonly IniFile _iniFile;
        private readonly IMyLog _logFile;
        private string _username;
        private string _clientIp;
        private WcfFactory _wcfFactory;

        public DesktopC2DWcfManager(IniFile iniFile, IMyLog logFile)
        {
            _iniFile = iniFile;
            _logFile = logFile;
        }

        public IWcfServiceDesktopC2D SetServerAddresses(DoubleAddress newServerAddress, string username, string clientIp)
        {
            _wcfFactory = new WcfFactory(newServerAddress, _iniFile, _logFile);
            _username = username;
            _clientIp = clientIp;
            return this;
        }

        public async Task<bool> SendHeartbeat(HeartbeatDto dto)
        {
            var wcfConnection = _wcfFactory.GetDesktopC2DChannelFactory();
            if (wcfConnection == null)
                return false;

            try
            {
                var channel = wcfConnection.CreateChannel();
                dto.ClientIp = _clientIp;
                var result = await channel.SendHeartbeat(dto);
                wcfConnection.Close();
                return result;
            }
            catch (Exception e)
            {
                _logFile.AppendLine("SendHeartbeat: " + e.Message);
                return false;
            }
        }

        public async Task<int> SendCommandsAsObjs(List<object> cmds)
        {
            var list = new List<string>();
            foreach (var cmd in cmds)
            {
                list.Add(JsonConvert.SerializeObject(cmd, cmd.GetType(), JsonSerializerSettings));
            }

            return await SendCommands(list, _username, _clientIp);
        }

        public async Task<int> SendCommands(List<string> jsons, string username, string clientIp)
        {
            var wcfConnection = _wcfFactory.GetDesktopC2DChannelFactory();
            if (wcfConnection == null)
                return -1;

            try
            {
                var channel = wcfConnection.CreateChannel();
                var result = await channel.SendCommands(jsons, username, clientIp);
                wcfConnection.Close();
                return result;
            }
            catch (Exception e)
            {
                _logFile.AppendLine("SendCommands: " + e.Message);
                return -1;
            }
        }

        public async Task<int> SendMeas(List<AddMeasurementFromOldBase> list)
        {
            var wcfConnection = _wcfFactory.GetDesktopC2DChannelFactory();
            if (wcfConnection == null)
                return -1;

            try
            {
                var channel = wcfConnection.CreateChannel();
                var result = await channel.SendMeas(list);
                wcfConnection.Close();
                return result;
            }
            catch (Exception e)
            {
                _logFile.AppendLine("SendMeas: " + e.Message);
                return -1;
            }
        }

        public async Task<string> SendCommandAsObj(object cmd)
        {
            return await SendCommand(JsonConvert.SerializeObject(cmd, cmd.GetType(), JsonSerializerSettings), _username, _clientIp);
        }

        public async Task<string> SendCommand(string serializedCmd, string username, string clientIp)
        {
            var wcfConnection = _wcfFactory.GetDesktopC2DChannelFactory();
            if (wcfConnection == null)
                return @"Cannot establish data-center connection.";

            try
            {
                var channel = wcfConnection.CreateChannel();
                var result = await channel.SendCommand(serializedCmd, username, clientIp);
                wcfConnection.Close();
                return result;
            }
            catch (Exception e)
            {
                _logFile.AppendLine("SendCommand: " + e.Message);
                return e.Message;
            }
        }

        public async Task<string[]> GetEvents(GetEventsDto dto)
        {
            var wcfConnection = _wcfFactory.GetDesktopC2DChannelFactory();
            if (wcfConnection == null)
                return null;

            try
            {
                var channel = wcfConnection.CreateChannel();
                dto.ClientIp = _clientIp;
                var result = await channel.GetEvents(dto);
                wcfConnection.Close();
                return result;
            }
            catch (Exception e)
            {
                _logFile.AppendLine("GetEvents: " + e.Message);
                return null;
            }
        }

        public async Task<SerializedModelDto> GetModelDownloadParams(GetSnapshotDto dto)
        {
            var wcfConnection = _wcfFactory.GetDesktopC2DChannelFactory();
            if (wcfConnection == null)
                return null;

            try
            {
                var channel = wcfConnection.CreateChannel();
                dto.ClientIp = _clientIp;
                var result = await channel.GetModelDownloadParams(dto);
                wcfConnection.Close();
                return result;
            }
            catch (Exception e)
            {
                _logFile.AppendLine("GetModelDownloadParams: " + e.Message);
                return null;
            }
        }

        public async Task<byte[]> GetModelPortion(int portionOrdinal)
        {
            var wcfConnection = _wcfFactory.GetDesktopC2DChannelFactory();
            if (wcfConnection == null)
                return null;

            try
            {
                var channel = wcfConnection.CreateChannel();
                var result = await channel.GetModelPortion(portionOrdinal);
                wcfConnection.Close();
                return result;
            }
            catch (Exception e)
            {
                _logFile.AppendLine("GetModelPortion: " + e.Message);
                return null;
            }
        }

        public async Task<bool> CheckServerConnection(CheckServerConnectionDto dto)
        {
            var wcfConnection = _wcfFactory.GetDesktopC2DChannelFactory();
            if (wcfConnection == null)
                return false;

            try
            {
                dto.ClientIp = _clientIp;
                var channel = wcfConnection.CreateChannel();
                var result = await channel.CheckServerConnection(dto);
                wcfConnection.Close();
                return result;
            }
            catch (Exception e)
            {
                _logFile.AppendLine("CheckServerConnection: " + e.Message);
                return false;
            }
        }

        public async Task<bool> SaveSmtpSettings(SmtpSettingsDto dto)
        {
            var wcfConnection = _wcfFactory.GetDesktopC2DChannelFactory();
            if (wcfConnection == null)
                return false;

            try
            {
                var channel = wcfConnection.CreateChannel();
                var result = await channel.SaveSmtpSettings(dto);
                wcfConnection.Close();
                return result;
            }
            catch (Exception e)
            {
                _logFile.AppendLine("SaveSmtpSettings: " + e.Message);
                return false;
            }
        }
        public async Task<bool> SaveAndTestSnmpSettings(SnmpSettingsDto dto)
        {
            var wcfConnection = _wcfFactory.GetDesktopC2DChannelFactory();
            if (wcfConnection == null)
                return false;

            try
            {
                var channel = wcfConnection.CreateChannel();
                var result = await channel.SaveAndTestSnmpSettings(dto);
                wcfConnection.Close();
                return result;
            }
            catch (Exception e)
            {
                _logFile.AppendLine("SaveSnmpSettings: " + e.Message);
                return false;
            }
        }

        public async Task<bool> SaveGisMode(bool isMapVisible)
        {
            var wcfConnection = _wcfFactory.GetDesktopC2DChannelFactory();
            if (wcfConnection == null)
                return false;

            try
            {
                var channel = wcfConnection.CreateChannel();
                var result = await channel.SaveGisMode(isMapVisible);
                wcfConnection.Close();
                return result;
            }
            catch (Exception e)
            {
                _logFile.AppendLine("SaveGisMode: " + e.Message);
                return false;
            }
        }

        public async Task<bool> SaveGsmComPort(int comPort)
        {
            var wcfConnection = _wcfFactory.GetDesktopC2DChannelFactory();
            if (wcfConnection == null)
                return false;

            try
            {
                var channel = wcfConnection.CreateChannel();
                var result = await channel.SaveGsmComPort(comPort);
                wcfConnection.Close();
                return result;
            }
            catch (Exception e)
            {
                _logFile.AppendLine("SaveGsmComPort: " + e.Message);
                return false;
            }
        }

        public async Task<bool> SendTest(string to, NotificationType notificationType)
        {
            var wcfConnection = _wcfFactory.GetDesktopC2DChannelFactory();
            if (wcfConnection == null)
                return false;

            try
            {
                var channel = wcfConnection.CreateChannel();
                var result = await channel.SendTest(to, notificationType);
                wcfConnection.Close();
                return result;
            }
            catch (Exception e)
            {
                _logFile.AppendLine("SendTestToUser: " + e.Message);
                return false;
            }
        }

        public async Task<DiskSpaceDto> GetDiskSpaceGb()
        {
            var wcfConnection = _wcfFactory.GetDesktopC2DChannelFactory();
            if (wcfConnection == null)
                return null;

            try
            {
                var channel = wcfConnection.CreateChannel();
                var result = await channel.GetDiskSpaceGb();
                wcfConnection.Close();
                return result;
            }
            catch (Exception e)
            {
                _logFile.AppendLine("SendTestToUser: " + e.Message);
                return null;
            }
        }



    }
}
