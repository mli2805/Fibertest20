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
        private readonly ReflectogramManager _reflectogramManager;
        private TraceLeaf _traceLeaf;

        public OneMeasurementExecutor OneMeasurementExecutor { get; }
        public bool IsShowRef { get; set; }

        public AutoBaseViewModel(IMyLog logFile, Model readModel, IWindowManager windowManager,
            OneMeasurementExecutor oneMeasurementExecutor, ReflectogramManager reflectogramManager)
        {
            _logFile = logFile;
            _readModel = readModel;
            _windowManager = windowManager;
            OneMeasurementExecutor = oneMeasurementExecutor;
            _reflectogramManager = reflectogramManager;
        }

        public bool Initialize(TraceLeaf traceLeaf)
        {
            _traceLeaf = traceLeaf;

            var trace = _readModel.Traces.First(t => t.TraceId == traceLeaf.Id);
            var rtu = _readModel.Rtus.First(r => r.Id == trace.RtuId);

            if (!OneMeasurementExecutor.Initialize(rtu, false))
                return false;

            IsShowRef = true;
            OneMeasurementExecutor.Model.IsEnabled = true;

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
            OneMeasurementExecutor.MeasurementCompleted += OneMeasurementExecutor_MeasurementCompleted;
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
            OneMeasurementExecutor.Model.IsOpen = false;
            OneMeasurementExecutor.MeasurementCompleted -= OneMeasurementExecutor_MeasurementCompleted;
            callback(true);
        }
    }
}
