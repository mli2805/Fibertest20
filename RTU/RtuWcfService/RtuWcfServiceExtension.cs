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
        public static Task<bool> StartMonitoringAsync(
            this IRtuWcfService rtuWcfService, RtuWcfServiceBackward backwardService, StartMonitoringDto dto)
        {
            var src = new TaskCompletionSource<bool>();
            backwardService.HandlerForStartMonitoring.AddHandler(src);
            rtuWcfService.BeginStartMonitoring(dto);
            return src.Task;
        }
        public static Task<bool> StopMonitoringAsync(
            this IRtuWcfService rtuWcfService, RtuWcfServiceBackward backwardService, StopMonitoringDto dto)
        {
            var src = new TaskCompletionSource<bool>();
            backwardService.HandlerForStartMonitoring.AddHandler(src);
            rtuWcfService.BeginStopMonitoring(dto);
            return src.Task;
        }
        public static Task<bool> ApplyMonitoringSettingsAsync(
            this IRtuWcfService rtuWcfService, RtuWcfServiceBackward backwardService, ApplyMonitoringSettingsDto dto)
        {
            var src = new TaskCompletionSource<bool>();
            backwardService.HandlerForStartMonitoring.AddHandler(src);
            rtuWcfService.BeginApplyMonitoringSettings(dto);
            return src.Task;
        }
        public static Task<bool> AssignBaseRefAsync(
            this IRtuWcfService rtuWcfService, RtuWcfServiceBackward backwardService, AssignBaseRefDto dto)
        {
            var src = new TaskCompletionSource<bool>();
            backwardService.HandlerForStartMonitoring.AddHandler(src);
            rtuWcfService.BeginAssignBaseRef(dto);
            return src.Task;
        }
    }
}