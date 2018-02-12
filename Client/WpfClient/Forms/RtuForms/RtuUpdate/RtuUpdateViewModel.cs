using System;
using System.ComponentModel;
using System.Linq;
using AutoMapper;
using Caliburn.Micro;
using GMap.NET;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.WcfServiceForClientInterface;

namespace Iit.Fibertest.Client
{
    public class RtuUpdateViewModel : Screen, IDataErrorInfo
    {
        public Guid RtuId;
        private Rtu _originalRtu;
        private readonly ReadModel _readModel;
        private readonly IWcfServiceForClient _c2DWcfManager;

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

        public RtuUpdateViewModel(ReadModel readModel, IWcfServiceForClient c2DWcfManager)
        {
            _readModel = readModel;
            _c2DWcfManager = c2DWcfManager;
        }

        public void Initilize(Guid rtuId)
        {
            RtuId = rtuId;
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
                UpdateRtu cmd = mapper.Map<UpdateRtu>(this);
                cmd.Id = _originalRtu.Id;
                _c2DWcfManager.SendCommandAsObj(cmd);
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
