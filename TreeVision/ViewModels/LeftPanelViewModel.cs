using System;
using System.Collections.ObjectModel;

namespace TreeVision
{
    public class LeftPanelViewModel
    {
        public ObservableCollection<Leaf> RootCollection { get; set; }

        public LeftPanelViewModel()
        {
            Initialize();
        }

        private void Initialize()
        {
            RootCollection = new ObservableCollection<Leaf>
            {
                new Leaf() {Id = Guid.NewGuid(), Title = "fase45g543aq25 sd4e5qws"},
                new Leaf() {Id = Guid.NewGuid(), Title = "as980q92 q098wf df"},
                new Leaf() {Id = Guid.NewGuid(), Title = "1241234c1w ds"},
                new Leaf() {Id = Guid.NewGuid(), Title = "גûאש839 3ףרגנ3רד4נ ס"}
            };
        }
    }
}
