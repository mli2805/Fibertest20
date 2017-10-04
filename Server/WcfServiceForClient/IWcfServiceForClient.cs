﻿using System.ServiceModel;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.WcfServiceForClientInterface
{
    [ServiceContract]
    public interface IWcfServiceForClient
    {
        [OperationContract]
        string SendCommand(string json);

        [OperationContract]
        string[] GetEvents(int revision);



        // C2D
        [OperationContract]
        Task<ClientRegisteredDto> RegisterClientAsync(RegisterClientDto dto);

        [OperationContract]
        Task UnregisterClientAsync(UnRegisterClientDto dto);

        [OperationContract]
        bool CheckServerConnection(CheckServerConnectionDto dto);


        // C2D2R
        [OperationContract]
        Task<RtuConnectionCheckedDto> CheckRtuConnectionAsync(CheckRtuConnectionDto rtuAddress);


        [OperationContract]
        void InitializeRtuLongTask(InitializeRtuDto dto);


        [OperationContract]
        bool InitializeRtu(InitializeRtuDto rtu);

        [OperationContract]
        bool StartMonitoring(StartMonitoringDto dto);

        [OperationContract]
        bool StopMonitoring(StopMonitoringDto dto);

        [OperationContract]
        bool ApplyMonitoringSettings(ApplyMonitoringSettingsDto settings);

        [OperationContract]
        bool AssignBaseRef(AssignBaseRefDto baseRef);
    }
}