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
        void EndAttachOtau(OtauAttachedDto dto);

        [OperationContract(IsOneWay = true)]
        void EndDetachOtau(OtauDetachedDto dto);

        [OperationContract(IsOneWay = true)]
        void EndStopMonitoring(bool result);

        [OperationContract(IsOneWay = true)]
        void EndApplyMonitoringSettings(RequestAnswer result);

        [OperationContract(IsOneWay = true)]
        void EndAssignBaseRef(BaseRefAssignedDto result);

        [OperationContract(IsOneWay = true)]
        void EndStartClientMeasurement(ClientMeasurementStartedDto result);

        [OperationContract(IsOneWay = true)]
        void EndStartOutOfTurnMeasurement(RequestAnswer result);

        [OperationContract(IsOneWay = true)]
        void EndInterruptMeasurement(RequestAnswer result);

        [OperationContract(IsOneWay = true)]
        void EndFreeOtdr(RequestAnswer result);
    }
}