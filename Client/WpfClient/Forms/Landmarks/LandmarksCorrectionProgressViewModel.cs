using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Autofac;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.WcfConnections;
using Iit.Fibertest.WpfCommonViews;

namespace Iit.Fibertest.Client
{
    public class LandmarksCorrectionProgressViewModel : Screen
    {
        private readonly ILifetimeScope _globalScope;
        private readonly Model _readModel;
        private readonly IWcfServiceCommonC2D _wcfCommonManager;

        #region visual properties
        private Visibility _progressBarVisibility;
        public Visibility ProgressBarVisibility
        {
            get => _progressBarVisibility;
            set
            {
                if (value == _progressBarVisibility) return;
                _progressBarVisibility = value;
                NotifyOfPropertyChange();
            }
        }

        public ObservableCollection<string> Messages { get; set; } = new ObservableCollection<string>();
        #endregion

        public LandmarksCorrectionProgressViewModel(ILifetimeScope globalScope, Model readModel,
            IWcfServiceCommonC2D wcfCommonManager)
        {
            _globalScope = globalScope;
            _readModel = readModel;
            _wcfCommonManager = wcfCommonManager;
        }

        protected override async void OnViewLoaded(object view)
        {
            while (true)
            {
                await Task.Delay(1000);
                using (_globalScope.Resolve<IWaitCursor>())
                {
                    var result = await _wcfCommonManager.GetLandmarksCorrectionProgress(_batchId);
                    if (result == null) continue;
                    SetProgress(result);
                    if (result.AllTracesInvolved == result.TracesCorrected)
                        break;
                }
            }
        }

        private Guid _batchId;
        public void SetProgress(CorrectionProgressDto dto)
        {
            if (dto == null) return;
            _batchId = dto.BatchId;

            var trace = _readModel.Traces.FirstOrDefault(t => t.TraceId == dto.TraceId);
            var traceStr = trace != null ? $"Trace: {trace.Title}  " : "";
            Messages.Add($"{traceStr}{dto.ReturnCode.GetLocalizedString()}");
            Messages.Add($"Processed {dto.TracesCorrected} / {dto.AllTracesInvolved} traces");

            ProgressBarVisibility = dto.AllTracesInvolved == dto.TracesCorrected
                ? Visibility.Hidden : Visibility.Visible;
        }
    }
}
