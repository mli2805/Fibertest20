using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Autofac;
using Caliburn.Micro;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public class TabulatorViewModel : PropertyChangedBase
    {
        public GraphReadModel GraphReadModel { get; }
        private readonly ILifetimeScope _globalScope;
        private readonly IWindowManager _windowManager;
        private readonly IDispatcherProvider _dispatcherProvider;
        private readonly OpticalEventsDoubleViewModel _opticalEventsDoubleViewModel;
        private readonly NetworkEventsDoubleViewModel _networkEventsDoubleViewModel;
        private readonly BopNetworkEventsDoubleViewModel _bopNetworkEventsDoubleViewModel;
        private readonly CurrentlyHiddenRtu _currentlyHiddenRtu;
        private readonly Model _readModel;

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

        public TabulatorViewModel(ILifetimeScope globalScope, IWindowManager windowManager, IDispatcherProvider dispatcherProvider,
            OpticalEventsDoubleViewModel opticalEventsDoubleViewModel,
            NetworkEventsDoubleViewModel networkEventsDoubleViewModel,
            BopNetworkEventsDoubleViewModel bopNetworkEventsDoubleViewModel,
            GraphReadModel graphReadModel, CurrentlyHiddenRtu currentlyHiddenRtu,
            Model readModel)
        {
            GraphReadModel = graphReadModel;
            _globalScope = globalScope;
            _windowManager = windowManager;
            _dispatcherProvider = dispatcherProvider;
            _opticalEventsDoubleViewModel = opticalEventsDoubleViewModel;
            _networkEventsDoubleViewModel = networkEventsDoubleViewModel;
            _bopNetworkEventsDoubleViewModel = bopNetworkEventsDoubleViewModel;
            _currentlyHiddenRtu = currentlyHiddenRtu;
            _readModel = readModel;
            SubscribeActualEventsRowChanged();
            SelectedTabIndex = 0;
        }

        private void SubscribeActualEventsRowChanged()
        {
            _opticalEventsDoubleViewModel.ActualOpticalEventsViewModel.Rows.CollectionChanged += (s, e) =>
            {
                IsThereActualOpticalEventsPictogram =
                    _opticalEventsDoubleViewModel.ActualOpticalEventsViewModel.Rows.Any()
                        ? Visibility.Visible
                        : Visibility.Hidden;
                NotifyOfPropertyChange(nameof(IsThereActualOpticalEventsPictogram));
            };
            _networkEventsDoubleViewModel.ActualNetworkEventsViewModel.Rows.CollectionChanged += (s, e) =>
            {
                IsThereActualNetworkEventsPictogram =
                    _networkEventsDoubleViewModel.ActualNetworkEventsViewModel.Rows.Any()
                        ? Visibility.Visible
                        : Visibility.Hidden;
                NotifyOfPropertyChange(nameof(IsThereActualNetworkEventsPictogram));
            };
            _bopNetworkEventsDoubleViewModel.ActualBopNetworkEventsViewModel.Rows.CollectionChanged += (s, e) =>
            {
                IsThereActualBopEventsPictogram =
                    _bopNetworkEventsDoubleViewModel.ActualBopNetworkEventsViewModel.Rows.Any()
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
            foreach (var trace in _readModel.Traces)
                trace.IsHighlighted = false;
        }

        public async void ShowAllGraph()
        {
            var vm = MyMessageBoxExt.DrawingGraph();
            _windowManager.ShowWindowWithAssignedOwner(vm);
            await ShowAllLongOperation();
            await _dispatcherProvider.GetDispatcher().InvokeAsync(() => vm.TryClose(), DispatcherPriority.ApplicationIdle);
        }

        private async Task ShowAllLongOperation()
        {
            await Task.Delay(1); // just to get rid of warning
            using (_globalScope.Resolve<IWaitCursor>())
            {
                _currentlyHiddenRtu.Collection.Clear();
                _currentlyHiddenRtu.IsShowAllPressed = true;
            }
        }

        public async void HideAllGraph()
        {
            var vm = MyMessageBoxExt.DrawingGraph();
            _windowManager.ShowWindowWithAssignedOwner(vm);

            await HideAllLongOperationAsync();
            await _dispatcherProvider.GetDispatcher().InvokeAsync(() => vm.TryClose(), DispatcherPriority.ApplicationIdle);
        }

        private async Task HideAllLongOperationAsync()
        {
            await Task.Delay(1); // just to get rid of warning
            using (_globalScope.Resolve<IWaitCursor>())
            {
                var rtuToHide = _readModel.Rtus.Where(r => !_currentlyHiddenRtu.Collection.Contains(r.Id))
                    .Select(rr => rr.Id);
                _currentlyHiddenRtu.Collection.AddRange(rtuToHide);

                _currentlyHiddenRtu.IsHideAllPressed = true;
            }
        } 
    }
}