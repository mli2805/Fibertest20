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

        public Handler<bool> HandlerForStartMonitoring { get; } = new Handler<bool>();
        public void EndStartMonitoring(bool result) => HandlerForStartMonitoring.End(result);

        public Handler<bool> HandlerForStopMonitoring { get; } = new Handler<bool>();
        public void EndStopMonitoring(bool result) => HandlerForStopMonitoring.End(result);

        public Handler<MonitoringSettingsAppliedDto> HandlerForApplyMonitoringSettings { get; } = new Handler<MonitoringSettingsAppliedDto>();
        public void EndApplyMonitoringSettings(MonitoringSettingsAppliedDto result) => HandlerForApplyMonitoringSettings.End(result);

        public Handler<BaseRefAssignedDto> HandlerForAssignBaseRef { get; } = new Handler<BaseRefAssignedDto>();
        public void EndAssignBaseRef(BaseRefAssignedDto result) => HandlerForAssignBaseRef.End(result);
    }
}