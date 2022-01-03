﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Caliburn.Micro;
using Iit.Fibertest.Graph;
using Iit.Fibertest.Graph.Requests;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.WpfCommonViews;


namespace Iit.Fibertest.Client
{
    public class GrmRtuRequests
    {
        private readonly ILifetimeScope _globalScope;
        private readonly Model _model;
        private readonly CurrentGis _currentGis;
        private readonly IWindowManager _windowManager;
        private readonly RtuRemover _rtuRemover;

        public GrmRtuRequests(ILifetimeScope globalScope, Model model, CurrentGis currentGis,
           IWindowManager windowManager, RtuRemover rtuRemover)
        {
            _globalScope = globalScope;
            _model = model;
            _currentGis = currentGis;
            _windowManager = windowManager;
            _rtuRemover = rtuRemover;
        }

        public void AddRtuAtGpsLocation(RequestAddRtuAtGpsLocation request)
        {
            var vm = _globalScope.Resolve<RtuUpdateViewModel>();
            vm.Initialize(request);
            _windowManager.ShowDialogWithAssignedOwner(vm);
        }

        public void UpdateRtu(RequestUpdateRtu request)
        {
            var rtu = _model.Rtus.First(r => r.NodeId == request.NodeId);
            var vm = _globalScope.Resolve<RtuUpdateViewModel>();
            vm.Initialize(rtu.Id);
            _windowManager.ShowDialogWithAssignedOwner(vm);
        }

        public async Task<string> RemoveRtu(RequestRemoveRtu request)
        {
            var rtu = _model.Rtus.First(r => r.NodeId == request.NodeId);
            return await _rtuRemover.Fire(rtu);
        }


        public async void DefineTraceStepByStep(Guid rtuNodeId, string rtuTitle)
        {
            var vm = _globalScope.Resolve<TraceStepByStepViewModel>();
            if (vm.IsOpen)
            {
                var vm1 = new MyMessageBoxViewModel(MessageType.Information, Resources.SID_Step_by_step_trace_definition_is_started_already_);
                _windowManager.ShowDialogWithAssignedOwner(vm1);
                return;
            }

            using (_globalScope.Resolve<IWaitCursor>())
            {
                await vm.Initialize(rtuNodeId, rtuTitle);

            }
            _windowManager.ShowWindowWithAssignedOwner(vm);
        }


        public void ChangeRtuTracesVisibility(Guid rtuNodeId)
        {
            var rtu = _model.Rtus.First(r => r.NodeId == rtuNodeId);
            if (rtu.IsHighlighted)
            {
                foreach (var trace in _currentGis.Traces.Where(t => t.RtuId == rtu.Id).ToList())
                {
                    _currentGis.Traces.Remove(trace);
                    trace.IsHighlighted = false;
                }
            }
            else
            {
                foreach (var trace in _model.Traces.Where(t => t.RtuId == rtu.Id))
                {
                    if (!_currentGis.Traces.Contains(trace))
                        _currentGis.Traces.Add(trace);
                    trace.IsHighlighted = true;
                }
            }

            rtu.IsHighlighted = !rtu.IsHighlighted;
        }
    }
}