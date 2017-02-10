using System;
using System.Collections.ObjectModel;
using System.Windows.Media;
using System.Windows.Media.Imaging;

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
            RootCollection = new ObservableCollection<Leaf>();
            var server = new Leaf() {Id = Guid.NewGuid(), Title = "ServerOnLeftPanel", Color = Brushes.Black };
            // or better inherit from an abstract class LEAF 
            var rtu = new Leaf() { Id = Guid.NewGuid(), Title = "Rtu", Color = Brushes.Black, LeafType = LeafType.Rtu};
            
            var path = "pack://application:,,,/Resources/Rtu/blue_sphere_16.jpg";
            rtu.Pic1 = new BitmapImage(new Uri(path));

            rtu.Children.Add(new Leaf() { Id = Guid.NewGuid(), Title = "Trace", Color = Brushes.Blue });
            server.Children.Add(rtu);
            RootCollection.Add(server);
            server.Children.Add(new Leaf() {Id = Guid.NewGuid(), Title = "Rtu2", Color = Brushes.Red });
            server.Children.Add(new Leaf() {Id = Guid.NewGuid(), Title = "Rtu3", Color = Brushes.Green});
            server.Children.Add(new Leaf() {Id = Guid.NewGuid(), Title = "Rtu4", Color = Brushes.Blue });
        }
    }
}
