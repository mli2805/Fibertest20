using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Autofac;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WpfCommonViews;

namespace Iit.Fibertest.Client
{
    public class AutoBaseViewModel : Screen
    {
        private readonly ILifetimeScope _globalScope;
        private readonly IMyLog _logFile;
        private readonly Model _readModel;
        private readonly IWindowManager _windowManager;
        private readonly IDispatcherProvider _dispatcherProvider;
        private readonly ReflectogramManager _reflectogramManager;
        private TraceLeaf _traceLeaf;

        public bool IsOpen { get; set; }
        public IOneMeasurementExecutor OneMeasurementExecutor { get; set; }
        public bool IsShowRef { get; set; }

        public AutoBaseViewModel(ILifetimeScope globalScope, IMyLog logFile, Model readModel, 
            IWindowManager windowManager, IDispatcherProvider dispatcherProvider,
            ReflectogramManager reflectogramManager)
        {
            _globalScope = globalScope;
            _logFile = logFile;
            _readModel = readModel;
            _windowManager = windowManager;
            _dispatcherProvider = dispatcherProvider;
            _reflectogramManager = reflectogramManager;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Assign_base_refs_automatically;
            IsOpen = true;
            OneMeasurementExecutor.MeasurementCompleted += OneMeasurementExecutor_MeasurementCompleted;
        }

        public bool Initialize(TraceLeaf traceLeaf)
        {
            _traceLeaf = traceLeaf;

            var trace = _readModel.Traces.First(t => t.TraceId == traceLeaf.Id);
            var rtu = _readModel.Rtus.First(r => r.Id == trace.RtuId);

            OneMeasurementExecutor = rtu.RtuMaker == RtuMaker.IIT 
                ? (IOneMeasurementExecutor)_globalScope.Resolve<OneMeasurementExecutor>()
                : _globalScope.Resolve<OneVeexMeasurementExecutor>();
            if (!OneMeasurementExecutor.Initialize(rtu, false))
                return false;

            IsShowRef = true;
            OneMeasurementExecutor.Model.IsEnabled = true;

            return true;
        }

        private void OneMeasurementExecutor_MeasurementCompleted(object sender, EventArgs e)
        {
            var result = (MeasurementEventArgs)e;
            _logFile.AppendLine($@"Measurement on trace {_traceLeaf.Title}: {result.Code}");

            _dispatcherProvider.GetDispatcher().Invoke(() => Finish(result));
        }

        private void Finish(MeasurementEventArgs result)
        {
            _waitCursor.Dispose();
            OneMeasurementExecutor.Model.MeasurementProgressViewModel.ControlVisibility = Visibility.Collapsed;
            OneMeasurementExecutor.Model.IsEnabled = true;

            if (result.Code == ReturnCode.BaseRefAssignedSuccessfully)
            {
                if (IsShowRef)
                    _reflectogramManager.ShowClientMeasurement(result.SorBytes);
                TryClose();
            }
            else
            {
                var strings = new List<string>() { result.Code.GetLocalizedString() };
                if (result.AdditionalErrorLines[0] != "")
                    strings.AddRange(result.AdditionalErrorLines);
                var vm = new MyMessageBoxViewModel(MessageType.Error, strings, 0);
                _windowManager.ShowDialogWithAssignedOwner(vm);
            }
        }

        private WaitCursor _waitCursor;
        public async void Start()
        {
            _waitCursor = new WaitCursor();
            OneMeasurementExecutor.Model.IsEnabled = false;
            await OneMeasurementExecutor.Start(_traceLeaf);
        }


        public void Close()
        {
            TryClose();
        }

        public override void CanClose(Action<bool> callback)
        {
            IsOpen = false;
            OneMeasurementExecutor.MeasurementCompleted -= OneMeasurementExecutor_MeasurementCompleted;
            callback(true);
        }
    }
}
