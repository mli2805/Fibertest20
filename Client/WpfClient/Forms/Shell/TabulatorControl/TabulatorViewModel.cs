using System.Linq;
using System.Windows;
using Autofac;
using Caliburn.Micro;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public class TabulatorViewModel : PropertyChangedBase
    {
        public GraphReadModel GraphReadModel { get; }
        private readonly ILifetimeScope _globalScope;
        private readonly OpticalEventsDoubleViewModel _opticalEventsDoubleViewModel;
        private readonly NetworkEventsDoubleViewModel _networkEventsDoubleViewModel;
        private readonly BopNetworkEventsDoubleViewModel _bopNetworkEventsDoubleViewModel;
        private readonly CurrentlyHiddenRtu _currentlyHiddenRtu;
        private readonly Model _readModel;
        private readonly IWindowManager _windowManager;

        private int _selectedTabIndex;
        public int SelectedTabIndex
        {
            get => _selectedTabIndex;
            set
            {
                //                if (value == _selectedTabIndex) return;
                _selectedTabIndex = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(IsGisActive));
                ChangeTabVisibility();
            }
        }

        public bool IsGisActive => SelectedTabIndex == 3;

        #region Pictograms

        public Visibility IsThereActualOpticalEventsPictogram { get; set; } = Visibility.Hidden;
        public Visibility IsThereActualNetworkEventsPictogram { get; set; } = Visibility.Hidden;
        public Visibility IsThereActualBopEventsPictogram { get; set; } = Visibility.Hidden;

        #endregion

        #region Visibilities

        private Visibility _opticalEventsVisibility;
        public Visibility OpticalEventsVisibility
        {
            get => _opticalEventsVisibility;
            set
            {
                if (value == _opticalEventsVisibility) return;
                _opticalEventsVisibility = value;
                NotifyOfPropertyChange();
            }
        }

        private Visibility _networkEventsVisibility;
        public Visibility NetworkEventsVisibility
        {
            get => _networkEventsVisibility;
            set
            {
                if (value == _networkEventsVisibility) return;
                _networkEventsVisibility = value;
                NotifyOfPropertyChange();
            }
        }

        private Visibility _bopNetworkEventsVisibility;
        public Visibility BopNetworkEventsVisibility
        {
            get => _bopNetworkEventsVisibility;
            set
            {
                if (value == _bopNetworkEventsVisibility) return;
                _bopNetworkEventsVisibility = value;
                NotifyOfPropertyChange();
            }
        }

        private Visibility _mapVisibility;
        public Visibility MapVisibility
        {
            get => _mapVisibility;
            set
            {
                if (value == _mapVisibility) return;
                _mapVisibility = value;
                NotifyOfPropertyChange();
            }
        }

        private Visibility _messageVisibility;
        public Visibility MessageVisibility
        {
            get => _messageVisibility;
            set
            {
                if (value == _messageVisibility) return;
                _messageVisibility = value;
                NotifyOfPropertyChange();
            }
        }

        #endregion

        public TabulatorViewModel(ILifetimeScope globalScope,
            OpticalEventsDoubleViewModel opticalEventsDoubleViewModel,
            NetworkEventsDoubleViewModel networkEventsDoubleViewModel,
            BopNetworkEventsDoubleViewModel bopNetworkEventsDoubleViewModel,
            GraphReadModel graphReadModel, CurrentlyHiddenRtu currentlyHiddenRtu, 
            Model readModel, IWindowManager windowManager)
        {
            GraphReadModel = graphReadModel;
            _globalScope = globalScope;
            _opticalEventsDoubleViewModel = opticalEventsDoubleViewModel;
            _networkEventsDoubleViewModel = networkEventsDoubleViewModel;
            _bopNetworkEventsDoubleViewModel = bopNetworkEventsDoubleViewModel;
            _currentlyHiddenRtu = currentlyHiddenRtu;
            _readModel = readModel;
            _windowManager = windowManager;
            SubscribeActualEventsRowChanged();
            SelectedTabIndex = 0;
        }

        private void SubscribeActualEventsRowChanged()
        {
            _opticalEventsDoubleViewModel.ActualOpticalEventsViewModel.Rows.CollectionChanged += (s, e) =>
            {
                IsThereActualOpticalEventsPictogram = _opticalEventsDoubleViewModel.ActualOpticalEventsViewModel.Rows.Any()
                    ? Visibility.Visible
                    : Visibility.Hidden;
                NotifyOfPropertyChange(nameof(IsThereActualOpticalEventsPictogram));
            };
            _networkEventsDoubleViewModel.ActualNetworkEventsViewModel.Rows.CollectionChanged += (s, e) =>
            {
                IsThereActualNetworkEventsPictogram = _networkEventsDoubleViewModel.ActualNetworkEventsViewModel.Rows.Any()
                    ? Visibility.Visible
                    : Visibility.Hidden;
                NotifyOfPropertyChange(nameof(IsThereActualNetworkEventsPictogram));
            };
            _bopNetworkEventsDoubleViewModel.ActualBopNetworkEventsViewModel.Rows.CollectionChanged += (s, e) =>
            {
                IsThereActualBopEventsPictogram = _bopNetworkEventsDoubleViewModel.ActualBopNetworkEventsViewModel.Rows.Any()
                    ? Visibility.Visible
                    : Visibility.Hidden;
                NotifyOfPropertyChange(nameof(IsThereActualBopEventsPictogram));
            };
        }

        private void ChangeTabVisibility()
        {
            switch (_selectedTabIndex)
            {
                case 0:
                    OpticalEventsVisibility = Visibility.Visible;
                    NetworkEventsVisibility = Visibility.Collapsed;
                    BopNetworkEventsVisibility = Visibility.Collapsed;
                    MapVisibility = Visibility.Collapsed;
                    MessageVisibility = Visibility.Collapsed;
                    break;
                case 1:
                    OpticalEventsVisibility = Visibility.Collapsed;
                    NetworkEventsVisibility = Visibility.Visible;
                    BopNetworkEventsVisibility = Visibility.Collapsed;
                    MapVisibility = Visibility.Collapsed;
                    MessageVisibility = Visibility.Collapsed;
                    break;
                case 2:
                    OpticalEventsVisibility = Visibility.Collapsed;
                    NetworkEventsVisibility = Visibility.Collapsed;
                    BopNetworkEventsVisibility = Visibility.Visible;
                    MapVisibility = Visibility.Collapsed;
                    MessageVisibility = Visibility.Collapsed;
                    break;
                case 3:
                    OpticalEventsVisibility = Visibility.Collapsed;
                    NetworkEventsVisibility = Visibility.Collapsed;
                    BopNetworkEventsVisibility = Visibility.Collapsed;
                    MapVisibility = Visibility.Visible;
                    MessageVisibility = Visibility.Collapsed;
                    break;
                case 4:
                    OpticalEventsVisibility = Visibility.Collapsed;
                    NetworkEventsVisibility = Visibility.Collapsed;
                    BopNetworkEventsVisibility = Visibility.Collapsed;
                    MapVisibility = Visibility.Collapsed;
                    MessageVisibility = Visibility.Visible;
                    break;
            }
        }

        public void Extinguish()
        {
            GraphReadModel.Extinguish();
        }

        public void ShowAllGraph()
        {
            using (_globalScope.Resolve<IWaitCursor>())
            {
                var vm = new MyMessageBoxViewModel(MessageType.LongOperation, @"----------------------------------------------------");
                _windowManager.ShowWindowWithAssignedOwner(vm);

                _currentlyHiddenRtu.Collection.Clear();

                vm.TryClose();

            }

        }

        public void HideAllGraph()
        {
            using (_globalScope.Resolve<IWaitCursor>())
            {
                var vm = new MyMessageBoxViewModel(MessageType.LongOperation, @"----------------------------------------------");
                _windowManager.ShowWindowWithAssignedOwner(vm);

                _currentlyHiddenRtu.Collection.Clear();
                foreach (var rtu in _readModel.Rtus)
                    _currentlyHiddenRtu.Collection.Add(rtu.Id);

                vm.TryClose();
            }
        }
    }
}
