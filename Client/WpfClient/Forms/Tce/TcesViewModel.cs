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
        private ObservableCollection<TceS> _tces;

        public ObservableCollection<TceS> Tces
        {
            get => _tces;
            set
            {
                if (Equals(value, _tces)) return;
                _tces = value;
                NotifyOfPropertyChange();
            }
        }

        public TceS SelectedTce { get; set; }
        public bool IsEnabled { get; set; }

        public TcesViewModel(ILifetimeScope globalScope, Model readModel, EventArrivalNotifier eventArrivalNotifier,
            IWcfServiceDesktopC2D c2DWcfManager, IWindowManager windowManager, CurrentUser currentUser)
        {
            _globalScope = globalScope;
            _readModel = readModel;
            _c2DWcfManager = c2DWcfManager;
            _windowManager = windowManager;
            eventArrivalNotifier.PropertyChanged += _eventArrivalNotifier_PropertyChanged;
            IsEnabled = currentUser.Role <= Role.Root;
        }

        private void _eventArrivalNotifier_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Tces = new ObservableCollection<TceS>(_readModel.TcesNew);
        }

        public void Initialize()
        {
            Tces = new ObservableCollection<TceS>(_readModel.TcesNew);
            if (Tces.Count > 0)
                SelectedTce = Tces.First();
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Telecommunications_equipment;
        }

        public async void AddTce()
        {
            var vm = _globalScope.Resolve<TceTypeViewModel>();
            if (!_readModel.TceTypeStructs.Any())
                await vm.ReSeed();

            vm.Initialize(_readModel.TceTypeStructs.First());
            if (_windowManager.ShowDialogWithAssignedOwner(vm) != true)
                return;

            var selectedTceType = vm.SelectedTabItem == 0
                ? vm.HuaweiSelectionViewModel.SelectedType
                : vm.ZteSelectionViewModel.SelectedType;

            var ovm = _globalScope.Resolve<OneTceViewModel>();
            var tce = selectedTceType.CreateTce();
            ovm.Initialize(tce, true);
            _windowManager.ShowDialogWithAssignedOwner(ovm);
        }

        public void UpdateTce()
        {
            var vm = _globalScope.Resolve<TceTypeViewModel>();
            vm.Initialize(SelectedTce.TceTypeStruct);
            if (_windowManager.ShowDialogWithAssignedOwner(vm) != true)
                return;

            var selectedTceType = vm.SelectedTabItem == 0
                ? vm.HuaweiSelectionViewModel.SelectedType
                : vm.ZteSelectionViewModel.SelectedType;

            if (SelectedTce.TceTypeStruct.Id != selectedTceType.Id)
            {
                ApplyTypeChanges(selectedTceType);
            }
        }

        private void ApplyTypeChanges(TceTypeStruct newTceTypeStruct)
        {
            SelectedTce.TceTypeStruct = newTceTypeStruct;
            var thisTceRelations = _readModel.GponPortRelations.Where(r => r.TceId == SelectedTce.Id).ToList();
            foreach (var slot in SelectedTce.Slots)
            {
                if (!newTceTypeStruct.SlotPositions.Contains(slot.Position))
                {
                    var relationsForRemoval = thisTceRelations.Where(r => r.TceSlot == slot.Position);
                }
            }
        }

        public void UpdateTceComponents()
        {
            var vm = _globalScope.Resolve<OneTceViewModel>();
            vm.Initialize(SelectedTce, false);
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

        public void Close()
        {
            TryClose();
        }

    }
}
