using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.WcfServiceForClientInterface;
using Iit.Fibertest.WpfCommonViews;


namespace Iit.Fibertest.Client
{
    public class TraceInfoViewModel : Screen, IDataErrorInfo
    {
        private readonly Model _readModel;
        private readonly CurrentUser _currentUser;
        private readonly IWcfServiceForClient _c2DWcfManager;
        private readonly IWindowManager _windowManager;
        private readonly TraceInfoCalculator _traceInfoCalculator;
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

        public bool IsButtonSaveEnabled => IsEditEnabled && IsTitleValid() == string.Empty;

        public TraceInfoViewModel(Model readModel, CurrentUser currentUser, IWcfServiceForClient c2DWcfManager,
            IWindowManager windowManager, TraceInfoCalculator traceInfoCalculator)
        {
            _readModel = readModel;
            _currentUser = currentUser;
            _c2DWcfManager = c2DWcfManager;
            _windowManager = windowManager;
            _traceInfoCalculator = traceInfoCalculator;
        }


        /// Setup traceId (for existing trace) or traceEquipments for trace creation moment
        public void Initialize(Guid traceId, List<Guid> traceEquipments, List<Guid> traceNodes, bool isInCreationMode)
        {
            _isInCreationMode = isInCreationMode;
            Model.TraceId = traceId;
            Model.TraceEquipments = traceEquipments;
            Model.TraceNodes = traceNodes;
            Model.Rtu = _readModel.Rtus.First(r => r.Id == Model.TraceEquipments[0]);
            Model.RtuTitle = Model.Rtu.Title;
            Model.PortNumber = Resources.SID_not_attached;
            var dict = _traceInfoCalculator.BuildDictionaryByEquipmentType(Model.TraceEquipments);
            Model.EquipmentsRows = _traceInfoCalculator.CalculateEquipment(dict);
            Model.NodesRows = _traceInfoCalculator.CalculateNodes(dict);

            if (dict.ContainsKey(EquipmentType.AdjustmentPoint))
            {
                Model.AdjustmentPointsLine = string.Format(Resources.SID_To_adjust_trace_drawing_were_used__0__point_s_,
                    dict[EquipmentType.AdjustmentPoint]);
                Model.AdjustmentPointsLineVisibility = Visibility.Visible;
            }

            if (_isInCreationMode)
                Model.IsTraceModeDark = true;
            else
                GetOtherPropertiesOfExistingTrace();
            IsEditEnabled = _currentUser.Role <= Role.Root;
        }

        private void GetOtherPropertiesOfExistingTrace()
        {
            var trace = _readModel.Traces.First(t => t.TraceId == Model.TraceId);

            Title = trace.Title;
            if (trace.Mode == TraceMode.Light)
                Model.IsTraceModeLight = true;
            else
                Model.IsTraceModeDark = true;
            Model.PortNumber = trace.Port > 0 ? trace.Port.ToString() : Resources.SID_not_attached;
            Model.Comment = trace.Comment;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Trace;
        }

        public async void Save()
        {
           
            if (_isInCreationMode)
                await SendAddTraceCommand();
            else
                await SendUpdateTraceCommand();
            IsSavePressed = true;
            TryClose();
        }

        private string IsTitleValid()
        {
            if (string.IsNullOrEmpty(Title))
                return Resources.SID_Title_is_required;
            if (_readModel.Traces.Any(t => t.Title == Title && t.TraceId != Model.TraceId))
                return Resources.SID_There_is_a_trace_with_the_same_title;
            if (Title.IndexOfAny(@"*+:\/[];|=".ToCharArray()) != -1)
                 return   Resources.SID_Trace_title_contains_forbidden_symbols;

            return string.Empty;
        }

        private async Task SendAddTraceCommand()
        {
            var fiberIds = _readModel.GetFibersOnTraceCreation(Model.TraceNodes).ToList();
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
            var message = await _c2DWcfManager.SendCommandAsObj(cmd);

            if (message != null)
                _windowManager.ShowDialogWithAssignedOwner(new MyMessageBoxViewModel(MessageType.Error, message));
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
            await _c2DWcfManager.SendCommandAsObj(cmd);
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