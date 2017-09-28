using System;
using System.Threading.Tasks;
using Dto;
using Iit.Fibertest.UtilsLib;
using RtuWcfServiceLibrary;

namespace WcfConnections
{
    public class D2RWcfManager
    {
        private readonly IMyLog _logFile;
        private readonly WcfFactory _wcfFactory;

        public D2RWcfManager(DoubleAddress dataCenterAddress, IniFile iniFile, IMyLog logFile)
        {
            _logFile = logFile;
            _wcfFactory = new WcfFactory(dataCenterAddress, iniFile, _logFile);
        }

        private IRtuWcfService _d2RChannel;
        public bool InitializeRtuLongTask(InitializeRtuDto dto)
        {
            _d2RChannel = _wcfFactory.CreateRtuConnection();
            if (_d2RChannel == null)
                return false;
            var asyncState = new object();
            _d2RChannel.BeginInitializeAndAnswer(dto, MyCallback, asyncState);
            return true;
        }
        private void MyCallback(object asyncState)
        {
            try
            {
                _logFile.AppendLine("MyCallback started");

                if (_d2RChannel == null)
                    return;

                var result = _d2RChannel.EndInitializeAndAnswer((IAsyncResult)asyncState);
                _logFile.AppendLine($@"{result.Version}");

            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
            }
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
