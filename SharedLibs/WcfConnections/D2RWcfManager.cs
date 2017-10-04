using System;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.RtuWcfServiceLibrary;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.WcfConnections
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


        public async Task<RtuInitializedDto> InitializeRtuAsync(InitializeRtuDto dto)
        {
            _d2RChannel = _wcfFactory.CreateRtuConnection();
            if (_d2RChannel == null)
                return new RtuInitializedDto() { IsInitialized = false, ErrorCode = 21 };

            return await _d2RChannel.InitializeRtuAsync(dto);
        }

        private IRtuWcfService _d2RChannel;
        public bool InitializeRtuLongTask(InitializeRtuDto dto, AsyncCallback callback)
        {
            _d2RChannel = _wcfFactory.CreateRtuConnection();
            if (_d2RChannel == null)
                return false;
            var asyncState = new object();
            _d2RChannel.BeginInitializeRtu(dto, callback, asyncState);
            return true;
        }

        public RtuInitializedDto InitializeRtuLongTaskEnd(IAsyncResult asyncState)
        {
            if (_d2RChannel == null)
                return null;

            try
            {
                return _d2RChannel.EndInitializeRtu(asyncState);
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
                return null;
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
