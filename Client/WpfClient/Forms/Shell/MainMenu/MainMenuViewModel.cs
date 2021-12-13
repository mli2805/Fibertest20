using Autofac;
using Caliburn.Micro;

namespace Iit.Fibertest.Client
{
    public partial class MainMenuViewModel : PropertyChangedBase
    {
        private readonly ILifetimeScope _globalScope;
        private readonly IWindowManager _windowManager;
        private readonly IMyLog _logFile;
        private readonly Model _readModel;
        private readonly CurrentlyHiddenRtu _currentlyHiddenRtu;
        private readonly ComponentsReportViewModel _componentsReportViewModel;
        private readonly OpticalEventsReportViewModel _opticalEventsReportViewModel;

        private Role _currentUserRole = Role.Supervisor;
        public Role CurrentUserRole
        {
            get => _currentUserRole;
            set
            {
                if (Equals(value, _currentUserRole)) return;
                _currentUserRole = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(DeveloperMenuItemsVisibility));
            }
        }

        public Visibility DeveloperMenuItemsVisibility =>
            _currentUserRole == Role.Developer ? Visibility.Visible : Visibility.Collapsed;

        public MainMenuViewModel(ILifetimeScope globalScope, IWindowManager windowManager,
            IMyLog logFile, Model readModel, CurrentlyHiddenRtu currentlyHiddenRtu,
            ComponentsReportViewModel componentsReportViewModel, OpticalEventsReportViewModel opticalEventsReportViewModel)
        {
            _globalScope = globalScope;
            _windowManager = windowManager;
            _logFile = logFile;
            _readModel = readModel;
            _currentlyHiddenRtu = currentlyHiddenRtu;
            _componentsReportViewModel = componentsReportViewModel;
            _opticalEventsReportViewModel = opticalEventsReportViewModel;
        }

         public async void ImportRtuFromFolder()
        {
            var basePath = AppDomain.CurrentDomain.BaseDirectory;
            var folder = Path.GetFullPath(Path.Combine(basePath, @"..\temp\"));
            string[] files = Directory.GetFiles(folder, "*.brtu");

            foreach (var filename in files)
            {
                var bytes = File.ReadAllBytes(filename);
                var oneRtuGraphModel = new Model();
                if (!await oneRtuGraphModel.Deserialize(_logFile, bytes)) return;
                _readModel.AddOneRtuToModel(oneRtuGraphModel);
            }

            _currentlyHiddenRtu.Collection.AddRange(_readModel.Rtus.Select(r=>r.Id));
            _currentlyHiddenRtu.IsHideAllPressed = true;
        }
    }
}