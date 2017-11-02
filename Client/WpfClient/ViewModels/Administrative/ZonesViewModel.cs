using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Caliburn.Micro;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public class ZonesViewModel : Screen
    {
        private List<Zone> _zones;
        public ObservableCollection<Zone> Rows { get; set; } = new ObservableCollection<Zone>();

        public ZonesViewModel(List<Zone> zones)
        {
            foreach (var zone in zones)
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
            _zones = new List<Zone>();
            foreach (var zone in Rows)
            {
                if (zone.Id == Guid.Empty)
                    zone.Id = Guid.NewGuid();
                _zones.Add((Zone)zone.Clone());
            }
            TryClose(true);
        }

        public void Cancel()
        {
            TryClose(false);
        }
    }
}
