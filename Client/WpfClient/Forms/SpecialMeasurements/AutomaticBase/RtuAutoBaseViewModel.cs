using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.Client
{
    public class RtuAutoBaseViewModel : Screen
    {
        private readonly IMyLog _logFile;
        private readonly Model _readModel;
        public readonly OneMeasurementExecutor OneMeasurementExecutor;
        private List<TraceLeaf> _traceLeaves;

        public OneMeasurementModel Model { get; set; } = new OneMeasurementModel();

        public RtuAutoBaseViewModel(IniFile iniFile, IMyLog logFile, CurrentUser currentUser, Model readModel,
            AutoAnalysisParamsViewModel autoAnalysisParamsViewModel,
            OneMeasurementExecutor oneMeasurementExecutor
            )
        {
            _logFile = logFile;
            _readModel = readModel;
            OneMeasurementExecutor = oneMeasurementExecutor;

            Model.CurrentUser = currentUser;
            Model.MeasurementTimeout = iniFile.Read(IniSection.Miscellaneous, IniKey.MeasurementTimeoutMs, 60000);
            Model.AutoAnalysisParamsViewModel = autoAnalysisParamsViewModel;
            Model.OtdrParametersTemplatesViewModel = new OtdrParametersTemplatesViewModel(iniFile);

        }

        public bool Initialize(RtuLeaf rtuLeaf)
        {
            _traceLeaves = rtuLeaf.GetAttachedTraces();

            Model.Rtu = _readModel.Rtus.First(r => r.Id == rtuLeaf.Id);
            Model.OtdrParametersTemplatesViewModel.Initialize(Model.Rtu, true);
            if (!Model.AutoAnalysisParamsViewModel.Initialize())
                return false;
            Model.MeasurementProgressViewModel = new MeasurementProgressViewModel();
            Model.ShouldStartMonitoring = true;
            Model.IsEnabled = true;

            OneMeasurementExecutor.Model = Model;
            OneMeasurementExecutor.MeasurementCompleted += OneMeasurementExecutor_MeasurementCompleted;

            return true;
        }

        private void OneMeasurementExecutor_MeasurementCompleted(object sender, EventArgs e)
        {
            _logFile.AppendLine(@"Measurement on trace {_traceLeaves[0].Title} completed!");
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Assign_base_refs_automatically;
        }

        public async void Start()
        {
            var result = await OneMeasurementExecutor.Start(_traceLeaves[0]);
            _logFile.AppendLine($@"Measurement on trace {_traceLeaves[0].Title} started: {result}");
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
