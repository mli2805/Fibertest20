using System;
using Dto;
using Iit.Fibertest.Utils35;

namespace WcfConnections
{
    public class R2DWcfManager
    {
        private readonly Logger35 _logger35;
        private readonly WcfFactory _wcfFactory;

        public R2DWcfManager(string dataCenterAddress, IniFile iniFile, Logger35 logger35)
        {
            _logger35 = logger35;
            _wcfFactory = new WcfFactory(dataCenterAddress, iniFile, _logger35);
        }

        public void SendCurrentState(RtuConnectionCheckedDto dto)
        {
            var wcfConnection = _wcfFactory.CreateR2DConnection();
            if (wcfConnection == null)
                return;

            try
            {
                wcfConnection.ProcessRtuConnectionChecked(dto);
                _logger35.AppendLine(@"Sent RTU's current monitoringStep");
            }
            catch (Exception e)
            {
                _logger35.AppendLine(e.Message);
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
                _logger35.AppendLine("Sent initializatioln result to server...");
            }
            catch (Exception e)
            {
                _logger35.AppendLine(e.Message);
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
                _logger35.AppendLine("Sending apply monitoring settings result");
            }
            catch (Exception e)
            {
                _logger35.AppendLine(e.Message);
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
                _logger35.AppendLine("Sending assign base ref result");
            }
            catch (Exception e)
            {
                _logger35.AppendLine(e.Message);
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
                _logger35.AppendLine("Sent result of start monitoring request");
            }
            catch (Exception e)
            {
                _logger35.AppendLine(e.Message);
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
                _logger35.AppendLine("Sending result of stop monitoring request");
            }
            catch (Exception e)
            {
                _logger35.AppendLine(e.Message);
            }
        }

        public void SendCurrentMonitoringStep(KnowRtuCurrentMonitoringStepDto monitoringStep)
        {
            var wcfConnection = _wcfFactory.CreateR2DConnection();
            if (wcfConnection == null)
                return;

            try
            {
                wcfConnection.KnowRtuCurrentMonitoringStep(monitoringStep);
//                _logger35.AppendLine("Sending current monitoring step");
            }
            catch (Exception e)
            {
                _logger35.AppendLine(e.Message);
            }
        }

        public void SendMonitoringResult(SaveMonitoringResultDto dto)
        {
            var wcfConnection = _wcfFactory.CreateR2DConnection();
            if (wcfConnection == null)
                return;

            try
            {
                wcfConnection.ProcessMonitoringResult(dto);
                _logger35.AppendLine("Sending monitoring result");
            }
            catch (Exception e)
            {
                _logger35.AppendLine(e.Message);
            }
        }
    }
}