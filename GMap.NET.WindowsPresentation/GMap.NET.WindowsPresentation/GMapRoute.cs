using System;
using System.IO;
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

      private int _askContextMenu;
      public int AskContextMenu
      {
         get { return _askContextMenu; }
         set
         {
            _askContextMenu = value;
            OnPropertyChanged("AskContextMenu");
         }
      }

      public ContextMenu ContextMenu { get; set; }

      public GMapRoute(Guid id, Guid leftId, Guid rightId, Brush color, double thickness, IEnumerable<PointLatLng> points, GMapControl map)
      {
         Id = id;
         LeftId = leftId;
         RightId = rightId;
         Color = color;
         StrokeThickness = thickness;
         Points.AddRange(points);
         RegenerateShape(map);
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
               var ppp = new System.Windows.Point(p.X - offset.X, p.Y - offset.Y);
              
               localPath.Add(ppp);
            }

            // если координата более 250 000 , то линия просто не перерисовывается
            // т.к. такая точка уже давно далеко за пределами экрана , то без перерисовки линия смотрит куда надо
//            if ((Math.Abs(localPath[0].X) > 248000) || (Math.Abs(localPath[1].X) > 248000) || (Math.Abs(localPath[2].X) > 248000)) 
//            {
//               File.AppendAllText(@"c:\temp\gmaproute.txt", 
//                  $"denied  {localPath[0].X} ; {localPath[0].Y} ; {localPath[1].X} ; {localPath[1].Y} ;" + Environment.NewLine);
//               return;
//            }

            for (int i = 0; i < Points.Count - 1; i++)
            {
               File.AppendAllText(@"c:\temp\gmaproute.txt", 
                  $"{DateTime.Now} reshape ({i}) {localPath[i].X} ; {localPath[i].Y} ; {localPath[i+1].X} ; {localPath[i+1].Y} ;" + Environment.NewLine);

            }

          for (int i = 0; i < Points.Count; i++)
            {
                if ((Math.Abs(localPath[i].X) > 248000))
                {
                    File.AppendAllText(@"c:\temp\gmaproute.txt", "denied");
                    return;
                }
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
