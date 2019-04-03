using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.WpfCommonViews;
using Newtonsoft.Json;
using NEventStore;

namespace Iit.Fibertest.Client
{
    public class EventLogViewModel : Screen
    {
        private readonly ILocalDbManager _localDbManager;
        private readonly EventToLogLineParser _eventToLogLineParser;
        private readonly Model _readModel;
        private readonly LogOperationsViewModel _logOperationsViewModel;
        private readonly IWindowManager _windowManager;
        private UserFilter _selectedUserFilter;
        private string _operationsFilterButtonContent;

        private static readonly JsonSerializerSettings JsonSerializerSettings =
            new JsonSerializerSettings {TypeNameHandling = TypeNameHandling.All};

        public string OperationsFilterButtonContent
        {
            get => _operationsFilterButtonContent;
            set
            {
                if (value == _operationsFilterButtonContent) return;
                _operationsFilterButtonContent = value;
                NotifyOfPropertyChange();
            }
        }

        public List<UserFilter> UserFilters { get; set; }

        public UserFilter SelectedUserFilter
        {
            get => _selectedUserFilter;
            set
            {
                if (Equals(value, _selectedUserFilter)) return;
                _selectedUserFilter = value;
                NotifyOfPropertyChange();
                var view = CollectionViewSource.GetDefaultView(Rows);
                view.Refresh();
            }
        }

        public List<LogLine> Rows { get; set; }

        public EventLogViewModel(ILocalDbManager localDbManager, EventToLogLineParser eventToLogLineParser,
            Model readModel, LogOperationsViewModel logOperationsViewModel, IWindowManager windowManager)
        {
            _localDbManager = localDbManager;
            _eventToLogLineParser = eventToLogLineParser;
            _readModel = readModel;
            _logOperationsViewModel = logOperationsViewModel;
            _windowManager = windowManager;
        }

        private void InitializeFilters()
        {
            UserFilters = new List<UserFilter>() {new UserFilter()};
            foreach (var user in _readModel.Users.Where(u => u.Role >= Role.Root && u.Role <= Role.Superclient))
                UserFilters.Add(new UserFilter(user));
            SelectedUserFilter = UserFilters.First();

            _logOperationsViewModel.IsAll = true;
            OperationsFilterButtonContent = Resources.SID__no_filter_;
        }

        protected override void OnViewLoaded(object o)
        {
            DisplayName = Resources.SID_User_operations_log;
            var view = CollectionViewSource.GetDefaultView(Rows);
            view.Filter += OnFilter;
            view.SortDescriptions.Add(new SortDescription(@"Ordinal", ListSortDirection.Descending));
        }

        private bool OnFilter(object o)
        {
            var logLine = (LogLine) o;
            return
                (SelectedUserFilter.IsOn == false ||
                 SelectedUserFilter.User.Title == logLine.Username)
                && IsIncludedInOperationFilter(logLine.OperationCode);
        }

        private bool IsIncludedInOperationFilter(LogOperationCode operationCode)
        {
            switch (operationCode)
            {
                case LogOperationCode.ClientStarted: return _logOperationsViewModel.IsClientStarted;
                case LogOperationCode.ClientExited: return _logOperationsViewModel.IsClientExited;
                case LogOperationCode.ClientConnectionLost: return _logOperationsViewModel.IsClientConnectionLost;

                case LogOperationCode.RtuAdded: return _logOperationsViewModel.IsRtuAdded;
                case LogOperationCode.RtuUpdated: return _logOperationsViewModel.IsRtuUpdated;
                case LogOperationCode.RtuInitialized: return _logOperationsViewModel.IsRtuInitialized;
                case LogOperationCode.RtuRemoved: return _logOperationsViewModel.IsRtuRemoved;

                case LogOperationCode.TraceAdded: return _logOperationsViewModel.IsTraceAdded;
                case LogOperationCode.TraceUpdated: return _logOperationsViewModel.IsTraceUpdated;
                case LogOperationCode.TraceAttached: return _logOperationsViewModel.IsTraceAttached;
                case LogOperationCode.TraceDetached: return _logOperationsViewModel.IsTraceDetached;
                case LogOperationCode.TraceCleaned: return _logOperationsViewModel.IsTraceCleaned;
                case LogOperationCode.TraceRemoved: return _logOperationsViewModel.IsTraceRemoved;

                case LogOperationCode.BaseRefAssigned: return _logOperationsViewModel.IsBaseRefAssined;
                case LogOperationCode.MonitoringSettingsChanged:
                    return _logOperationsViewModel.IsMonitoringSettingsChanged;
                case LogOperationCode.MonitoringStarted: return _logOperationsViewModel.IsMonitoringStarted;
                case LogOperationCode.MonitoringStopped: return _logOperationsViewModel.IsMonitoringStopped;

                case LogOperationCode.MeasurementUpdated: return _logOperationsViewModel.IsMeasurementUpdated;

                default: return false;
            }
        }

        // do before show form!
        public async Task Initialize()
        {
            Rows = new List<LogLine>();
            InitializeFilters();

            _eventToLogLineParser.Initialize();
            var jsonsInCache = await _localDbManager.LoadEvents();
            var ordinal = 1;
            try
            {
                foreach (var json in jsonsInCache)
                {
                    var msg = (EventMessage) JsonConvert.DeserializeObject(json, JsonSerializerSettings);
                    var username = (string) msg.Headers[@"Username"];
                    var user = _readModel.Users.FirstOrDefault(u => u.Title == username);

                    var line = _eventToLogLineParser.ParseEventBody(msg.Body);
                    if (line == null) continue;

                    if (user == null || user.Role < Role.Root) continue;

                    line.Ordinal = ordinal;
                    line.Username = username;
                    line.ClientIp = (string) msg.Headers[@"ClientIp"];
                    line.Timestamp = (DateTime) msg.Headers[@"Timestamp"];
                    Rows.Insert(0, line);
                    ordinal++;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void ShowOperationFilter()
        {
            _windowManager.ShowDialogWithAssignedOwner(_logOperationsViewModel);
            OperationsFilterButtonContent = _logOperationsViewModel.IsAllChecked()
                ? Resources.SID__no_filter_
                : Resources.SID__filter_applied_;
            var view = CollectionViewSource.GetDefaultView(Rows);
            view.Refresh();
        }

        public void Close()
        {
            TryClose();
        }
    }
}