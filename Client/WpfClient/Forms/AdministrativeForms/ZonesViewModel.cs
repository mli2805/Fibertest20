using System.Collections.ObjectModel;
using Caliburn.Micro;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public class ZonesViewModel : Screen
    {
        public ObservableCollection<Zone> Rows { get; set; } = new ObservableCollection<Zone>();

        public void Initialize()
        {
            // TODO get zones from Db
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Responsibility_zones;
        }

        public void Save()
        {
                TryClose();
        }

        public void Cancel()
        {
            TryClose();
        }
    }
}
