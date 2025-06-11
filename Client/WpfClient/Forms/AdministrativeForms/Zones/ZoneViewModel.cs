using System;
using System.ComponentModel;
using System.Linq;
using AutoMapper;
using Caliburn.Micro;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.WcfConnections;

namespace Iit.Fibertest.Client
{
    public class ZoneViewModel : Screen, IDataErrorInfo
    {
        private static readonly IMapper Mapper = new MapperConfiguration(
            cfg => cfg.AddProfile<MappingViewModelToCommand>()).CreateMapper();

        private readonly IWcfServiceDesktopC2D _c2DWcfManager;
        private readonly Model _readModel;
        private bool _isInCreationMode;
        private Zone _originalZone;

        public Guid ZoneId { get; set; }

        private string _title;
        public string Title
        {
            get => _title;
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
            get => _comment;
            set
            {
                if (value == _comment) return;
                _comment = value;
                NotifyOfPropertyChange();
            }
        }

        private bool _isButtonSaveEnabled;
        public bool IsButtonSaveEnabled
        {
            get => _isButtonSaveEnabled;
            set
            {
                if (value == _isButtonSaveEnabled) return;
                _isButtonSaveEnabled = value;
                NotifyOfPropertyChange();
            }
        }

        public ZoneViewModel(IWcfServiceDesktopC2D c2DWcfManager, Model readModel)
        {
            _c2DWcfManager = c2DWcfManager;
            _readModel = readModel;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = _isInCreationMode ? Resources.SID_New_responsibility_zone_creation : Resources.SID_Update_responsibility_zone;
        }

        public void Initialize(Zone selectedZone)
        {
            _isInCreationMode = false;
            _originalZone = selectedZone;
            ZoneId = selectedZone.ZoneId;
            Title = selectedZone.Title;
            Comment = selectedZone.Comment;
        }

        public void Initialize()
        {
            _isInCreationMode = true;
            ZoneId = Guid.NewGuid();
            _originalZone = new Zone() { ZoneId = ZoneId };
        }

        public async void Save()
        {
            object cmd;
            if (_isInCreationMode)
                cmd = Mapper.Map<AddZone>(this);
            else
                cmd = Mapper.Map<UpdateZone>(this);
            await _c2DWcfManager.SendCommandAsObj(cmd);
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
                        if (string.IsNullOrEmpty(_title?.Trim()))
                            errorMessage = Resources.SID_Title_is_required;
                        if (_readModel.Zones.Any(z => z.Title == Title && z.ZoneId != _originalZone.ZoneId))
                            errorMessage = @"There is a zone with the same title";
                        IsButtonSaveEnabled = errorMessage == string.Empty;
                        break;
                }
                return errorMessage;
            }
        }

        public string Error { get; set; }
    }
}
