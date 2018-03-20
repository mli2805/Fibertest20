using System;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.WcfConnections
{
    public class R2DWcfManager
    {
        private readonly IMyLog _logFile;

        private readonly WcfFactory _wcfFactory;

        public R2DWcfManager(DoubleAddress dataCenterAddresses, IniFile iniFile, IMyLog logFile)
        {
            _logFile = logFile;
            _wcfFactory = new WcfFactory(dataCenterAddresses, iniFile, _logFile);
        }

        public bool SendHeartbeat(RtuChecksChannelDto dto)
        {
            var wcfConnection = _wcfFactory.GetR2DChannelFactory(false);
            if (wcfConnection == null)
                return false;

            try
            {
                var channel = wcfConnection.CreateChannel();
                channel.RegisterRtuHeartbeat(dto);
                wcfConnection.Close();
                return true;
            }
            catch (Exception e)
            {
                _logFile.AppendLine("SendHeartbeat: " + e.Message);
                return false;
            }
        }

        public void SendCurrentMonitoringStep(CurrentMonitoringStepDto dto)
        {
            var wcfConnection = _wcfFactory.GetR2DChannelFactory(false);
            if (wcfConnection == null)
                return ;

            try
            {
                var channel = wcfConnection.CreateChannel();
                channel.NotifyUserCurrentMonitoringStep(dto);
                wcfConnection.Close();
            }
            catch (Exception e)
            {
                _logFile.AppendLine("SendCurrentMonitoringStep: " + e.Message);
            }
        }

        public void SendClientMeasurementDone(ClientMeasurementDoneDto dto)
        {
            var wcfConnection = _wcfFactory.GetR2DChannelFactory(false);
            if (wcfConnection == null)
                return;

            try
            {
                var channel = wcfConnection.CreateChannel();
                channel.TransmitClientMeasurementResult(dto);
                wcfConnection.Close();
            }
            catch (Exception e)
            {
                _logFile.AppendLine("SendClientMeasurementDone: " + e.Message);
            }
        }
    }
}