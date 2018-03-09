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
using Iit.Fibertest.WcfServiceForClientInterface;

namespace Iit.Fibertest.Client
{
    public class RtuLeafActions
    {
        private readonly ILifetimeScope _globalScope;
        private readonly IMyLog _logFile;
        private readonly ReadModel _readModel;
        private readonly GraphReadModel _graphReadModel;
        private readonly IWindowManager _windowManager;
        private readonly IWcfServiceForClient _c2DWcfManager;
        private readonly RtuStateViewsManager _rtuStateViewsManager;
        private readonly LandmarksViewModel _landmarksViewModel;

        public RtuLeafActions(ILifetimeScope globalScope, IMyLog logFile, ReadModel readModel, GraphReadModel graphReadModel,
            IWindowManager windowManager, IWcfServiceForClient c2DWcfManager, 
            RtuStateViewsManager rtuStateViewsManager, LandmarksViewModel landmarksViewModel)
        {
            _globalScope = globalScope;
            _logFile = logFile;
            _readModel = readModel;
            _graphReadModel = graphReadModel;
            _windowManager = windowManager;
            _c2DWcfManager = c2DWcfManager;
            _rtuStateViewsManager = rtuStateViewsManager;
            _landmarksViewModel = landmarksViewModel;
        }

        public void UpdateRtu(object param)
        {
            if (!(param is RtuLeaf rtuLeaf))
                return;

            var vm = _globalScope.Resolve<RtuUpdateViewModel>();
            vm.Initialize(rtuLeaf.Id);
            _windowManager.ShowWindowWithAssignedOwner(vm);
        }

        public void ShowRtu(object param)
        {
            if (param is RtuLeaf rtuLeaf)
                //   rtuLeaf.PostOffice.Message = new CenterToRtu() { RtuId = rtuLeaf.Id };
                _graphReadModel.PlaceRtuIntoScreenCenter(rtuLeaf.Id);
        }

        public void InitializeRtu(object param)
        {
            if (!(param is RtuLeaf rtuLeaf))
                return;

            var localScope = _globalScope.BeginLifetimeScope(ctx => ctx.RegisterInstance(rtuLeaf));
            var vm = localScope.Resolve<RtuInitializeViewModel>();
            _windowManager.ShowWindowWithAssignedOwner(vm);
        }

        public void ShowRtuState(object param)
        {
            if (param is RtuLeaf rtuLeaf)
                _rtuStateViewsManager.ShowRtuState(rtuLeaf);
        }

        public void ShowRtuLandmarks(object param)
        {
            if (!(param is RtuLeaf rtuLeaf))
                return;

            _landmarksViewModel.Initialize(rtuLeaf.Id, true);
            _windowManager.ShowWindowWithAssignedOwner(_landmarksViewModel);
        }

        public void ShowMonitoringSettings(object param)
        {
            if (!(param is RtuLeaf rtuLeaf))
                return;

            var vm = new MonitoringSettingsViewModel(rtuLeaf, _readModel, _c2DWcfManager);
            _windowManager.ShowWindowWithAssignedOwner(vm);
        }

        public async void StopMonitoring(object param)
        {
            if (!(param is RtuLeaf rtuLeaf))
                return;

            bool result;
            using (new WaitCursor())
            {
                result =
                    await _c2DWcfManager.StopMonitoringAsync(new StopMonitoringDto() { RtuId = rtuLeaf.Id });
            }
            _logFile.AppendLine($@"Stop monitoring result - {result}");
            if (result)
            {
                var cmd = new StopMonitoring() { RtuId = rtuLeaf.Id };
                await _c2DWcfManager.SendCommandAsObj(cmd);
                _rtuStateViewsManager.NotifyUserMonitoringStopped(rtuLeaf.Id);
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
                if (child is IPortOwner otauLeaf)
                {
                    result.AddRange(CollectTracesInMonitoringCycle(otauLeaf, false));
                    continue;
                }

                if (!(child is TraceLeaf trace))
                    continue;

                if (trace.BaseRefsSet.IsInMonitoringCycle)
                    result.Add(new PortWithTraceDto()
                    {
                        TraceId = trace.Id,
                        OtauPort = new OtauPortDto()
                        {
                            OtauIp = portOwnerLeaf.OtauNetAddress.Ip4Address,
                            OtauTcpPort = portOwnerLeaf.OtauNetAddress.Port,
                            IsPortOnMainCharon = isMainCharon,
                            OpticalPort = trace.PortNumber,
                        }
                    });
            }
            return result;
        }

        public async void StartMonitoring(object param)
        {
            if (!(param is RtuLeaf rtuLeaf))
                return;

            var dto = CollectMonitoringSettingsFromTree(rtuLeaf);
            if (dto.Ports.Count == 0)
            {
                var vm = new MyMessageBoxViewModel(MessageType.Error, Resources.SID_No_traces_selected_for_monitoring_);
                _windowManager.ShowDialogWithAssignedOwner(vm);
                return;
            }
            dto.IsMonitoringOn = true;

            using (new WaitCursor())
            {
                var resultDto = await _c2DWcfManager.ApplyMonitoringSettingsAsync(dto);
                _logFile.AppendLine($@"Start monitoring result - {resultDto.ReturnCode == ReturnCode.MonitoringSettingsAppliedSuccessfully}");
                if (resultDto.ReturnCode == ReturnCode.MonitoringSettingsAppliedSuccessfully)
                {
                    var cmd = new StartMonitoring() { RtuId = rtuLeaf.Id };
                    await _c2DWcfManager.SendCommandAsObj(cmd);
                    _rtuStateViewsManager.NotifyUserMonitoringStarted(rtuLeaf.Id);
                }
            }
        }


        public void RemoveRtu(object param)
        {
            if (param is RtuLeaf rtuLeaf)
                _c2DWcfManager.SendCommandAsObj(new RemoveRtu() { Id = rtuLeaf.Id });
        }

        public void DefineTraceStepByStep(object param)
        {
            if (!(param is RtuLeaf rtuLeaf))
                return;

            var rtuNodeId = _graphReadModel.Data.Rtus.First(r => r.Id == rtuLeaf.Id).Node.Id;
            _graphReadModel.GrmRtuRequests.DefineTraceStepByStep(rtuNodeId);
        }
    }
}