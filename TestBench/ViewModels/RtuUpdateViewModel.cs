using System;
using System.ComponentModel;
using System.Linq;
using AutoMapper;
using Caliburn.Micro;
using GMap.NET;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.TestBench
{
    public class RtuUpdateViewModel : Screen, IDataErrorInfo
    {
        public readonly Guid RtuId;
        private Rtu _originalRtu;
        private readonly ReadModel _readModel;

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

        private bool IsChanged()
        {
            return _title != _originalRtu.Title
                   || _comment != _originalRtu.Comment;
        }
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

        public RtuUpdateViewModel(Guid rtuId, ReadModel readModel)
        {
            RtuId = rtuId;
            _readModel = readModel;

            Initilize();
        }

        private void Initilize()
        {
            _originalRtu = _readModel.Rtus.First(r => r.Id == RtuId);

            var currentMode = GpsInputMode.DegreesAndMinutes; // somewhere in configuration file...
            var node = _readModel.Nodes.First(n => n.Id == _originalRtu.NodeId);
            GpsInputViewModel = new GpsInputViewModel(currentMode, new PointLatLng(node.Latitude, node.Longitude));

            Title = _originalRtu.Title;
            Comment = _originalRtu.Comment;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_RTU_Information;
        }

        public void Save()
        {
            if (IsChanged())
            {
                IMapper mapper =
                    new MapperConfiguration(cfg => cfg.AddProfile<MappingViewModelToCommand>()).CreateMapper();
                Command = mapper.Map<UpdateRtu>(this);
                Command.Id = _originalRtu.Id;
            }
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
                        if (_readModel.Rtus.Any(n => n.Title == Title && n.Id != _originalRtu.Id))
                            errorMessage = Resources.SID_There_is_a_rtu_with_the_same_title;
                        IsButtonSaveEnabled = errorMessage == string.Empty;
                        break;
                }
                return errorMessage;
            }
        }

        public string Error { get; set; }
    }


}
