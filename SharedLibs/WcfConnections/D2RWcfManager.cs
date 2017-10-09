using System;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.RtuWcfServiceInterface;
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
            _logFile.AppendLine($"Still on datacenter, transmitting command");
            var d2RContract = _wcfFactory.CreateRtuConnection();
            if (d2RContract == null)
                return new RtuInitializedDto() { IsInitialized = false, ErrorCode = 21 };

            _logFile.AppendLine($"Channel created");

            return await d2RContract.InitializeRtuAsync(dto);
        }

        private IRtuWcfService _rtuWcfService;
        public IAsyncResult BeginInitializeRtu(InitializeRtuDto dto, AsyncCallback callback, object asyncState)
        {
            _logFile.AppendLine($"Still on datacenter, transmitting command");
            _rtuWcfService = _wcfFactory.CreateRtuConnection();
            if (_rtuWcfService == null)
                return null;

            _logFile.AppendLine($"Channel created");
            var t = _rtuWcfService.BeginInitializeRtu(dto, callback, asyncState);
            _logFile.AppendLine($"Begin finished");
            return t;
        }

        public RtuInitializedDto EndInitializeRtu(IAsyncResult asyncResult)
        {
            return ((Task<RtuInitializedDto>) asyncResult).Result;
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
