using System;
using Dto;
using Iit.Fibertest.UtilsLib;

namespace WcfConnections
{
    public class R2DWcfManager
    {
        private readonly LogFile _logFile;
        private readonly WcfFactory _wcfFactory;

        public R2DWcfManager(DoubleAddressWithLastConnectionCheck dataCenterAddress, IniFile iniFile, LogFile logFile)
        {
            _logFile = logFile;
            _wcfFactory = new WcfFactory(dataCenterAddress, iniFile, _logFile);
        }

        public void SendCurrentState(RtuConnectionCheckedDto dto)
        {
            var wcfConnection = _wcfFactory.CreateR2DConnection();
            if (wcfConnection == null)
                return;

            try
            {
                wcfConnection.ProcessRtuConnectionChecked(dto);
                _logFile.AppendLine(@"Sent RTU's current monitoringStep");
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
            }
        }

        public void SendInitializationConfirm(RtuInitializedDto rtu)
        {
            var wcfConnection = _wcfFactory.CreateR2DConnection();
            if (wcfConnection == null)
                return;

            try
            {
                wcfConnection.ProcessRtuInitialized(rtu);
                _logFile.AppendLine("Sent initializatioln result to server...");
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
            }
        }

        public void SendMonitoringSettingsApplied(MonitoringSettingsAppliedDto result)
        {
            var wcfConnection = _wcfFactory.CreateR2DConnection();
            if (wcfConnection == null)
                return;

            try
            {
                wcfConnection.ConfirmMonitoringSettingsApplied(result);
                _logFile.AppendLine("Sending apply monitoring settings result");
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
            }
        }

        public void SendBaseRefAssigned(BaseRefAssignedDto result)
        {
            var wcfConnection = _wcfFactory.CreateR2DConnection();
            if (wcfConnection == null)
                return;

            try
            {
                wcfConnection.ConfirmBaseRefAssigned(result);
                _logFile.AppendLine("Sending assign base ref result");
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
            }

        }

        public void SendMonitoringStarted(MonitoringStartedDto result)
        {
            var wcfConnection = _wcfFactory.CreateR2DConnection();
            if (wcfConnection == null)
                return;

            try
            {
                wcfConnection.ConfirmStartMonitoring(result);
                _logFile.AppendLine("Sent result of start monitoring request");
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
            }
        }

        public void SendMonitoringStopped(MonitoringStoppedDto result)
        {
            var wcfConnection = _wcfFactory.CreateR2DConnection();
            if (wcfConnection == null)
                return;

            try
            {
                wcfConnection.ConfirmStopMonitoring(result);
                _logFile.AppendLine("Sending result of stop monitoring request");
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
            }
        }

        public void SendCurrentMonitoringStep(KnowRtuCurrentMonitoringStepDto monitoringStep)
        {
//            _logFile.AppendLine("Sending current monitoring step1");
            var wcfConnection = _wcfFactory.CreateR2DConnection();
//            _logFile.AppendLine("Sending current monitoring step2");
            if (wcfConnection == null)
                return;

            try
            {
                wcfConnection.KnowRtuCurrentMonitoringStep(monitoringStep);
//                _logFile.AppendLine("Sending current monitoring step3");
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
            }
        }

        public void CheckChannels()
        {
            
        }

        public bool SendMonitoringResult(MonitoringResultDto dto)
        {
            var wcfConnection = _wcfFactory.CreateR2DConnection();
            if (wcfConnection == null)
                return false;

            try
            {
                wcfConnection.ProcessMonitoringResult(dto);
                var port = dto.OtauPort.IsPortOnMainCharon 
                    ? dto.OtauPort.OpticalPort.ToString() 
                    : $"{dto.OtauPort.OpticalPort} on {dto.OtauPort.Ip}:{dto.OtauPort.TcpPort}";
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