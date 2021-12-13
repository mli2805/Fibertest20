using System.Windows;
using Autofac;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.WcfConnections;

namespace Iit.Fibertest.Client
{
    public partial class MainMenuViewModel : PropertyChangedBase
    {
        private readonly ILifetimeScope _globalScope;
        private readonly IWindowManager _windowManager;
        private readonly IWcfServiceDesktopC2D _wcfDesktopC2D;
        private readonly ComponentsReportViewModel _componentsReportViewModel;
        private readonly OpticalEventsReportViewModel _opticalEventsReportViewModel;
      
        private CurrentUser _currentUser = new CurrentUser();
        public CurrentUser CurrentUser
        {
            get => _currentUser;
            set
            {
                if (Equals(value, _currentUser)) return;
                _currentUser = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(ServiceMenuVisibility));
            }
        }

        public Visibility ServiceMenuVisibility =>
            CurrentUser.Role == Role.Developer ? Visibility.Visible : Visibility.Collapsed;

        public MainMenuViewModel(ILifetimeScope globalScope, IWindowManager windowManager, IWcfServiceDesktopC2D wcfDesktopC2D,
            ComponentsReportViewModel componentsReportViewModel, OpticalEventsReportViewModel opticalEventsReportViewModel)
        {
            _globalScope = globalScope;
            _windowManager = windowManager;
            _wcfDesktopC2D = wcfDesktopC2D;
            _componentsReportViewModel = componentsReportViewModel;
            _opticalEventsReportViewModel = opticalEventsReportViewModel;
        }
    }
}