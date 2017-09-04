using Dto;
using Iit.Fibertest.UtilsLib;

namespace WcfConnections
{
    public class D2RWcfManager
    {
        private readonly LogFile _logFile;
        private readonly WcfFactory _wcfFactory;

        public D2RWcfManager(DoubleAddress dataCenterAddress, IniFile iniFile, LogFile logFile)
        {
            _logFile = logFile;
            _wcfFactory = new WcfFactory(dataCenterAddress, iniFile, _logFile);
        }

        public MessageProcessingResult InitializeRtu(InitializeRtuDto dto)
        {
            var rtuConnection = _wcfFactory.CreateRtuConnection();
            if (rtuConnection == null)
                return MessageProcessingResult.FailedToTransmit;
            rtuConnection.Initialize(dto);

            _logFile.AppendLine($"Transfered command to initialize RTU {dto.RtuId}");
            return MessageProcessingResult.TransmittedSuccessfully;
        }

        public MessageProcessingResult StartMonitoring(StartMonitoringDto dto)
        {
            var rtuConnection = _wcfFactory.CreateRtuConnection();
            if (rtuConnection == null)
                return MessageProcessingResult.FailedToTransmit;

            var result = rtuConnection.StartMonitoring(dto);
            _logFile.AppendLine($"Transfered command to start monitoring on RTU {dto.RtuId.First6()}");
            return result ? MessageProcessingResult.TransmittedSuccessfully : MessageProcessingResult.TransmittedSuccessfullyButRtuIsBusy;
        }

        public MessageProcessingResult StopMonitoring(StopMonitoringDto dto)
        {
            var rtuConnection = _wcfFactory.CreateRtuConnection();
            if (rtuConnection == null)
                return MessageProcessingResult.FailedToTransmit;

            var result = rtuConnection.StopMonitoring(dto);
            _logFile.AppendLine($"Transfered command to stop monitoring on RTU {dto.RtuId.First6()}");
            return result ? MessageProcessingResult.TransmittedSuccessfully : MessageProcessingResult.TransmittedSuccessfullyButRtuIsBusy;
        }

        public MessageProcessingResult AssignBaseRef(AssignBaseRefDto dto)
        {
            var rtuConnection = _wcfFactory.CreateRtuConnection();
            if (rtuConnection == null)
                return MessageProcessingResult.FailedToTransmit;

            var result = rtuConnection.AssignBaseRef(dto);
            _logFile.AppendLine($"Transfered command to assign base ref to RTU {dto.RtuId.First6()}");
            return result ? MessageProcessingResult.TransmittedSuccessfully : MessageProcessingResult.TransmittedSuccessfullyButRtuIsBusy;
        }

        public MessageProcessingResult ApplyMonitoringSettings(ApplyMonitoringSettingsDto dto)
        {
            var rtuConnection = _wcfFactory.CreateRtuConnection();
            if (rtuConnection == null)
                return MessageProcessingResult.FailedToTransmit;

            rtuConnection.ApplyMonitoringSettings(dto);
            _logFile.AppendLine($"Transfered command to apply monitoring settings for RTU {dto.RtuId.First6()}");
            return MessageProcessingResult.TransmittedSuccessfully;
        }
    }
}
