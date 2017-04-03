using System.Collections.Generic;
using System.Collections.ObjectModel;
using Caliburn.Micro;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.TestBench
{
    public class ZonesViewModel : Screen
    {
        private readonly UsersDb _usersDb;

        public ObservableCollection<Zone> Rows { get; set; } = new ObservableCollection<Zone>();

        public ZonesViewModel(UsersDb usersDb)
        {
            _usersDb = usersDb;

            foreach (var zone in usersDb.Zones)
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
            _usersDb.Zones = new List<Zone>();
            foreach (var zone in Rows)
            {
                _usersDb.Zones.Add((Zone)zone.Clone());
            }
            _usersDb.Save();
            TryClose(true);
        }

        public void Cancel()
        {
            TryClose(false);
        }
    }
}
