using System.Collections.ObjectModel;
using Autofac;
using Caliburn.Micro;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.WcfServiceForClientInterface;

namespace Iit.Fibertest.Client
{
    public class ZonesViewModel : Screen
    {
        private readonly ILifetimeScope _globalScope;
        private readonly ReadModel _readModel;
        private readonly IWcfServiceForClient _c2DWcfManager;
        private readonly IWindowManager _windowManager;

        private ObservableCollection<Zone> _rows;
        public ObservableCollection<Zone> Rows
        {
            get { return _rows; }
            set
            {
                if (Equals(value, _rows)) return;
                _rows = value;
                NotifyOfPropertyChange();
            }
        }

        public Zone SelectedZone { get; set; }

        public ZonesViewModel(ILifetimeScope globalScope, ReadModel readModel, IWcfServiceForClient c2DWcfManager, IWindowManager windowManager)
        {
            _globalScope = globalScope;
            _readModel = readModel;
            _c2DWcfManager = c2DWcfManager;
            _windowManager = windowManager;
            Rows = new ObservableCollection<Zone>(readModel.Zones);
            readModel.PropertyChanged += ReadModel_PropertyChanged;
        }

        private void ReadModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Rows = new ObservableCollection<Zone>(_readModel.Zones);
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Responsibility_zones;
        }

        public void AddZone()
        {
            var vm = _globalScope.Resolve<ZoneViewModel>();
            vm.Initialize();
            _windowManager.ShowDialogWithAssignedOwner(vm);
        }

        public void UpdateZone()
        {
            var vm = _globalScope.Resolve<ZoneViewModel>();
            vm.Initialize(SelectedZone);
            _windowManager.ShowDialogWithAssignedOwner(vm);
        }

        public async void RemoveZone()
        {
            var cmd = new RemoveZone(){ZoneId = SelectedZone.ZoneId};
            if (await _c2DWcfManager.SendCommandAsObj(cmd) == null)
                Rows.Remove(SelectedZone);
        }

        public void Close()
        {
                TryClose();
        }

    }
}
