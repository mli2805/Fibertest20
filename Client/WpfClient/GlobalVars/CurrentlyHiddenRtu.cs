using System;
using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.Graph;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WpfCommonViews;

namespace Iit.Fibertest.Client
{
    public class CurrentlyHiddenRtu : PropertyChangedBase
    {
        private readonly IniFile _iniFile;
        private readonly Model _readModel;
        private bool _isShowAllPressed;
        private bool _isHideAllPressed;
        private Guid _changedRtu;

        public void CleanFlags()
        {
            _isShowAllPressed = false;
            _isHideAllPressed = false;
            _changedRtu = Guid.Empty;
        }

        // True if buttons ShowAll or HideAll pressed, 
        // after Rendering is completed (bulk operation) it will clear this flag
        public bool IsShowAllPressed
        {
            get { return _isShowAllPressed; }
            set
            {
                if (value == _isShowAllPressed) return;
                _isHideAllPressed = false;
                _isShowAllPressed = value;
                NotifyOfPropertyChange();
            }
        }

        public bool IsHideAllPressed
        {
            get { return _isHideAllPressed; }
            set
            {
                if (value == _isHideAllPressed) return;
                _isShowAllPressed = false;
                _isHideAllPressed = value;
                NotifyOfPropertyChange();
            }
        }

        public Guid ChangedRtu
        {
            get { return _changedRtu; }
            set
            {
                if (value.Equals(_changedRtu)) return;
                _changedRtu = value;
                NotifyOfPropertyChange();

            }
        }

        public ObservableRangeCollection<Guid> Collection { get; set; }

        public CurrentlyHiddenRtu(IniFile iniFile, Model readModel)
        {
            _iniFile = iniFile;
            _readModel = readModel;
        }

        public void Initialize()
        {
            var isGraphVisibleOnStart = _iniFile.Read(IniSection.Miscellaneous, IniKey.IsGraphVisibleOnStart, false);
            Collection = isGraphVisibleOnStart 
                ? new ObservableRangeCollection<Guid>() 
                : new ObservableRangeCollection<Guid>(_readModel.Rtus.Select(r => r.Id));
        }
    }
}