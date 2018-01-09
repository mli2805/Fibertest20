using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.WcfServiceForClientInterface;

namespace Iit.Fibertest.Client
{
    public class TraceInfoViewModel : Screen, IDataErrorInfo
    {
        private readonly ReadModel _readModel;
        private readonly IWcfServiceForClient _c2DWcfManager;
        private readonly IWindowManager _windowManager;

        private Rtu _rtu;
        public Guid TraceId { get; set; }
        private List<Guid> _traceEquipments;
        private readonly List<Guid> _traceNodes;
        public string RtuTitle { get; set; }
        public string PortNumber { get; set; }

        public List<NodesStatisticsItem> NodesStatistics { get; } = new List<NodesStatisticsItem>();

        private string _title;
        public string Title
        {
            get { return _title; }
            set
            {
                if (value == _title) return;
                _title = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(IsButtonSaveEnabled));
            }
        }

        private bool _isTraceModeLight;
        public bool IsTraceModeLight
        {
            get { return _isTraceModeLight; }
            set
            {
                if (value == _isTraceModeLight) return;
                _isTraceModeLight = value;
                NotifyOfPropertyChange();
            }
        }

        private bool _isTraceModeDark;
        public bool IsTraceModeDark
        {
            get { return _isTraceModeDark; }
            set
            {
                if (value == _isTraceModeDark) return;
                _isTraceModeDark = value;
                NotifyOfPropertyChange();
            }
        }

        private string _comment;
        public string Comment
        {
            get { return _comment; }
            set
            {
                if (value == _comment) return;
                _comment = value;
                NotifyOfPropertyChange();
            }
        }

        public bool IsInTraceCreationMode { get; set; }
        public bool IsButtonSaveEnabled => !string.IsNullOrEmpty(_title);

        /// <summary>
        /// Setup traceId (for existing trace) or traceEquipments for trace creation moment
        /// </summary>
        /// <param name="readModel"></param>
        /// <param name="c2DWcfManager"></param>
        /// <param name="windowManager"></param>
        /// <param name="traceId"></param>
        /// <param name="traceEquipments"></param>
        /// <param name="traceNodes"></param>
        public TraceInfoViewModel(ReadModel readModel, IWcfServiceForClient c2DWcfManager, IWindowManager windowManager, 
            Guid traceId, List<Guid> traceEquipments = null, List<Guid> traceNodes = null)
        {
            _readModel = readModel;
            _c2DWcfManager = c2DWcfManager;
            _windowManager = windowManager;
            TraceId = traceId;
            _traceEquipments = traceEquipments;
            _traceNodes = traceNodes;

            IsInTraceCreationMode = traceId == Guid.Empty;
            Initialize();
        }

        private void Initialize()
        {
            if (IsInTraceCreationMode)
            {
                IsTraceModeDark = true;
            }
            else
            {          // trace editing
                var trace = _readModel.Traces.First(t => t.Id == TraceId);
                Title = trace.Title;

                if (trace.Mode == TraceMode.Light)
                    IsTraceModeLight = true;
                else
                    IsTraceModeDark = true;
                PortNumber = trace.Port > 0 ? trace.Port.ToString() : Resources.SID_not_attached;
                _traceEquipments = trace.Equipments;
                Comment = trace.Comment;
            }
            _rtu = _readModel.Rtus.First(r => r.Id == _traceEquipments[0]);
            RtuTitle = _rtu.Title;
            InitializeNodesStatistics(_traceEquipments);

        }
        private void InitializeNodesStatistics(List<Guid> traceEquipments)
        {
            NodesStatistics.Add(new NodesStatisticsItem(Resources.SID_In_total__including_RTU, traceEquipments.Count));

            var dict = new Dictionary<EquipmentType, int>();
            foreach (var id in traceEquipments.Skip(1).Where(e => e != Guid.Empty))
            {
                var type = _readModel.Equipments.First(e => e.Id == id).Type;
                if (dict.ContainsKey(type))
                    dict[type]++;
                else dict.Add(type, 1);
            }

            NodesStatistics.AddRange(dict.Select(item => new NodesStatisticsItem(item.Key.ToLocalizedString(), item.Value)));
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Trace;
        }

        public async void Save()
        {
            if (IsInTraceCreationMode)
                await SendAddTraceCommand();
            else
                await SendUpdateTraceCommand();
            TryClose();
        }

        private async Task SendAddTraceCommand()
        {
            var cmd = new AddTrace()
            {
                Id = Guid.NewGuid(),
                RtuId = _rtu.Id,
                Title = Title,
                Nodes = _traceNodes,
                Equipments = _traceEquipments,
                Comment = Comment
            };
            var message = 
//                await _bus.SendCommand(cmd);
                    await _c2DWcfManager.SendCommandAsObj(cmd);

            if (message != null)
                _windowManager.ShowDialogWithAssignedOwner(new MyMessageBoxViewModel(MessageType.Error, message));
        }

        private async Task SendUpdateTraceCommand()
        {
            var cmd = new UpdateTrace()
            {
                Id = TraceId,
                Title = Title,
                Mode = IsTraceModeLight ? TraceMode.Light : TraceMode.Dark,
                Comment = Comment
            };
                    await _c2DWcfManager.SendCommandAsObj(cmd);
//            await _bus.SendCommand(cmd);
        }

        public void Cancel()
        {
            TryClose();
        }

        public string this[string columnName]
        {
            get
            {
                String errorMessage = String.Empty;
                switch (columnName)
                {
                    case "Title":
                        if (String.IsNullOrEmpty(Title))
                        {
                            errorMessage = Resources.SID_Title_is_required;
                        }
                        break;
                }
                return errorMessage;
            }
        }

        public string Error { get; } = null;
    }
}
