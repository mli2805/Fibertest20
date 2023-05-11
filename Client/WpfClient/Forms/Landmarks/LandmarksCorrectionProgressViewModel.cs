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
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;
using Iit.Fibertest.WpfCommonViews;

namespace Iit.Fibertest.Client
{
    public class LandmarksCorrectionProgressViewModel : Screen
    {
        private readonly ILifetimeScope _globalScope;
        private readonly IMyLog _logFile;
        private readonly Model _readModel;
        private readonly IWcfServiceCommonC2D _wcfCommonManager;
        private readonly IWindowManager _windowManager;

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

        public LandmarksCorrectionProgressViewModel(ILifetimeScope globalScope, IMyLog logFile, Model readModel,
            IWcfServiceCommonC2D wcfCommonManager, IWindowManager windowManager)
        {
            _globalScope = globalScope;
            _logFile = logFile;
            _readModel = readModel;
            _wcfCommonManager = wcfCommonManager;
            _windowManager = windowManager;
        }

        private LandmarksCorrectionDto _dto;
        public void Initialize(LandmarksCorrectionDto dto)
        {
            _dto = dto;
        }

        protected override async void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Saving_landmark_changes;
            var result = await SendCommand(_dto);
            if (result == null) return;
            UpdateProgressView(result);
            await GetUpdates();
        }

        public bool IsSentSuccessfully;
        private async Task<CorrectionProgressDto> SendCommand(LandmarksCorrectionDto dto)
        {
            CorrectionProgressDto result;
            using (_globalScope.Resolve<IWaitCursor>())
            {
                result = await _wcfCommonManager.StartLandmarksCorrection(dto);
                _logFile.AppendLine($@"{result.ReturnCode}");
                if (result.ReturnCode != ReturnCode.LandmarkChangesAppliedSuccessfully)
                {
                    var em = new MyMessageBoxViewModel(MessageType.Error, $@"{result.ErrorMessage}");
                    _windowManager.ShowDialogWithAssignedOwner(em);
                    return null;
                }
            }

            IsSentSuccessfully = true;
            return result;
        }

        private async Task GetUpdates()
        {
            while (true)
            {
                await Task.Delay(1000);
                using (_globalScope.Resolve<IWaitCursor>())
                {
                    var result = await _wcfCommonManager.GetLandmarksCorrectionProgress(_batchId);
                    if (result == null) continue;
                    UpdateProgressView(result);
                    if (result.AllTracesInvolved == result.TracesCorrected)
                        break;
                }
            }
        }

        private Guid _batchId;
        private int _problemCount;

        private void UpdateProgressView(CorrectionProgressDto dto)
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
