﻿using System.ServiceModel;
using Iit.Fibertest.Dto;

namespace ClientWcfServiceLibrary
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IClientWcfService" in both code and config file together.
    [ServiceContract]
    public interface IClientWcfService
    {
        // Responses on Client's requests

        [OperationContract]
        void ConfirmDelivery(RtuCommandDeliveredDto dto);

        [OperationContract]
        void ConfirmRtuConnectionChecked(RtuConnectionCheckedDto dto);

        [OperationContract]
        void ConfirmRtuInitialized(RtuInitializedDto rtu);

        [OperationContract]
        void ConfirmMonitoringStarted(MonitoringStartedDto confirm);

        [OperationContract]
        void ConfirmMonitoringStopped(MonitoringStoppedDto confirm);

        [OperationContract]
        void ConfirmMonitoringSettingsApplied(MonitoringSettingsAppliedDto confirm);

        [OperationContract]
        void ConfirmBaseRefAssigned(BaseRefAssignedDto confirm);


        // Notifications
        [OperationContract]
        void ProcessRtuCurrentMonitoringStep(KnowRtuCurrentMonitoringStepDto dto);
    }


}
