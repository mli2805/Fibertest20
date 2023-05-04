using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Autofac;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
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
        private Visibility _progressBarVisibility = Visibility.Visible;
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

        public string DoneMessage { get; set; }

        public ObservableCollection<LandmarksCorrectionProgressLine> Messages { get; set; } = 
            new ObservableCollection<LandmarksCorrectionProgressLine>();
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
            DisplayName = Resources.SID_Saving_landmark_changes;
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
        private int _problemCount;

        public void SetProgress(CorrectionProgressDto dto)
        {
            if (dto == null) return;
            _batchId = dto.BatchId;

            var trace = _readModel.Traces.FirstOrDefault(t => t.TraceId == dto.TraceId);
            var traceStr = trace != null ? string.Format(Resources.SID_Trace___0___, trace.Title) : "";
            if (dto.ReturnCode.GetColor() == @"red")
                _problemCount++;
            var line = new LandmarksCorrectionProgressLine($@"{traceStr}{dto.ReturnCode.GetLocalizedString()}", 
                dto.ReturnCode.GetColor());
            Messages.Add(line);
            var line2 = new LandmarksCorrectionProgressLine(
                string.Format(Resources.SID_Processed__0_____1__traces, dto.TracesCorrected, dto.AllTracesInvolved),
                @"black");
            Messages.Add(line2);

            DoneMessage = string.Format(Resources.SID_Attention__There_are__0__problems_, _problemCount);

            if (dto.AllTracesInvolved == dto.TracesCorrected)
            {
                if (_problemCount == 0)
                    TryClose();
                else
                {
                    ProgressBarVisibility = Visibility.Hidden;
                    NotifyOfPropertyChange(nameof(DoneMessage));
                }
            }
        }
    }
}
