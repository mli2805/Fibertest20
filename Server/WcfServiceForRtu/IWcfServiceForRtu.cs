﻿using System.ServiceModel;
using Dto;

namespace WcfServiceForRtuLibrary
{
    [ServiceContract]
    public interface IWcfServiceForRtu
    {
        [OperationContract]
        bool ProcessRtuConnectionChecked(RtuConnectionCheckedDto result);

        [OperationContract]
        bool ProcessRtuInitialized(RtuInitializedDto result);

        [OperationContract]
        bool ConfirmStartMonitoring(MonitoringStartedDto result);

        [OperationContract]
        bool ConfirmStopMonitoring(MonitoringStoppedDto result);


        [OperationContract]
        bool ProcessMonitoringResult(SaveMonitoringResultDto result);

        [OperationContract]
        bool ConfirmMonitoringSettingsApplied(MonitoringSettingsAppliedDto result);

        [OperationContract]
        bool ConfirmBaseRefAssigned(BaseRefAssignedDto result);
    }
}
