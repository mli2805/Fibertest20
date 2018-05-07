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
        private readonly Model _readModel;
        private readonly GraphReadModel _graphReadModel;
        private readonly IWindowManager _windowManager;
        private readonly IWcfServiceForClient _c2DWcfManager;
        private readonly RtuRemover _rtuRemover;
        private readonly RtuStateViewsManager _rtuStateViewsManager;
        private readonly LandmarksViewsManager _landmarksViewsManager;

        public RtuLeafActions(ILifetimeScope globalScope, IMyLog logFile, Model readModel, GraphReadModel graphReadModel,
            IWindowManager windowManager, IWcfServiceForClient c2DWcfManager, RtuRemover rtuRemover, 
            RtuStateViewsManager rtuStateViewsManager, LandmarksViewsManager landmarksViewsManager)
        {
            _globalScope = globalScope;
            _logFile = logFile;
            _readModel = readModel;
            _graphReadModel = graphReadModel;
            _windowManager = windowManager;
            _c2DWcfManager = c2DWcfManager;
            _rtuRemover = rtuRemover;
            _rtuStateViewsManager = rtuStateViewsManager;
            _landmarksViewsManager = landmarksViewsManager;
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

            var vm = _globalScope.Resolve<RtuInitializeViewModel>(new NamedParameter(@"rtuLeaf", rtuLeaf));
            _windowManager.ShowWindowWithAssignedOwner(vm);
        }

        public void ShowRtuState(object param)
        {
            if (param is RtuLeaf rtuLeaf)
                _rtuStateViewsManager.ShowRtuState(rtuLeaf);
        }

        public async void ShowRtuLandmarks(object param)
        {
            if (!(param is RtuLeaf rtuLeaf))
                return;

            await _landmarksViewsManager.InitializeFromRtu(rtuLeaf.Id);
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


        public async void RemoveRtu(object param)
        {
            if (param is RtuLeaf rtuLeaf)
                await _rtuRemover.Fire(_readModel.Rtus.First(r => r.Id == rtuLeaf.Id));
        }

        public void DefineTraceStepByStep(object param)
        {
            if (!(param is RtuLeaf rtuLeaf))
                return;

            var rtuNodeId = _readModel.Rtus.First(r => r.Id == rtuLeaf.Id).NodeId;
            _graphReadModel.GrmRtuRequests.DefineTraceStepByStep(rtuNodeId, rtuLeaf.Title);
        }

        public async void HideTraces(object parameter)
        {
            if (!(parameter is RtuLeaf rtuLeaf))
                return;

            var item = rtuLeaf.MyContextMenu.First(i => i?.Header == Resources.SID_Hide_traces);
            item.IsChecked = !item.IsChecked;
            var rtuNodeId = _readModel.Rtus.First(r => r.Id == rtuLeaf.Id).NodeId;
            await _graphReadModel.GrmRtuRequests.SaveUsersHiddenRtus(rtuNodeId);
        }
    }
}