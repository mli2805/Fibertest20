using System;
using System.Collections.ObjectModel;
using Caliburn.Micro;

namespace TreeVision {
    public class ShellViewModel : Screen, IShell
    {
        public LeftPanelViewModel LeftPanelViewModel { get; set; } = new LeftPanelViewModel();
        public ObservableCollection<Leaf> RootCollection2 { get; set; }
        private void Initialize()
        {
            RootCollection2 = new ObservableCollection<Leaf>();
            var server = new Leaf() { Id = Guid.NewGuid(), Title = "ServerOnShellViewModel" };
            var rtu = new Leaf() { Id = Guid.NewGuid(), Title = "Rtu" };
            rtu.Children.Add(new Leaf() { Id = Guid.NewGuid(), Title = "Trace" });
            server.Children.Add(rtu);
            RootCollection2.Add(server);
            server.Children.Add(new Leaf() { Id = Guid.NewGuid(), Title = "Rtu2" });
            server.Children.Add(new Leaf() { Id = Guid.NewGuid(), Title = "Rtu3" });
            server.Children.Add(new Leaf() { Id = Guid.NewGuid(), Title = "Rtu4" });
        }
        public ShellViewModel()
        {
            Initialize();
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Fibertest 2.0";
        }
    }
}