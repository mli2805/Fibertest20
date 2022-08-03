using System.Threading.Tasks;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.RtuWcfServiceInterface
{
    public static class RtuWcfServiceExtension
    {
        public static Task<RtuInitializedDto> InitializeAsync(
            this IRtuWcfService rtuWcfService, RtuWcfServiceBackward backwardService, InitializeRtuDto dto)
        {
            var src = new TaskCompletionSource<RtuInitializedDto>();
            backwardService.HandlerForInitializeRtu.AddHandler(src);
            rtuWcfService.BeginInitialize(dto);
            return src.Task;
        }

        public static Task<OtauAttachedDto> AttachOtauAsync(
            this IRtuWcfService rtuWcfService, RtuWcfServiceBackward backwardService, AttachOtauDto dto)
        {
            var src = new TaskCompletionSource<OtauAttachedDto>();
            backwardService.HandlerForAttachOtau.AddHandler(src);
            rtuWcfService.BeginAttachOtau(dto);
            return src.Task;
        }

        public static Task<OtauDetachedDto> DetachOtauAsync(
            this IRtuWcfService rtuWcfService, RtuWcfServiceBackward backwardService, DetachOtauDto dto)
        {
            var src = new TaskCompletionSource<OtauDetachedDto>();
            backwardService.HandlerForDetachOtau.AddHandler(src);
            rtuWcfService.BeginDetachOtau(dto);
            return src.Task;
        }

        public static Task<bool> StopMonitoringAsync(
            this IRtuWcfService rtuWcfService, RtuWcfServiceBackward backwardService, StopMonitoringDto dto)
        {
            var src = new TaskCompletionSource<bool>();
            backwardService.HandlerForStopMonitoring.AddHandler(src);
            rtuWcfService.BeginStopMonitoring(dto);
            return src.Task;
        }
        public static Task<MonitoringSettingsAppliedDto> ApplyMonitoringSettingsAsync(
            this IRtuWcfService rtuWcfService, RtuWcfServiceBackward backwardService, ApplyMonitoringSettingsDto dto)
        {
            var src = new TaskCompletionSource<MonitoringSettingsAppliedDto>();
            backwardService.HandlerForApplyMonitoringSettings.AddHandler(src);
            rtuWcfService.BeginApplyMonitoringSettings(dto);
            return src.Task;
        }
        public static Task<BaseRefAssignedDto> AssignBaseRefAsync(
            this IRtuWcfService rtuWcfService, RtuWcfServiceBackward backwardService, AssignBaseRefsDto dto)
        {
            var src = new TaskCompletionSource<BaseRefAssignedDto>();
            backwardService.HandlerForAssignBaseRef.AddHandler(src);
            rtuWcfService.BeginAssignBaseRef(dto);
            return src.Task;
        }
        public static Task<ClientMeasurementStartedDto> DoClientMeasurementAsync(
            this IRtuWcfService rtuWcfService, RtuWcfServiceBackward backwardService, DoClientMeasurementDto dto)
        {
            var src = new TaskCompletionSource<ClientMeasurementStartedDto>();
            backwardService.HandlerForClientMeasurement.AddHandler(src);
            rtuWcfService.BeginClientMeasurement(dto);
            return src.Task;
        }
        public static Task<RequestAnswer> StartOutOfTurnMeasurementAsync(
            this IRtuWcfService rtuWcfService, RtuWcfServiceBackward backwardService, DoOutOfTurnPreciseMeasurementDto dto)
        {
            var src = new TaskCompletionSource<RequestAnswer>();
            backwardService.HandlerForOutOfTurnMeasurement.AddHandler(src);
            rtuWcfService.BeginOutOfTurnPreciseMeasurement(dto);
            return src.Task;
        }
        public static Task<RequestAnswer> StartInterruptMeasurementAsync(
             this IRtuWcfService rtuWcfService, RtuWcfServiceBackward backwardService, InterruptMeasurementDto dto)
        {
            var src = new TaskCompletionSource<RequestAnswer>();
            backwardService.HandlerForInterruptMeasurement.AddHandler(src);
            rtuWcfService.BeginInterruptMeasurement(dto);
            return src.Task;
        }
    }
}