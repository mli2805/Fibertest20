using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Caliburn.Micro;

namespace Iit.Fibertest.Client
{
    public class TabulatorViewModel : PropertyChangedBase
    {
        private int _selectedTabIndex;
        public int SelectedTabIndex
        {
            get => _selectedTabIndex;
            set
            {
                if (value == _selectedTabIndex) return;
                _selectedTabIndex = value;
                ChangeTabVisibility();
            }
        }


        public ImageSource IsThereActualOpticalEventsPictogram => GetPictogram();
        private ImageSource GetPictogram()
        {
//            return new BitmapImage(new Uri("pack://application:,,,/Resources/LeftPanel/EmptySquare.png"));
            return new BitmapImage(new Uri("pack://application:,,,/Resources/LeftPanel/RedSquare.png"));
        }


        #region Visibilities

        private Visibility _opticalEventsVisibility;
        public Visibility OpticalEventsVisibility
        {
            get { return _opticalEventsVisibility; }
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
            get { return _networkEventsVisibility; }
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
            get { return _bopNetworkEventsVisibility; }
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

        #endregion

        public TabulatorViewModel()
        {
            SelectedTabIndex = 3;
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
                    break;
                case 1:
                    OpticalEventsVisibility = Visibility.Collapsed;
                    NetworkEventsVisibility = Visibility.Visible;
                    BopNetworkEventsVisibility = Visibility.Collapsed;
                    MapVisibility = Visibility.Collapsed;
                    break;
                case 2:
                    OpticalEventsVisibility = Visibility.Collapsed;
                    NetworkEventsVisibility = Visibility.Collapsed;
                    BopNetworkEventsVisibility = Visibility.Visible;
                    MapVisibility = Visibility.Collapsed;
                    break;
                case 3:
                    OpticalEventsVisibility = Visibility.Collapsed;
                    NetworkEventsVisibility = Visibility.Collapsed;
                    BopNetworkEventsVisibility = Visibility.Collapsed;
                    MapVisibility = Visibility.Visible;
                    break;
            }
        }

    }
}
