using System.ServiceModel;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.RtuWcfServiceInterface
{
    [ServiceContract]
    public interface IRtuWcfServiceBackward
    {
        [OperationContract(IsOneWay = true)]
        void EndInitialize(RtuInitializedDto dto);

        [OperationContract(IsOneWay = true)]
        void EndStartMonitoring(bool result);

        [OperationContract(IsOneWay = true)]
        void EndStopMonitoring(bool result);

        [OperationContract(IsOneWay = true)]
        void EndApplyMonitoringSettings(MonitoringSettingsAppliedDto result);

        [OperationContract(IsOneWay = true)]
        void EndAssignBaseRef(BaseRefAssignedDto result);
    }
}