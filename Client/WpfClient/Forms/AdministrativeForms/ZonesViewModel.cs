using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.WcfServiceForClientInterface;

namespace Iit.Fibertest.Client
{
    public class ZonesViewModel : Screen
    {
        private readonly IWcfServiceForClient _c2DWcfManager;
        public ObservableCollection<Zone> Rows { get; set; }

        public ZonesViewModel(IWcfServiceForClient c2DWcfManager)
        {
            _c2DWcfManager = c2DWcfManager;
        }

        public async Task<int> Initialize()
        {
            var zones = await _c2DWcfManager.GetZonesAsync();
            var defaultZone = zones.First(z => z.IsDefaultZone);
            defaultZone.Title = Resources.SID_Default_Zone;
            Rows = new ObservableCollection<Zone>();
            foreach (var zone in zones)
            {
                Rows.Add(zone);
            }

            return 1;
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
