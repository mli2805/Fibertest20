using System;
using System.ComponentModel;
using System.Linq;
using AutoMapper;
using Caliburn.Micro;
using Iit.Fibertest.Graph.Commands;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.TestBench
{
    public class RtuUpdateViewModel : Screen, IDataErrorInfo
    {
        private readonly Guid _nodeId;
        private readonly GraphReadModel _graphReadModel;

        private string _title;
        public string Title
        {
            get { return _title; }
            set
            {
                if (value == _title) return;
                _title = value;
                NotifyOfPropertyChange();
            }
        }

        public string Comment { get; set; }

        private bool _isButtonSaveEnabled;

        public bool IsButtonSaveEnabled
        {
            get { return _isButtonSaveEnabled; }
            set
            {
                if (value == _isButtonSaveEnabled) return;
                _isButtonSaveEnabled = value;
                NotifyOfPropertyChange();
            }
        }
        public GpsInputViewModel GpsInputViewModel { get; set; }

        public UpdateRtu Command { get; set; }

        public RtuUpdateViewModel(Guid nodeId, GraphReadModel graphReadModel)
        {
            _nodeId = nodeId;
            _graphReadModel = graphReadModel;

            Initilize();
        }

        public NodeVm NodeVm { get; set; }

        private void Initilize()
        {
            var rtu = _graphReadModel.Rtus.First(r => r.Node.Id == _nodeId);

            var currentMode = GpsInputMode.DegreesAndMinutes; // somewhere in configuration file...
            NodeVm = rtu.Node;
            GpsInputViewModel = new GpsInputViewModel(currentMode, NodeVm.Position);

            Title = rtu.Title;
            Comment = rtu.Comment;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_RTU_Information;
        }

        public void Save()
        {
            IMapper mapper = new MapperConfiguration(cfg => cfg.AddProfile<MappingViewModelToCommand>()).CreateMapper();
            Command = mapper.Map<UpdateRtu>(this);
            Command.Id = _graphReadModel.Rtus.First(r => r.Node.Id == _nodeId).Id;
            TryClose();
        }

        public void Cancel()
        {
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
                        if (string.IsNullOrEmpty(Title))
                            errorMessage = Resources.SID_Title_is_required;
                        if (_graphReadModel.Rtus.Any(n => n.Title == Title))
                            errorMessage = Resources.SID_There_is_a_rtu_with_the_same_title;
                        break;
                }
                return errorMessage;
            }
        }

        public string Error { get; set; }
    }


}
