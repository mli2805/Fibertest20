using System;
using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WpfCommonViews;

namespace Iit.Fibertest.Client
{
    public class AutoBaseViewModel : Screen
    {
        private readonly IMyLog _logFile;
        private readonly Model _readModel;
        private readonly IWindowManager _windowManager;
        public readonly OneMeasurementExecutor OneMeasurementExecutor;
        private readonly ReflectogramManager _reflectogramManager;
        private TraceLeaf _traceLeaf;

        public OneMeasurementModel Model { get; set; } = new OneMeasurementModel();
        public bool IsShowRef { get; set; }

        public AutoBaseViewModel(IniFile iniFile, IMyLog logFile, CurrentUser currentUser, Model readModel,
            IWindowManager windowManager, AutoAnalysisParamsViewModel autoAnalysisParamsViewModel,
            OneMeasurementExecutor oneMeasurementExecutor, ReflectogramManager reflectogramManager
            )
        {
            _logFile = logFile;
            _readModel = readModel;
            _windowManager = windowManager;
            OneMeasurementExecutor = oneMeasurementExecutor;
            OneMeasurementExecutor.Model = Model;
            OneMeasurementExecutor.MeasurementCompleted += OneMeasurementExecutor_MeasurementCompleted;
            _reflectogramManager = reflectogramManager;

            Model.CurrentUser = currentUser;
            Model.MeasurementTimeout = iniFile.Read(IniSection.Miscellaneous, IniKey.MeasurementTimeoutMs, 60000);
            Model.AutoAnalysisParamsViewModel = autoAnalysisParamsViewModel;
            Model.OtdrParametersTemplatesViewModel = new OtdrParametersTemplatesViewModel(iniFile);
            Model.MeasurementProgressViewModel = new MeasurementProgressViewModel();
        }

        public bool Initialize(TraceLeaf traceLeaf)
        {
            _traceLeaf = traceLeaf;

            var trace = _readModel.Traces.First(t => t.TraceId == traceLeaf.Id);
            Model.Rtu = _readModel.Rtus.First(r => r.Id == trace.RtuId);

            Model.OtdrParametersTemplatesViewModel.Initialize(Model.Rtu, false);
            if (!Model.AutoAnalysisParamsViewModel.Initialize())
                return false;
            IsShowRef = true;
            Model.IsEnabled = true;

            return true;
        }

        private void OneMeasurementExecutor_MeasurementCompleted(object sender, EventArgs e)
        {
            var result = (MeasurementCompletedEventArgs)e;


            if (result.CompletedStatus == MeasurementCompletedStatus.BaseRefAssignedSuccessfully)
            {
                if (IsShowRef)
                    _reflectogramManager.ShowClientMeasurement(result.SorBytes);
                TryClose();
            }
            else
            {
                var vm = new MyMessageBoxViewModel(MessageType.Error, result.Lines, 0);
                _windowManager.ShowDialogWithAssignedOwner(vm);
            }
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Assign_base_refs_automatically;
        }

        public async void Start()
        {
            var result = await OneMeasurementExecutor.Start(_traceLeaf);
            _logFile.AppendLine($@"Measurement on trace {_traceLeaf.Title} started: {result}");
        }


        public void Close()
        {
            TryClose();
        }

        public override void CanClose(Action<bool> callback)
        {
            Model.IsOpen = false;
            callback(true);
        }
    }
}
