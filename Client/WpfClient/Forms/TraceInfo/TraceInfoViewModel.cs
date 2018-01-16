using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
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
        private readonly TraceInfoCalculator _traceInfoCalculator;

        public TraceInfoModel Model { get; set; } = new TraceInfoModel();
        
        public TraceInfoViewModel(ReadModel readModel, IWcfServiceForClient c2DWcfManager,
            IWindowManager windowManager, TraceInfoCalculator traceInfoCalculator)
        {
            _readModel = readModel;
            _c2DWcfManager = c2DWcfManager;
            _windowManager = windowManager;
            _traceInfoCalculator = traceInfoCalculator;
        }


        /// Setup traceId (for existing trace) or traceEquipments for trace creation moment
        public void Initialize(Guid traceId, List<Guid> traceEquipments, List<Guid> traceNodes)
        {
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
                // Model.AdjustmentPointsLine = $"To adjust trace drawing were used {dict[EquipmentType.AdjustmentPoint]} point(s)";
                Model.AdjustmentPointsLine = $@"Для украшения трассы было использовано точек привязки: {dict[EquipmentType.AdjustmentPoint]} шт.";
                Model.AdjustmentPointsLineVisibility = Visibility.Visible;
            }

            if (traceId == Guid.Empty)
                Model.IsTraceModeDark = true;
            else
               GetOtherPropertiesOfExistingTrace();
        }

        private void GetOtherPropertiesOfExistingTrace()
        {
            var trace = _readModel.Traces.First(t => t.Id == Model.TraceId);

            Model.Title = trace.Title;
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
            if (Model.TraceId == Guid.Empty)
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
                RtuId = Model.Rtu.Id,
                Title = Model.Title,
                Nodes = Model.TraceNodes,
                Equipments = Model.TraceEquipments,
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
                Title = Model.Title,
                Mode = Model.IsTraceModeLight ? TraceMode.Light : TraceMode.Dark,
                Comment = Model.Comment
            };
            await _c2DWcfManager.SendCommandAsObj(cmd);
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
                    case "Model.Title": if (string.IsNullOrEmpty(Model.Title))
                                        errorMessage = Resources.SID_Title_is_required;
                                    break;
                }
                return errorMessage;
            }
        }

        public string Error { get; } = null;
    }
}
