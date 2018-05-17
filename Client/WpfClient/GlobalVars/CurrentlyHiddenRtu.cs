using System;
using System.Collections.ObjectModel;
using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.Graph;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.Client
{
    public class CurrentlyHiddenRtu : PropertyChangedBase
    {
        private readonly IniFile _iniFile;
        private readonly Model _readModel;
        private ObservableCollection<Guid> _collection;

        public ObservableCollection<Guid> Collection
        {
            get => _collection;
            set
            {
                if (Equals(value, _collection)) return;
                _collection = value;
                NotifyOfPropertyChange();
            }
        }

        public CurrentlyHiddenRtu(IniFile iniFile, Model readModel)
        {
            _iniFile = iniFile;
            _readModel = readModel;
        }

        public void Initialize()
        {
            var isGraphVisibleOnStart = _iniFile.Read(IniSection.Miscellaneous, IniKey.IsGraphVisibleOnStart, true);
            Collection = isGraphVisibleOnStart ? new ObservableCollection<Guid>() : new ObservableCollection<Guid>(_readModel.Rtus.Select(r => r.Id));
        }
    }
}