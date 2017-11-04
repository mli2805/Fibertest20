using System;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.WcfConnections
{
    public class R2DWcfManager
    {
        private readonly IniFile _iniFile;
        private readonly IMyLog _logFile;

        private readonly WcfFactory _wcfFactory;

        public R2DWcfManager(DoubleAddress dataCenterAddresses, IniFile iniFile, IMyLog logFile)
        {
            _iniFile = iniFile;
            _logFile = logFile;

            _wcfFactory = new WcfFactory(dataCenterAddresses, iniFile, _logFile);
        }

        public bool SendHeartbeat(RtuChecksChannelDto dto)
        {
            var wcfConnection = _wcfFactory.CreateR2DConnection(false);
            if (wcfConnection == null)
                return false;

            try
            {
                wcfConnection.RegisterRtuHeartbeat(dto);
                return true;
            }
            catch (Exception e)
            {
                _logFile.AppendLine("SendHeartbeat: " + e.Message);
                return false;
            }
        }

        public bool SendMonitoringResult(MonitoringResultDto dto)
        {
            var wcfConnection = _wcfFactory.CreateR2DConnection(false);
            if (wcfConnection == null)
                return false;

            try
            {
                wcfConnection.ProcessMonitoringResult(dto);
                var port = dto.OtauPort.IsPortOnMainCharon
                    ? dto.OtauPort.OpticalPort.ToString()
                    : $"{dto.OtauPort.OpticalPort} on {dto.OtauPort.OtauIp}:{dto.OtauPort.OtauTcpPort}";
                _logFile.AppendLine($"Sending {dto.BaseRefType} meas port {port} : {dto.TraceState}");
                return true;
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
                return false;
            }
        }
    }
}