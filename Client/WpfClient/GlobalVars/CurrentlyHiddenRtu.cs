using System;
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


        // True if buttons ShowAll or HideAll pressed, then Rendering will clear this flag
        public bool IsShowAllPressed { get; set; }
        public bool IsHideAllPressed { get; set; }
        public ObservableRangeCollection<Guid> Collection { get; set; }

        public CurrentlyHiddenRtu(IniFile iniFile, Model readModel)
        {
            _iniFile = iniFile;
            _readModel = readModel;
        }

        public void Initialize()
        {
            var isGraphVisibleOnStart = _iniFile.Read(IniSection.Miscellaneous, IniKey.IsGraphVisibleOnStart, true);
            Collection = isGraphVisibleOnStart 
                ? new ObservableRangeCollection<Guid>() 
                : new ObservableRangeCollection<Guid>(_readModel.Rtus.Select(r => r.Id));
        }
    }
}