using System.Collections.Generic;
using System.Collections.ObjectModel;
using Caliburn.Micro;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.TestBench
{
    public class ZonesViewModel : Screen
    {
        private readonly AdministrativeDb _administrativeDb;

        public ObservableCollection<Zone> Rows { get; set; } = new ObservableCollection<Zone>();

        public ZonesViewModel(AdministrativeDb administrativeDb)
        {
            _administrativeDb = administrativeDb;

            foreach (var zone in administrativeDb.Zones)
            {
                Rows.Add((Zone)zone.Clone());
            }
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Responsibility_zones;
        }

        public void Save()
        {
            _administrativeDb.Zones = new List<Zone>();
            foreach (var zone in Rows)
            {
                _administrativeDb.Zones.Add((Zone)zone.Clone());
            }
            _administrativeDb.Save();
            TryClose(true);
        }

        public void Cancel()
        {
            TryClose(false);
        }
    }
}
