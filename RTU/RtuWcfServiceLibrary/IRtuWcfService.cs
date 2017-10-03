using System;
using System.ServiceModel;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.RtuWcfServiceLibrary
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IRtuWcfService" in both code and config file together.
    [ServiceContract]
    public interface IRtuWcfService
    {
     
        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginInitializeAndAnswer(InitializeRtuDto dto, AsyncCallback callback, object asyncState);
        RtuInitializedDto EndInitializeAndAnswer(IAsyncResult result);
        


        [OperationContract]
        bool Initialize(InitializeRtuDto rtu);

        [OperationContract]
        bool StartMonitoring(StartMonitoringDto dto);

        [OperationContract]
        bool StopMonitoring(StopMonitoringDto dto);

        [OperationContract]
        bool ApplyMonitoringSettings(ApplyMonitoringSettingsDto settings);

        [OperationContract]
        bool AssignBaseRef(AssignBaseRefDto baseRef);

        [OperationContract]
        bool ToggleToPort(OtauPortDto port);


        // for WatchDog
        [OperationContract]
        bool CheckLastSuccessfullMeasTime();
    }

}
