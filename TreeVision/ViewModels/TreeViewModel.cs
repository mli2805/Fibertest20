using System;
using System.Collections.ObjectModel;
using TreeVision.Vms;

namespace TreeVision
{
    public class TreeViewModel
    {
        public ObservableCollection<RtuVm> Rtus { get; set; }

        public TreeViewModel()
        {
            Initialize();
        }

        private void Initialize()
        {
            Rtus = new ObservableCollection<RtuVm>();
            Rtus.Add(new RtuVm() {Id = Guid.NewGuid(), Title = "fase45g543aq25 sd4e5qws"});
            Rtus.Add(new RtuVm() {Id = Guid.NewGuid(), Title = "as980q92 q098wf df"});
            Rtus.Add(new RtuVm() {Id = Guid.NewGuid(), Title = "1241234c1w ds"});
            Rtus.Add(new RtuVm() {Id = Guid.NewGuid(), Title = "выащ839 3ушвр3шг4р с"});
        }
    }
}
