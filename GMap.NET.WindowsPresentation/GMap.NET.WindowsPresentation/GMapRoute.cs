using System;
using System.Windows.Controls;
using System.Windows.Media;

namespace GMap.NET.WindowsPresentation
{
    using System.Collections.Generic;
    using System.Windows.Shapes;

    public interface IShapable
    {
        void RegenerateShape(GMapControl map);
    }

    public class GMapRoute : GMapMarker, IShapable
    {
        public readonly List<PointLatLng> Points = new List<PointLatLng>();
        public Guid LeftId { get; set; }
        public Guid RightId { get; set; }

        private ContextMenu _contextMenu;
        public ContextMenu ContextMenu
        {
            get { return _contextMenu; }
            set
            {
                _contextMenu = value;
                OnPropertyChanged("ContextMenu");
            }
        }

        public GMapRoute(Guid id, Guid leftId, Guid rightId, Brush color, double thickness, IEnumerable<PointLatLng> points)
        {
            Id = id;
            LeftId = leftId;
            RightId = rightId;
            Color = color;
            StrokeThickness = thickness;
            Points.AddRange(points);
            RegenerateShape(null);

            ContextMenu = new ContextMenu();
            ContextMenu.DataContext = this;
            ContextMenu.Items.Add(new MenuItem() { Header = "bluh" });

        }

        public override void Clear()
        {
            base.Clear();
            Points.Clear();
        }

        /// <summary>
        /// regenerates shape of route
        /// </summary>
        public virtual void RegenerateShape(GMapControl map)
        {
            if (map == null) return;

            Map = map;
            if (Points.Count > 1)
            {
                Position = Points[0];

                var localPath = new List<System.Windows.Point>(Points.Count);
                var offset = Map.FromLatLngToLocal(Points[0]);
                foreach (var i in Points)
                {
                    var p = Map.FromLatLngToLocal(i);
                    localPath.Add(new System.Windows.Point(p.X - offset.X, p.Y - offset.Y));
                }

                var shape = map.CreateRoutePath(localPath, Color, StrokeThickness);

                if (Shape is Path)
                    (Shape as Path).Data = shape.Data;
                else
                    Shape = shape;
            }
            else
                Shape = null;
        }

    }
}
