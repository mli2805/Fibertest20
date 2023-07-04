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
        private readonly Model _readModel;
        private readonly IWindowManager _windowManager;

        private List<RtuAccidentViewModel> LaunchedViews { get; } = new List<RtuAccidentViewModel>();

        public RtuAccidentViewsManager(ILifetimeScope globalScope, Model readModel, 
            IWindowManager windowManager, ChildrenViews childrenViews)
        {
            _globalScope = globalScope;
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
            }
        }

        private void AddRtuAccident(RtuAccidentAdded evnt)
        {
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
    }
}
