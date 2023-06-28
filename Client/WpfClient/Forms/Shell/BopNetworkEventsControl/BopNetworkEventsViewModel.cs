using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using Caliburn.Micro;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public class BopNetworkEventsViewModel : PropertyChangedBase
    {
        private readonly Model _readModel;
        public string TableTitle { get; set; }

        public ObservableCollection<BopNetworkEventModel> Rows { get; set; } = new ObservableCollection<BopNetworkEventModel>();

        public BopNetworkEventsViewModel(Model readModel)
        {
            _readModel = readModel;

            var view = CollectionViewSource.GetDefaultView(Rows);
            view.SortDescriptions.Add(new SortDescription(@"Nomer", ListSortDirection.Descending));
        }

        public void AddEvent(BopNetworkEvent evnt)
        {
            var rtu = _readModel.Rtus.FirstOrDefault(r => r.Id == evnt.RtuId);
            if (rtu == null) return;
            Rows.Add(new BopNetworkEventModel()
            {
                Nomer = evnt.Ordinal,
                EventTimestamp = evnt.EventTimestamp,
                OtauIp = evnt.OtauIp,
                BopName = rtu.OtdrNetAddress.Ip4Address == evnt.OtauIp ? Resources.SID_Main : evnt.OtauIp,
                TcpPort = evnt.TcpPort,
                RtuId = evnt.RtuId,
                RtuTitle = rtu.Title,
                IsOk = evnt.IsOk,
            });
        }

        public void RemoveOldEventForBopIfExists(string otauIp)
        {
            var oldEvent = Rows.FirstOrDefault(r => r.OtauIp == otauIp);
            if (oldEvent != null)
                Rows.Remove(oldEvent);
        }

        public void RemoveEvents(OtauDetached evnt)
        {
            foreach (var bopNetworkEventModel in Rows.Where(r => r.OtauIp == evnt.OtauIp && r.TcpPort == evnt.TcpPort).ToList())
            {
                Rows.Remove(bopNetworkEventModel);
            }
        }

        public void RemoveEventsAndSors(EventsAndSorsRemoved evnt)
        {
            if (!evnt.IsNetworkEvents) return;

            foreach (var bopNetworkEventModel in Rows.ToList())
            {
                if (_readModel.BopNetworkEvents.All(n => n.EventTimestamp != bopNetworkEventModel.EventTimestamp))
                    Rows.Remove(bopNetworkEventModel);
            }
        }
    }
}
