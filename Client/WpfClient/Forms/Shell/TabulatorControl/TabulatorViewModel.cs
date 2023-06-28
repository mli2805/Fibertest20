using System.Linq;
using System.Windows;
using Autofac;
using Caliburn.Micro;

namespace Iit.Fibertest.Client
{
    public class TabulatorViewModel : PropertyChangedBase
    {
        public GraphReadModel GraphReadModel { get; }
        private readonly ILifetimeScope _globalScope;
        private readonly OpticalEventsDoubleViewModel _opticalEventsDoubleViewModel;
        private readonly NetworkEventsDoubleViewModel _networkEventsDoubleViewModel;
        private readonly BopNetworkEventsDoubleViewModel _bopNetworkEventsDoubleViewModel;
        private readonly RtuAccidentsDoubleViewModel _rtuAccidentsDoubleViewModel;

        private int _selectedTabIndex;

        public int SelectedTabIndex
        {
            get => _selectedTabIndex;
            set
            {
                //                if (value == _selectedTabIndex) return;
                _selectedTabIndex = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(IsGisOpen));
                ChangeTabVisibility();
            }
        }

        public bool IsGisOpen => SelectedTabIndex == 4;

        public void OpenGis() { SelectedTabIndex = 4; }
        public void OpenGisIfNotYet() { if (SelectedTabIndex != 4) SelectedTabIndex = 4; }

        #region Pictograms

        public Visibility IsThereActualOpticalEventsPictogram { get; set; } = Visibility.Hidden;
        public Visibility IsThereActualNetworkEventsPictogram { get; set; } = Visibility.Hidden;
        public Visibility IsThereActualBopEventsPictogram { get; set; } = Visibility.Hidden;
        public Visibility IsThereActualRtuStatusEventsPictogram { get; set; } = Visibility.Hidden;

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

        private Visibility _rtuStatusEventsVisibility;
        public Visibility RtuStatusEventsVisibility
        {
            get => _rtuStatusEventsVisibility;
            set
            {
                if (value == _rtuStatusEventsVisibility) return;
                _rtuStatusEventsVisibility = value;
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
                // NotifyOfPropertyChange(nameof(ButtonVisibility));
            }
        }

        public Visibility ButtonVisibility => Visibility.Collapsed;
        // MapVisibility == Visibility.Visible && !_currentGis.IsHighDensityGraph
        // ? Visibility.Visible
        // : Visibility.Collapsed;

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

        public TabulatorViewModel(ILifetimeScope globalScope, OpticalEventsDoubleViewModel opticalEventsDoubleViewModel,
            NetworkEventsDoubleViewModel networkEventsDoubleViewModel,
            BopNetworkEventsDoubleViewModel bopNetworkEventsDoubleViewModel,
            RtuAccidentsDoubleViewModel rtuAccidentsDoubleViewModel,
            GraphReadModel graphReadModel)
        {
            GraphReadModel = graphReadModel;
            _globalScope = globalScope;
            _opticalEventsDoubleViewModel = opticalEventsDoubleViewModel;
            _networkEventsDoubleViewModel = networkEventsDoubleViewModel;
            _bopNetworkEventsDoubleViewModel = bopNetworkEventsDoubleViewModel;
            _rtuAccidentsDoubleViewModel = rtuAccidentsDoubleViewModel;
            SubscribeActualEventsRowChanged();
            SelectedTabIndex = 0;
        }

        // private async void _currentGis_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        // {
        //     if (e.PropertyName == @"IsHighDensityGraph")
        //     {
        //         NotifyOfPropertyChange(nameof(ButtonVisibility));
        //         await GraphReadModel.RefreshVisiblePart();
        //     }
        // }

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
            _rtuAccidentsDoubleViewModel.ActualRtuAccidentsViewModel.Rows.CollectionChanged += (s, e) =>
            {
                IsThereActualRtuStatusEventsPictogram =
                    _rtuAccidentsDoubleViewModel.ActualRtuAccidentsViewModel.Rows.Any()
                        ? Visibility.Visible
                        : Visibility.Hidden;
                NotifyOfPropertyChange(nameof(IsThereActualRtuStatusEventsPictogram));
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
                    RtuStatusEventsVisibility = Visibility.Collapsed;
                    MapVisibility = Visibility.Collapsed;
                    MessageVisibility = Visibility.Collapsed;
                    break;
                case 1:
                    OpticalEventsVisibility = Visibility.Collapsed;
                    NetworkEventsVisibility = Visibility.Visible;
                    BopNetworkEventsVisibility = Visibility.Collapsed;
                    RtuStatusEventsVisibility = Visibility.Collapsed;
                    MapVisibility = Visibility.Collapsed;
                    MessageVisibility = Visibility.Collapsed;
                    break;
                case 2:
                    OpticalEventsVisibility = Visibility.Collapsed;
                    NetworkEventsVisibility = Visibility.Collapsed;
                    BopNetworkEventsVisibility = Visibility.Visible;
                    RtuStatusEventsVisibility = Visibility.Collapsed;
                    MapVisibility = Visibility.Collapsed;
                    MessageVisibility = Visibility.Collapsed;
                    break;
                case 3:
                    OpticalEventsVisibility = Visibility.Collapsed;
                    NetworkEventsVisibility = Visibility.Collapsed;
                    BopNetworkEventsVisibility = Visibility.Collapsed;
                    RtuStatusEventsVisibility = Visibility.Visible;
                    MapVisibility = Visibility.Collapsed;
                    MessageVisibility = Visibility.Collapsed;
                    break;
                case 4:
                    OpticalEventsVisibility = Visibility.Collapsed;
                    NetworkEventsVisibility = Visibility.Collapsed;
                    BopNetworkEventsVisibility = Visibility.Collapsed;
                    RtuStatusEventsVisibility = Visibility.Collapsed;
                    MapVisibility = Visibility.Visible;
                    MessageVisibility = Visibility.Collapsed;
                    break;
                case 5:
                    OpticalEventsVisibility = Visibility.Collapsed;
                    NetworkEventsVisibility = Visibility.Collapsed;
                    BopNetworkEventsVisibility = Visibility.Collapsed;
                    RtuStatusEventsVisibility = Visibility.Collapsed;
                    MapVisibility = Visibility.Collapsed;
                    MessageVisibility = Visibility.Visible;
                    break;
            }
        }

        public async void ExtinguishAll()
        {
            var vm = _globalScope.Resolve<TraceStepByStepViewModel>();
            if (vm.IsOpen) return;

            GraphReadModel.ExtinguishAll();

            await GraphReadModel.RefreshVisiblePart();
        }

    }
}