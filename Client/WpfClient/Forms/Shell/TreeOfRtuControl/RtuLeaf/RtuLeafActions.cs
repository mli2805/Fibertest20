﻿using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Caliburn.Micro;
using Iit.Fibertest.Client.MonitoringSettings;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;
using Iit.Fibertest.WpfCommonViews;


namespace Iit.Fibertest.Client
{
    public class RtuLeafActions
    {
        private readonly ILifetimeScope _globalScope;
        private readonly IMyLog _logFile;
        private readonly Model _readModel;
        private readonly GraphReadModel _graphReadModel;
        private readonly IWindowManager _windowManager;
        private readonly IWcfServiceDesktopC2D _c2DWcfManager;
        private readonly IWcfServiceCommonC2D _commonC2DWcfManager;
        private readonly RtuRemover _rtuRemover;
        private readonly TabulatorViewModel _tabulatorViewModel;
        private readonly RtuStateViewsManager _rtuStateViewsManager;
        private readonly LandmarksViewsManager _landmarksViewsManager;

        public RtuLeafActions(ILifetimeScope globalScope, IMyLog logFile, Model readModel, GraphReadModel graphReadModel,
            IWindowManager windowManager, IWcfServiceDesktopC2D c2DWcfManager,  IWcfServiceCommonC2D commonC2DWcfManager,
            RtuRemover rtuRemover, TabulatorViewModel tabulatorViewModel,
            RtuStateViewsManager rtuStateViewsManager, LandmarksViewsManager landmarksViewsManager)
        {
            _globalScope = globalScope;
            _logFile = logFile;
            _readModel = readModel;
            _graphReadModel = graphReadModel;
            _windowManager = windowManager;
            _c2DWcfManager = c2DWcfManager;
            _commonC2DWcfManager = commonC2DWcfManager;
            _rtuRemover = rtuRemover;
            _tabulatorViewModel = tabulatorViewModel;
            _rtuStateViewsManager = rtuStateViewsManager;
            _landmarksViewsManager = landmarksViewsManager;
        }

        public void ShowRtuInfoView(object param)
        {
            if (!(param is RtuLeaf rtuLeaf))
                return;

            var vm = _globalScope.Resolve<RtuUpdateViewModel>();
            vm.Initialize(rtuLeaf.Id);
            _windowManager.ShowDialogWithAssignedOwner(vm);
        }

        public void HigtlightRtu(object param)
        {
            if (param is RtuLeaf rtuLeaf)
            {

                _graphReadModel.PlaceRtuIntoScreenCenter(rtuLeaf.Id);
                if (_tabulatorViewModel.SelectedTabIndex != 3)
                    _tabulatorViewModel.SelectedTabIndex = 3;
            }
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

            var vm = _globalScope.Resolve<MonitoringSettingsViewModel>(new NamedParameter(@"rtuLeaf", rtuLeaf));
            _windowManager.ShowWindowWithAssignedOwner(vm);
        }

        public async void StopMonitoring(object param)
        {
            if (!(param is RtuLeaf rtuLeaf))
                return;

            var rtu = _readModel.Rtus.FirstOrDefault(r => r.Id == rtuLeaf.Id);
            if (rtu == null) return;

            bool result;
            using (new WaitCursor())
            {
                result =
                    await _commonC2DWcfManager.StopMonitoringAsync(new StopMonitoringDto() { RtuId = rtuLeaf.Id, RtuMaker = rtu.RtuMaker });
            }
            _logFile.AppendLine($@"Stop monitoring result - {result}");
        }


        private ApplyMonitoringSettingsDto CollectMonitoringSettingsFromTree(RtuLeaf rtuLeaf)
        {
            var rtu = _readModel.Rtus.FirstOrDefault(r => r.Id == rtuLeaf.Id);
            if (rtu == null) return null;

            var result = new ApplyMonitoringSettingsDto()
            {
                RtuId = rtuLeaf.Id,
                RtuMaker = rtuLeaf.RtuMaker,
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
                            Serial = portOwnerLeaf.Serial,
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
                var resultDto = await _commonC2DWcfManager.ApplyMonitoringSettingsAsync(dto);
                _logFile.AppendLine($@"Start monitoring result - {resultDto.ReturnCode == ReturnCode.MonitoringSettingsAppliedSuccessfully}");
            }
        }

        public async void DetachAllTraces(object param)
        {
            if (!(param is RtuLeaf rtuLeaf))
                return;

            using (new WaitCursor())
            {
                var cmd = new DetachAllTraces() {RtuId = rtuLeaf.Id};
                await _c2DWcfManager.SendCommandAsObj(cmd);
                _rtuStateViewsManager.NotifyUserMonitoringStarted(rtuLeaf.Id);
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
    }
}