using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public class TceTypeSelectionViewModel : PropertyChangedBase
    {
        public List<TceTypeStruct> TceTypes { get; set; }
        public TceTypeStruct SelectedType { get; set; }

        public void Initialize(List<TceTypeStruct> tceTypes)
        {
            TceTypes = tceTypes;
            SelectedType = TceTypes.First();
        }
    }
}
