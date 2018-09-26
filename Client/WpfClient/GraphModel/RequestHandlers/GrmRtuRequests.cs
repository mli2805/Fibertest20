using System;
using System.Collections.Generic;
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
        private readonly CurrentlyHiddenRtu _currentlyHiddenRtu;
        private readonly IWindowManager _windowManager;
        private readonly RtuRemover _rtuRemover;

        public GrmRtuRequests(ILifetimeScope globalScope, Model model, CurrentlyHiddenRtu currentlyHiddenRtu,
           IWindowManager windowManager, RtuRemover rtuRemover)
        {
            _globalScope = globalScope;
            _model = model;
            _currentlyHiddenRtu = currentlyHiddenRtu;
            _windowManager = windowManager;
            _rtuRemover = rtuRemover;
        }

        public void AddRtuAtGpsLocation(RequestAddRtuAtGpsLocation request)
        {
            if (_model.License.RtuCount.Value <= _model.Rtus.Count)
            {
                var mb = new MyMessageBoxViewModel(MessageType.Error, Resources.SID_Exceeded_the_number_of_RTU_for_an_existing_license);
                _windowManager.ShowDialogWithAssignedOwner(mb);
                return;
            }
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

     
        public void DefineTraceStepByStep(Guid rtuNodeId, string rtuTitle)
        {
            if (!AskRevealTracesIfHidden(rtuNodeId)) return;

            var vm = _globalScope.Resolve<TraceStepByStepViewModel>();
            vm.Initialize(rtuNodeId, rtuTitle);
            _windowManager.ShowWindowWithAssignedOwner(vm);
        }

        public bool AskRevealTracesIfHidden(Guid rtuNodeId)
        {
            var rtuId = _model.Rtus.First(r => r.NodeId == rtuNodeId).Id;
            if (_currentlyHiddenRtu.Collection.Contains(rtuId))
            {
                var strs = new List<string>()
                {
                    Resources.SID_RTU_shoud_be_in_Reveal_Traces_Mode_,
                    "",
                    Resources.SID_Reveal_traces + @" ?",
                };
                var mb = new MyMessageBoxViewModel(MessageType.Confirmation, strs, 0);
                _windowManager.ShowDialogWithAssignedOwner(mb);
                if (!mb.IsAnswerPositive) return false;
                ChangeRtuTracesVisibility(rtuNodeId);
            }
            return true;
        }

        public void ChangeRtuTracesVisibility(Guid rtuNodeId)
        {
            var rtu = _model.Rtus.First(r => r.NodeId == rtuNodeId);
            _currentlyHiddenRtu.IsHideAllPressed = false;
            _currentlyHiddenRtu.IsShowAllPressed = false;

            if (_currentlyHiddenRtu.Collection.Contains(rtu.Id))
                _currentlyHiddenRtu.Collection.Remove(rtu.Id);
            else
                _currentlyHiddenRtu.Collection.Add(rtu.Id);
            _currentlyHiddenRtu.ChangedRtu = rtu.Id;
        }
    }
}