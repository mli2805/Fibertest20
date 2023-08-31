using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Autofac;
using AutoMapper;
using Caliburn.Micro;
using Iit.Fibertest.Graph;
using Iit.Fibertest.WpfCommonViews;

namespace Iit.Fibertest.Client
{
    public class RtuAccidentViewsManager
    {
        private static readonly IMapper Mapper = new MapperConfiguration(
            cfg => cfg.AddProfile<MappingEventToDomainModelProfile>()).CreateMapper();

        private readonly ILifetimeScope _globalScope;
        private readonly CurrentUser _currentUser;
        private readonly CurrentClientConfiguration _currentClientConfiguration;
        private readonly Model _readModel;
        private readonly IWindowManager _windowManager;

        private List<RtuAccidentViewModel> LaunchedViews { get; } = new List<RtuAccidentViewModel>();

        public RtuAccidentViewsManager(ILifetimeScope globalScope, CurrentUser currentUser, 
            CurrentClientConfiguration currentClientConfiguration, Model readModel,
            IWindowManager windowManager, ChildrenViews childrenViews)
        {
            _globalScope = globalScope;
            _currentUser = currentUser;
            _currentClientConfiguration = currentClientConfiguration;
            _readModel = readModel;
            _windowManager = windowManager;
            childrenViews.PropertyChanged += ChildrenViewsPropertyChanged;
        }

        private void ChildrenViewsPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == nameof(ChildrenViews.ShouldBeClosed))
            {
                if (((ChildrenViews)sender).ShouldBeClosed)
                {
                    foreach (var traceStateViewModel in LaunchedViews.ToArray())
                    {
                        traceStateViewModel.TryClose();
                        LaunchedViews.Remove(traceStateViewModel);
                    }
                }
            }
        }

        public void Apply(object e)
        {
            switch (e)
            {
                case RtuAccidentAdded evnt: AddRtuAccident(evnt); break;

                case RtuUpdated evnt: UpdateRtu(evnt); break; // Title
                case RtuRemoved evnt: RemoveRtu(evnt.RtuId); break;
                case TraceUpdated evnt: UpdateTrace(evnt); break; // Title
                case TraceCleaned evnt: RemoveOrCleanTrace(evnt.TraceId); break;
                case TraceRemoved evnt: RemoveOrCleanTrace(evnt.TraceId); break;
                case ResponsibilitiesChanged evnt: ChangeResponsibilities(evnt); break;
            }
        }

        private void AddRtuAccident(RtuAccidentAdded evnt)
        {
            if (_currentClientConfiguration.DoNotSignalAboutRtuStatusEvents) return;

            var accident = Mapper.Map<RtuAccident>(evnt);
            var accidentModel = new RtuAccidentModel(accident).Build(_readModel);

            Show(accidentModel);
        }

        private void Show(RtuAccidentModel rtuAccidentModel)
        {
            LaunchedViews.RemoveAll(v => !v.IsOpen);

            var vm = LaunchedViews.FirstOrDefault(v =>
                v.AccidentModel.Accident.TraceId == rtuAccidentModel.Accident.TraceId);

            if (vm != null)
            {
                vm.TryClose();
                LaunchedViews.Remove(vm);
            }

            vm = _globalScope.Resolve<RtuAccidentViewModel>();
            vm.Initialize(rtuAccidentModel);
            LaunchedViews.Add(vm);
            _windowManager.ShowWindowWithAssignedOwner(vm);
        }

        private void UpdateRtu(RtuUpdated e)
        {
            foreach (var rtuAccidentViewModel in LaunchedViews.Where(v => v.AccidentModel.Accident.RtuId == e.RtuId))
            {
                rtuAccidentViewModel.AccidentModel.RtuTitle = e.Title;
            }
        }

        private void UpdateTrace(TraceUpdated e)
        {
            foreach (var rtuAccidentViewModel in LaunchedViews.Where(v => v.AccidentModel.Accident.TraceId == e.Id))
            {
                rtuAccidentViewModel.AccidentModel.TraceTitle = e.Title;
            }
        }

        private void RemoveRtu(Guid rtuId)
        {
            foreach (var rtuAccidentViewModel in
                     LaunchedViews.Where(v => v.AccidentModel.Accident.RtuId == rtuId).ToList())
            {
                rtuAccidentViewModel.Close();
                LaunchedViews.Remove(rtuAccidentViewModel);
            }
        }

        private void RemoveOrCleanTrace(Guid traceId)
        {
            foreach (var rtuAccidentViewModel in
                     LaunchedViews.Where(v => v.AccidentModel.Accident.TraceId == traceId).ToList())
            {
                rtuAccidentViewModel.Close();
                LaunchedViews.Remove(rtuAccidentViewModel);
            }
        }

        private void ChangeResponsibilities(ResponsibilitiesChanged evnt)
        {
            foreach (var pair in evnt.ResponsibilitiesDictionary)
            {
                if (!pair.Value.Contains(_currentUser.ZoneId)) continue; // for current zone this trace doesn't change

                var rtu = _readModel.Rtus.FirstOrDefault(r => r.Id == pair.Key);
                if (rtu != null)
                    RemoveRtu(rtu.Id);
                else
                {
                    var trace = _readModel.Traces.FirstOrDefault(t => t.TraceId == pair.Key);
                    if (trace != null) RemoveOrCleanTrace(trace.TraceId);

                }
            }
        }
    }
}
