﻿using System.Linq;
using System.Windows;
using Caliburn.Micro;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public class TabulatorViewModel : PropertyChangedBase
    {
        public GraphReadModel GraphReadModel { get; }
        private readonly OpticalEventsDoubleViewModel _opticalEventsDoubleViewModel;
        private readonly NetworkEventsDoubleViewModel _networkEventsDoubleViewModel;
        private readonly BopNetworkEventsDoubleViewModel _bopNetworkEventsDoubleViewModel;
        private readonly CurrentGis _currentGis;
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
                NotifyOfPropertyChange(nameof(ButtonVisibility));
            }
        }

        public Visibility ButtonVisibility =>
            MapVisibility == Visibility.Visible && !_currentGis.IsBigGraphMode
                ? Visibility.Visible
                : Visibility.Collapsed;

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

        public TabulatorViewModel(OpticalEventsDoubleViewModel opticalEventsDoubleViewModel,
            NetworkEventsDoubleViewModel networkEventsDoubleViewModel,
            BopNetworkEventsDoubleViewModel bopNetworkEventsDoubleViewModel,
            GraphReadModel graphReadModel, 
            CurrentGis currentGis, Model readModel)
        {
            GraphReadModel = graphReadModel;
            _opticalEventsDoubleViewModel = opticalEventsDoubleViewModel;
            _networkEventsDoubleViewModel = networkEventsDoubleViewModel;
            _bopNetworkEventsDoubleViewModel = bopNetworkEventsDoubleViewModel;
            _currentGis = currentGis;
            _currentGis.PropertyChanged += _currentGis_PropertyChanged;
            _readModel = readModel;
            SubscribeActualEventsRowChanged();
            SelectedTabIndex = 0;
        }

        private async void _currentGis_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsBigGraphMode")
            {
                NotifyOfPropertyChange(nameof(ButtonVisibility));
                await GraphReadModel.RefreshVisiblePart();
            }
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

        public void ExtinguishAll()
        {
            GraphReadModel.ExtinguishAll();
            foreach (var rtu in _readModel.Rtus)
                rtu.IsHighlighted = false;
        }

        public void ShowAllGraph()
        {
            foreach (var rtu in _readModel.Rtus)
                rtu.IsHighlighted = true;
            foreach (var trace in _readModel.Traces)
                if (!_currentGis.Traces.Contains(trace))
                    _currentGis.Traces.Add(trace);
        }

    }
}