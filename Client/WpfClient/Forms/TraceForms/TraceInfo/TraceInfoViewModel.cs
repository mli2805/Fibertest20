using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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
using Optixsoft.SorExaminer.OtdrDataFormat;


namespace Iit.Fibertest.Client
{
    public class TraceInfoViewModel : Screen, IDataErrorInfo
    {
        private readonly ILifetimeScope _globalScope;
        private readonly Model _readModel;
        private readonly CurrentUser _currentUser;
        private readonly IWcfServiceDesktopC2D _c2DWcfManager;
        private readonly IWcfServiceCommonC2D _c2DWcfCommonManager;
        private readonly IWindowManager _windowManager;
        private readonly CurrentGis _currentGis;
        private readonly GraphGpsCalculator _graphGpsCalculator;
        public bool IsSavePressed { get; set; }

        private bool _isInCreationMode;


        private string _title;
        public string Title
        {
            get => _title;
            set
            {
                if (value == _title) return;
                _title = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(IsButtonSaveEnabled));
            }
        }
        public TraceInfoModel Model { get; set; } = new TraceInfoModel();
        public bool IsEditEnabled { get; set; }

        public Visibility LengthVisibility { get; set; }
        public bool IsButtonSaveEnabled => IsEditEnabled && IsTitleValid() == string.Empty;

        private bool _isButtonsEnabled = true;
        public bool IsButtonsEnabled
        {
            get { return _isButtonsEnabled; }
            set
            {
                if (value == _isButtonsEnabled) return;
                _isButtonsEnabled = value;
                NotifyOfPropertyChange();
            }
        }

        public bool IsCreatedSuccessfully { get; set; }

        public TraceInfoViewModel(ILifetimeScope globalScope, Model readModel, CurrentUser currentUser,
            IWcfServiceDesktopC2D c2DWcfManager, IWcfServiceCommonC2D c2DWcfCommonManager, IWindowManager windowManager,
            CurrentGis currentGis, GraphGpsCalculator graphGpsCalculator)
        {
            _globalScope = globalScope;
            _readModel = readModel;
            _currentUser = currentUser;
            _c2DWcfManager = c2DWcfManager;
            _c2DWcfCommonManager = c2DWcfCommonManager;
            _windowManager = windowManager;
            _currentGis = currentGis;
            _graphGpsCalculator = graphGpsCalculator;
        }


        /// Setup traceId (for existing trace) or traceEquipments for trace creation moment
        public async Task Initialize(Guid traceId, List<Guid> traceEquipments, List<Guid> traceNodes, bool isInCreationMode)
        {
            _isInCreationMode = isInCreationMode;
            LengthVisibility = isInCreationMode ? Visibility.Collapsed : Visibility.Visible;
            Model.TraceId = traceId;
            Model.TraceEquipments = traceEquipments;
            Model.TraceNodes = traceNodes;
            Model.Rtu = _readModel.Rtus.First(r => r.Id == Model.TraceEquipments[0]);
            Model.RtuTitle = Model.Rtu.Title;
            Model.PortNumber = Resources.SID_not_attached;
            var dict = _readModel.BuildDictionaryByEquipmentType(Model.TraceEquipments);
            Model.EquipmentsRows = TraceInfoCalculator.CalculateEquipment(dict);
            Model.NodesRows = TraceInfoCalculator.CalculateNodes(dict);

            if (dict.ContainsKey(EquipmentType.AdjustmentPoint))
            {
                Model.AdjustmentPointsLine = string.Format(Resources.SID_To_adjust_trace_drawing_were_used__0__point_s_,
                    dict[EquipmentType.AdjustmentPoint]);
                Model.AdjustmentPointsLineVisibility = Visibility.Visible;
            }

            if (_isInCreationMode)
                Model.IsTraceModeDark = true;
            else
                _ = await GetOtherPropertiesOfExistingTrace();
            IsEditEnabled = _currentUser.Role <= Role.Root;
            IsCreatedSuccessfully = false;
        }

        private async Task<bool> GetOtherPropertiesOfExistingTrace()
        {
            var trace = _readModel.Traces.FirstOrDefault(t => t.TraceId == Model.TraceId);
            if (trace == null) return false;

            Title = trace.Title;
            if (trace.Mode == TraceMode.Light)
                Model.IsTraceModeLight = true;
            else
                Model.IsTraceModeDark = true;
            Model.PortNumber = GetPortString(trace);
            Model.Comment = trace.Comment;

            var km = Resources.SID_km;
            var sorData = await GetBase(trace.PreciseId);
            Model.OpticalLength = sorData == null
                ? Resources.SID_no_base
                : $@"{sorData.GetTraceLengthKm():#,0.##} {km}";

            Model.PhysicalLength = _currentGis.IsWithoutMapMode
                ? Resources.SID_Without_map_mode
                : $@"{_graphGpsCalculator.CalculateTraceGpsLengthKm(trace):#,0.##} {km}";


            return true;
        }

        private string GetPortString(Trace trace)
        {
            if (trace.OtauPort == null) return Resources.SID_not_attached;
            if (trace.OtauPort.IsPortOnMainCharon) return trace.OtauPort.OpticalPort.ToString();

            var otau = _readModel.Otaus.FirstOrDefault(o => o.Serial == trace.OtauPort.Serial);
            if (otau == null) return @"error";
            return $@"{otau.MasterPort}-{trace.OtauPort.OpticalPort}";
        }

        private async Task<OtdrDataKnownBlocks> GetBase(Guid baseId)
        {
            if (baseId == Guid.Empty)
                return null;

            var baseRef = _readModel.BaseRefs.FirstOrDefault(b => b.Id == baseId);
            if (baseRef == null)
                return null;

            var sorBytes = await _c2DWcfCommonManager.GetSorBytes(baseRef.SorFileId);
            return SorData.FromBytes(sorBytes);
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Trace;
        }

        public async void Save()
        {
            IsButtonsEnabled = false;
            if (_isInCreationMode)
                await SendAddTraceCommand();
            else
                await SendUpdateTraceCommand();
            IsSavePressed = true;
            IsButtonsEnabled = true;
            TryClose();
        }

        private string IsTitleValid()
        {
            if (string.IsNullOrEmpty(Title))
                return Resources.SID_Title_is_required;
            if (_readModel.Traces.Any(t => t.Title == Title && t.TraceId != Model.TraceId))
                return Resources.SID_There_is_a_trace_with_the_same_title;
            if (Title.IndexOfAny(@"*+:\/[];|=".ToCharArray()) != -1)
                return Resources.SID_Trace_title_contains_forbidden_symbols;

            return string.Empty;
        }

        private async Task SendAddTraceCommand()
        {
            var fiberIds = _readModel.GetFibersAtTraceCreation(Model.TraceNodes).ToList();
            if (fiberIds.Count + 1 != Model.TraceNodes.Count)
            {
                var errVm = new MyMessageBoxViewModel(MessageType.Error, new List<string>()
                {
                    Resources.SID_Nodes_count_does_not_match_sections_count_,
                    "",
                    Resources.SID_Define_trace_again_,
                }, 0);
                _windowManager.ShowDialogWithAssignedOwner(errVm);
                return;
            }
            var cmd = new AddTrace()
            {
                TraceId = Model.TraceId,
                RtuId = Model.Rtu.Id,
                Title = Title,
                NodeIds = Model.TraceNodes,
                EquipmentIds = Model.TraceEquipments,
                FiberIds = fiberIds,
                Comment = Model.Comment
            };

            string message;
            using (_globalScope.Resolve<IWaitCursor>())
            {
                message = await _c2DWcfManager.SendCommandAsObj(cmd);
            }

            if (message == null)
                IsCreatedSuccessfully = true;
            else
                _windowManager.ShowDialogWithAssignedOwner(
                    new MyMessageBoxViewModel(MessageType.Error, @"AddTrace: " + message));
        }

        private async Task SendUpdateTraceCommand()
        {
            var cmd = new UpdateTrace()
            {
                Id = Model.TraceId,
                Title = Title,
                Mode = Model.IsTraceModeLight ? TraceMode.Light : TraceMode.Dark,
                Comment = Model.Comment
            };
            using (_globalScope.Resolve<IWaitCursor>())
            {
                await _c2DWcfManager.SendCommandAsObj(cmd);
            }

        }

        public void DevReport()
        {
            if (_isInCreationMode)
            {
                MessageBox.Show(@"Report could be done for ready trace only!");
                return;
            }

            var trace = _readModel.Traces.FirstOrDefault(t => t.TraceId == Model.TraceId);
            if (trace == null) return;

            var res = _readModel.TraceDevReport(trace, out List<string> content);
            var filename = $@"..\Reports\tdr-{trace.Title}.txt";
            File.WriteAllLines(filename, content);

            var content2 = _readModel.TraceDevReport2(trace);
            var filename2 = $@"..\Reports\tdr2-{trace.Title}.txt";
            File.WriteAllLines(filename2, content2);
            var vm = new MyMessageBoxViewModel(MessageType.Information, new List<string>()
            {
                res ? @"Trace has no errors. Congratulations!" : @"Error(s) found on trace!",
                "",
                $@"Created report {filename}"
            }, 0);
            _windowManager.ShowDialogWithAssignedOwner(vm);
        }

        public void Cancel()
        {
            IsSavePressed = false;
            TryClose();
        }

        public string this[string columnName]
        {
            get
            {
                var errorMessage = string.Empty;
                switch (columnName)
                {
                    case "Title":
                        errorMessage = IsTitleValid();
                        break;
                }

                return errorMessage;
            }
        }

        public string Error { get; set; }
    }
}