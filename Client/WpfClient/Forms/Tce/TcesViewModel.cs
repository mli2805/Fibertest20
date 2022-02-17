using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Autofac;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.WcfConnections;
using Iit.Fibertest.WpfCommonViews;

namespace Iit.Fibertest.Client
{
    public class TcesViewModel : Screen
    {
        private readonly ILifetimeScope _globalScope;
        private readonly Model _readModel;
        private readonly IWcfServiceDesktopC2D _c2DWcfManager;
        private readonly IWindowManager _windowManager;
        private readonly TceComponentsViewModel _tceComponentsViewModel;
        private ObservableCollection<Tce> _tces;

        public ObservableCollection<Tce> Tces   
        {
            get => _tces;
            set
            {
                if (Equals(value, _tces)) return;
                _tces = value;
                NotifyOfPropertyChange();
            }
        }

        public Tce SelectedTce { get; set; }
        public bool IsEnabled { get; set; }

        public TcesViewModel(ILifetimeScope globalScope, Model readModel, EventArrivalNotifier eventArrivalNotifier,
            IWcfServiceDesktopC2D c2DWcfManager, IWindowManager windowManager, CurrentUser currentUser, 
            TceComponentsViewModel tceComponentsViewModel)
        {
            _globalScope = globalScope;
            _readModel = readModel;
            _c2DWcfManager = c2DWcfManager;
            _windowManager = windowManager;
            _tceComponentsViewModel = tceComponentsViewModel;
            eventArrivalNotifier.PropertyChanged += _eventArrivalNotifier_PropertyChanged;
            IsEnabled = currentUser.Role <= Role.Root;
        }

        private void _eventArrivalNotifier_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Tces = new ObservableCollection<Tce>(_readModel.Tces);
        }

        public void Initialize()
        {
            Tces = new ObservableCollection<Tce>(_readModel.Tces);
            if (Tces.Count > 0)
                SelectedTce = Tces.First();
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Telecommunications_equipment;
        }

        public void MapTceToRtu()
        {
            var vm = _globalScope.Resolve<RelationsOfTceViewModel>();
            vm.Initialize(SelectedTce);
            _windowManager.ShowDialogWithAssignedOwner(vm);
        }

        public void AddTce()
        {
            var vm = _globalScope.Resolve<TceViewModel>();
            vm.Initialize();
            _windowManager.ShowDialogWithAssignedOwner(vm);
        }

        public void UpdateTce()
        {
            var vm = _globalScope.Resolve<TceViewModel>();
            vm.Initialize(SelectedTce);
            _windowManager.ShowDialogWithAssignedOwner(vm);
        }

        public async void RemoveTce()
        {
            if (!ConfirmTceRemove()) return;

            var cmd = new RemoveTce() { Id = SelectedTce.Id };
            if (await _c2DWcfManager.SendCommandAsObj(cmd) == null)
            {
                Tces.Remove(SelectedTce);
            }
        }

        private bool ConfirmTceRemove()
        {
            var strs = new List<string>()
            {
                string.Format(Resources.SID_Equipment__0__will_be_deleted, SelectedTce.Title),
                "",
                Resources.SID_RTU_port_links_to_this_equipment_will_be_deleted
            };
            var vm = new MyMessageBoxViewModel(MessageType.Confirmation, strs, 0);
            _windowManager.ShowDialogWithAssignedOwner(vm);
            return vm.IsAnswerPositive;
        }

        public void UpdateTceComponents()
        {
            _tceComponentsViewModel.Initialize(SelectedTce);
            _windowManager.ShowDialog(_tceComponentsViewModel);
        }


        public void Close()
        {
            TryClose();
        }

    }
}
