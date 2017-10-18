﻿using System.ServiceModel;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.RtuWcfServiceInterface
{
    [ServiceContract(CallbackContract = typeof(IRtuWcfServiceBackward))]
    public interface IRtuWcfService
    {
        [OperationContract]
        void BeginInitialize(InitializeRtuDto dto);

        [OperationContract]
        void BeginStartMonitoring(StartMonitoringDto dto);


        [OperationContract]
        void BeginStopMonitoring(StopMonitoringDto dto);

        [OperationContract]
        void BeginApplyMonitoringSettings(ApplyMonitoringSettingsDto settings);

        [OperationContract]
        void BeginAssignBaseRef(AssignBaseRefDto baseRef);


        // for Client
        [OperationContract]
        bool ToggleToPort(OtauPortDto port);


        // for WatchDog
        [OperationContract]
        bool CheckLastSuccessfullMeasTime();
    }
}