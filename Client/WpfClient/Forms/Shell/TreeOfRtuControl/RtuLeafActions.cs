using Autofac;
using Iit.Fibertest.Client.MonitoringSettings;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.Client
{
    public class RtuLeafActions
    {
        private readonly IMyLog _logFile;

        public RtuLeafActions(IMyLog logFile)
        {
            _logFile = logFile;
        }

        public void UpdateRtu(object param)
        {
            var rtuLeaf = param as RtuLeaf;
            if (rtuLeaf == null)
                return;

            var vm = new RtuUpdateViewModel(rtuLeaf.Id, rtuLeaf.ReadModel, rtuLeaf.C2DWcfManager);
            rtuLeaf.WindowManager.ShowDialog(vm);
        }

        public void ShowRtu(object param)
        {
            var rtuLeaf = param as RtuLeaf;
            if (rtuLeaf != null)
                rtuLeaf.PostOffice.Message = new CenterToRtu() { RtuId = rtuLeaf.Id };
        }

        public void InitializeRtu(object param)
        {
            var rtuLeaf = param as RtuLeaf;
            if (rtuLeaf == null)
                return;

            var localScope = rtuLeaf.GlobalScope.BeginLifetimeScope(ctx => ctx.RegisterInstance(rtuLeaf));
            var vm = localScope.Resolve<RtuInitializeViewModel>();
            rtuLeaf.WindowManager.ShowDialog(vm);
        }

        public void ShowRtuState(object param)
        {
        }

        public void ShowRtuLandmarks(object param)
        {
            var rtuLeaf = param as RtuLeaf;
            if (rtuLeaf == null)
                return;

            var vm = new LandmarksViewModel(rtuLeaf.ReadModel, rtuLeaf.WindowManager);
            vm.Initialize(rtuLeaf.Id, true);
            rtuLeaf.WindowManager.ShowDialog(vm);
        }

        public void ShowMonitoringSettings(object param)
        {
            var rtuLeaf = param as RtuLeaf;
            if (rtuLeaf == null)
                return;

            var vm = new MonitoringSettingsViewModel(rtuLeaf, rtuLeaf.ReadModel, rtuLeaf.C2DWcfManager);
            rtuLeaf.WindowManager.ShowDialog(vm);
        }

        public async void StopMonitoring(object param)
        {
            var rtuLeaf = param as RtuLeaf;
            if (rtuLeaf == null)
                return;

            bool result;
            using (new WaitCursor())
            {
                result =
                    await rtuLeaf.C2DWcfManager.StopMonitoringAsync(new StopMonitoringDto() { RtuId = rtuLeaf.Id });
            }
            _logFile.AppendLine($@"Stop monitoring result - {result}");
            var vm = new NotificationViewModel(
                result ? Resources.SID_Information : Resources.SID_Error_,
                result ? Resources.SID_RTU_is_turned_into_manual_mode : Resources.SID_Cannot_turn_RTU_into_manual_mode);
            if (result)
            {
                var cmd = new StopMonitoring() { RtuId = rtuLeaf.Id };
                await rtuLeaf.C2DWcfManager.SendCommandAsObj(cmd);
            }
            rtuLeaf.WindowManager.ShowDialog(vm);

        }

        public async void StartMonitoring(object param)
        {
            var rtuLeaf = param as RtuLeaf;
            if (rtuLeaf == null)
                return;

            bool result;
            using (new WaitCursor())
            {
                result =
                    await rtuLeaf.C2DWcfManager.StartMonitoringAsync(new StartMonitoringDto() { RtuId = rtuLeaf.Id });
            }
            _logFile.AppendLine($@"Start monitoring result - {result}");
            var vm = new NotificationViewModel(
                result ? Resources.SID_Information : Resources.SID_Error_,
                result ? Resources.SID_RTU_is_turned_into_automatic_mode : Resources.SID_Cannot_turn_RTU_into_automatic_mode);
            if (result)
            {
                var cmd = new StartMonitoring() { RtuId = rtuLeaf.Id };
                await rtuLeaf.C2DWcfManager.SendCommandAsObj(cmd);
            }
            rtuLeaf.WindowManager.ShowDialog(vm);
        }


        public void RemoveRtu(object param)
        {
            var rtuLeaf = param as RtuLeaf;
            rtuLeaf?.C2DWcfManager.SendCommandAsObj(new RemoveRtu() { Id = rtuLeaf.Id });
        }

        public void DefineTraceStepByStep(object param)
        {
        }
    }
}