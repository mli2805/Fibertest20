using System.ServiceModel;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.RtuWcfServiceInterface
{
    [ServiceContract(CallbackContract = typeof(IRtuWcfServiceBackward))]
    public interface IRtuWcfService
    {
        [OperationContract]
        void BeginInitialize(InitializeRtuDto dto);

        [OperationContract]
        void BeginAttachOtau(AttachOtauDto dto);

        [OperationContract]
        void BeginDetachOtau(DetachOtauDto dto);

        [OperationContract]
        void BeginStopMonitoring(StopMonitoringDto dto);

        [OperationContract]
        void BeginApplyMonitoringSettings(ApplyMonitoringSettingsDto settings);

        [OperationContract]
        void BeginAssignBaseRef(AssignBaseRefsDto baseRefs);


        [OperationContract]
        void BeginClientMeasurement(DoClientMeasurementDto dto);

        [OperationContract]
        void BeginOutOfTurnPreciseMeasurement(DoOutOfTurnPreciseMeasurementDto dto);


        // for WatchDog
        [OperationContract]
        bool CheckLastSuccessfullMeasTime();
    }
}