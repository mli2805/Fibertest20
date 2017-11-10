﻿using System.ServiceModel;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;

namespace ClientWcfServiceInterface
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IClientWcfService" in both code and config file together.
    [ServiceContract]
    public interface IClientWcfService
    {

        // Notifications
        [OperationContract]
        void ProcessRtuCurrentMonitoringStep(KnowRtuCurrentMonitoringStepDto dto);

        [OperationContract]
        Task<int> NotifyAboutRtuChangedAvailability(ListOfRtuWithChangedAvailabilityDto dto);
    }


}
