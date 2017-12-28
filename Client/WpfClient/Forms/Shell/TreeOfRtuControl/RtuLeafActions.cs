using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Caliburn.Micro;
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
        private readonly ReadModel _readModel;
        private readonly IWindowManager _windowManager;
        private readonly RtuStateViewsManager _rtuStateViewsManager;

        public RtuLeafActions(IMyLog logFile, ReadModel readModel, IWindowManager windowManager, RtuStateViewsManager rtuStateViewsManager)
        {
            _logFile = logFile;
            _readModel = readModel;
            _windowManager = windowManager;
            _rtuStateViewsManager = rtuStateViewsManager;
        }

        public void UpdateRtu(object param)
        {
            var rtuLeaf = param as RtuLeaf;
            if (rtuLeaf == null)
                return;

            var vm = new RtuUpdateViewModel(rtuLeaf.Id, rtuLeaf.ReadModel, rtuLeaf.C2DWcfManager);
            rtuLeaf.WindowManager.ShowWindowWithAssignedOwner(vm);
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
            rtuLeaf.WindowManager.ShowWindowWithAssignedOwner(vm);
        }

        public void ShowRtuState(object param)
        {
            var rtuLeaf = param as RtuLeaf;
            if (rtuLeaf != null)
                _rtuStateViewsManager.ShowRtuState(rtuLeaf);
        }

        public void ShowRtuLandmarks(object param)
        {
            var rtuLeaf = param as RtuLeaf;
            if (rtuLeaf == null)
                return;

            var vm = new LandmarksViewModel(rtuLeaf.ReadModel, rtuLeaf.WindowManager);
            vm.Initialize(rtuLeaf.Id, true);
            rtuLeaf.WindowManager.ShowWindowWithAssignedOwner(vm);
        }

        public void ShowMonitoringSettings(object param)
        {
            var rtuLeaf = param as RtuLeaf;
            if (rtuLeaf == null)
                return;

            var vm = new MonitoringSettingsViewModel(rtuLeaf, rtuLeaf.ReadModel, rtuLeaf.C2DWcfManager);
            rtuLeaf.WindowManager.ShowWindowWithAssignedOwner(vm);
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
            if (result)
            {
                var cmd = new StopMonitoring() { RtuId = rtuLeaf.Id };
                await rtuLeaf.C2DWcfManager.SendCommandAsObj(cmd);
            }
        }


        private ApplyMonitoringSettingsDto CollectMonitoringSettingsFromTree(RtuLeaf rtuLeaf)
        {
            var rtu = _readModel.Rtus.FirstOrDefault(r => r.Id == rtuLeaf.Id);
            if (rtu == null) return null;

            var result = new ApplyMonitoringSettingsDto()
            {
                RtuId = rtuLeaf.Id,
                Timespans = new MonitoringTimespansDto()
                {
                    FastSave = TimeSpan.FromHours((int)rtu.FastSave),
                    PreciseMeas = TimeSpan.FromHours((int)rtu.PreciseMeas),
                    PreciseSave = TimeSpan.FromHours((int)rtu.PreciseSave),
                },
                Ports = CollectTracesInMonitoringCycle(rtuLeaf, true),
            };
            return result;
        }

        private List<PortWithTraceDto> CollectTracesInMonitoringCycle(IPortOwner portOwnerLeaf, bool isMainCharon)
        {
            var result = new List<PortWithTraceDto>();
            foreach (var child in portOwnerLeaf.ChildrenImpresario.Children)
            {
                var otauLeaf = child as IPortOwner;
                if (otauLeaf != null)
                {
                    result.AddRange(CollectTracesInMonitoringCycle(otauLeaf, false));
                    continue;
                }
                var trace = child as TraceLeaf;
                if (trace == null)
                    continue;

                if (trace.IsInMonitoringCycle)
                    result.Add(new PortWithTraceDto() {TraceId = trace.Id, OtauPort = new OtauPortDto()
                    {
                        OtauIp = portOwnerLeaf.OtauNetAddress.Ip4Address, 
                        OtauTcpPort = portOwnerLeaf.OtauNetAddress.Port,
                        IsPortOnMainCharon = isMainCharon,
                        OpticalPort = trace.PortNumber,
                    } });
            }
            return result;
        }

        public async void StartMonitoring(object param)
        {
            var rtuLeaf = param as RtuLeaf;
            if (rtuLeaf == null)
                return;

            var dto = CollectMonitoringSettingsFromTree(rtuLeaf);
            if (dto.Ports.Count == 0)
            {
                var vm = new NotificationViewModel(Resources.SID_Error_, Resources.SID_No_traces_selected_for_monitoring_);
                _windowManager.ShowDialogWithAssignedOwner(vm);
                return;
            }
            dto.IsMonitoringOn = true;

            using (new WaitCursor())
            {
                var resultDto = await rtuLeaf.C2DWcfManager.ApplyMonitoringSettingsAsync(dto);
                _logFile.AppendLine($@"Start monitoring result - {resultDto.ReturnCode == ReturnCode.MonitoringSettingsAppliedSuccessfully}");
                if (resultDto.ReturnCode == ReturnCode.MonitoringSettingsAppliedSuccessfully)
                {
                    var cmd = new StartMonitoring() {RtuId = rtuLeaf.Id};
                    await rtuLeaf.C2DWcfManager.SendCommandAsObj(cmd);
                }
            }
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