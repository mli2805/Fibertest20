using System.Windows;
using Autofac;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;

namespace Iit.Fibertest.Client
{
    public partial class MainMenuViewModel : PropertyChangedBase
    {
        private readonly ILifetimeScope _globalScope;
        private readonly IMyLog _logFile;
        private readonly IWindowManager _windowManager;
        private readonly IWcfServiceDesktopC2D _wcfDesktopC2D;
        private readonly ComponentsReportViewModel _componentsReportViewModel;
        private readonly OpticalEventsReportViewModel _opticalEventsReportViewModel;
        private readonly ModelFromFileExporter _modelFromFileExporter;

        private CurrentUser _currentUser = new CurrentUser();
        public CurrentUser CurrentUser
        {
            get => _currentUser;
            set
            {
                if (Equals(value, _currentUser)) return;
                _currentUser = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(ToolsMenuVisibility));
            }
        }

        public Visibility ToolsMenuVisibility =>
            CurrentUser.Role == Role.Developer ? Visibility.Visible : Visibility.Collapsed;

        public MainMenuViewModel(ILifetimeScope globalScope, IMyLog logFile, IWindowManager windowManager,
            IWcfServiceDesktopC2D wcfDesktopC2D,
            ComponentsReportViewModel componentsReportViewModel, OpticalEventsReportViewModel opticalEventsReportViewModel,
            ModelFromFileExporter modelFromFileExporter)
        {
            _globalScope = globalScope;
            _logFile = logFile;
            _windowManager = windowManager;
            _wcfDesktopC2D = wcfDesktopC2D;
            _componentsReportViewModel = componentsReportViewModel;
            _opticalEventsReportViewModel = opticalEventsReportViewModel;
            _modelFromFileExporter = modelFromFileExporter;
        }

     
    }
}