using System;
using System.ServiceModel;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.RtuWcfServiceInterface
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IRtuWcfService" in both code and config file together.
    [ServiceContract]
    public interface IRtuWcfService
    {
     
        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginInitializeRtu(InitializeRtuDto dto, AsyncCallback callback, object asyncState);
        RtuInitializedDto EndInitializeRtu(IAsyncResult result);
        


        [OperationContract]
        bool Initialize(InitializeRtuDto dto);

        [OperationContract]
        bool StartMonitoring(StartMonitoringDto dto);

        [OperationContract]
        bool StopMonitoring(StopMonitoringDto dto);

        [OperationContract]
        bool ApplyMonitoringSettings(ApplyMonitoringSettingsDto settings);

        [OperationContract]
        bool AssignBaseRef(AssignBaseRefDto baseRef);


        // for Client
        [OperationContract]
        bool ToggleToPort(OtauPortDto port);


        // for WatchDog
        [OperationContract]
        bool CheckLastSuccessfullMeasTime();
    }

    public static class RtuWcfServiceExtension
    {
        public static async Task<RtuInitializedDto> InitializeRtuAsync(
            this IRtuWcfService rtuWcfService, InitializeRtuDto dto)
        {
            return await Task.Factory.FromAsync(rtuWcfService.BeginInitializeRtu, rtuWcfService.EndInitializeRtu, dto, null);
        }
    }

}
