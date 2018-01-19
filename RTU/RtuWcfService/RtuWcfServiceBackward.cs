using System.Collections.Generic;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.RtuWcfServiceInterface
{
    public class Handler<T>
    {
        private readonly Queue<TaskCompletionSource<T>> _handler = new Queue<TaskCompletionSource<T>>();
        public void AddHandler(TaskCompletionSource<T> handler) => _handler.Enqueue(handler);
        public void End(T result) => _handler.Dequeue().TrySetResult(result);
    }

    public class RtuWcfServiceBackward : IRtuWcfServiceBackward
    {
        public Handler<RtuInitializedDto> HandlerForInitializeRtu { get; } = new Handler<RtuInitializedDto>();
        public void EndInitialize(RtuInitializedDto dto) => HandlerForInitializeRtu.End(dto);
        public Handler<OtauAttachedDto> HandlerForAttachOtau { get; } = new Handler<OtauAttachedDto>();
        public void EndAttachOtau(OtauAttachedDto dto) => HandlerForAttachOtau.End(dto);
        public Handler<OtauDetachedDto> HandlerForDetachOtau { get; } = new Handler<OtauDetachedDto>();
        public void EndDetachOtau(OtauDetachedDto dto) => HandlerForDetachOtau.End(dto);
        public Handler<bool> HandlerForStopMonitoring { get; } = new Handler<bool>();
        public void EndStopMonitoring(bool result) => HandlerForStopMonitoring.End(result);

        public Handler<MonitoringSettingsAppliedDto> HandlerForApplyMonitoringSettings { get; } = new Handler<MonitoringSettingsAppliedDto>();
        public void EndApplyMonitoringSettings(MonitoringSettingsAppliedDto result) => HandlerForApplyMonitoringSettings.End(result);

        public Handler<BaseRefAssignedDto> HandlerForAssignBaseRef { get; } = new Handler<BaseRefAssignedDto>();
        public void EndAssignBaseRef(BaseRefAssignedDto result) => HandlerForAssignBaseRef.End(result);


        public Handler<ClientMeasurementDoneDto> HandlerForClientMeasurement { get; } = new Handler<ClientMeasurementDoneDto>();
        public void EndClientMeasurement(ClientMeasurementDoneDto result) => HandlerForClientMeasurement.End(result);

        public Handler<OutOfTurnMeasurementStartedDto> HandlerForOutOfTurnMeasurement { get; } = new Handler<OutOfTurnMeasurementStartedDto>();
        public void EndStartOutOfTurnMeasurement(OutOfTurnMeasurementStartedDto result) => HandlerForOutOfTurnMeasurement.End(result);
    }
}