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
//        public List<Guid> List { get; set; }

        public ObservableCollection<Guid> Collection { get; set; }

        public CurrentlyHiddenRtu(IniFile iniFile, Model reaModel)
        {
            var isGraphVisibleOnStart = iniFile.Read(IniSection.Miscellaneous, IniKey.IsGraphVisibleOnStart, true);
            Collection = isGraphVisibleOnStart ? new ObservableCollection<Guid>() : new ObservableCollection<Guid>(reaModel.Rtus.Select(r => r.Id));
        }
    }
}