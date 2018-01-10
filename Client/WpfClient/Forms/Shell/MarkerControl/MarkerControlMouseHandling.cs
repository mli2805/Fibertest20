using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using GMap.NET.WindowsPresentation;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    /// <summary>
    /// MarkerControl's mouse handlers
    /// </summary>
    public partial class MarkerControl
    {
        private void MarkerControl_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (Type == EquipmentType.Rtu)
                OpenRtuContextMenu();
            else
                OpenNodeContextMenu();
            e.Handled = true;
        }

        // происходит при запуске приложения поэтому используется для инициализации offset
        // offset задает смещение пиктограммы относительно координат
        void MarkerControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            GMapMarker.Offset = new Point(-e.NewSize.Width / 2, -e.NewSize.Height / 2);
        }

        // при нажатом Ctrl левая кнопка таскает данный маркер
        private void MarkerControl_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && IsMouseCaptured && (Keyboard.Modifiers & ModifierKeys.Control) != 0)
            {
                _popup.IsOpen = false;
                DragMarkerWithItsFibers(e);
            }
            e.Handled = true;
        }

        private void DragMarkerWithItsFibers(MouseEventArgs e)
        {
            Point p = e.GetPosition(MainMap);
            GMapMarker.Position = MainMap.FromLocalToLatLng((int)(p.X), (int)(p.Y));

            foreach (var route in MainMap.Markers.Where(m => m is GMapRoute))
            {
                var r = (GMapRoute)route;
                if (r.LeftId != GMapMarker.Id && r.RightId != GMapMarker.Id)
                    continue;
                if (r.LeftId == GMapMarker.Id)
                    r.Points[0] = GMapMarker.Position;
                if (r.RightId == GMapMarker.Id)
                    r.Points[1] = GMapMarker.Position;
                r.RegenerateShape(MainMap);
            }
        }

        void MarkerControl_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!IsMouseCaptured && (Keyboard.Modifiers & ModifierKeys.Control) != 0)
            {
                Mouse.Capture(this);
            }
            e.Handled = true;
        }

        void MarkerControl_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (IsMouseCaptured)
            {
                Mouse.Capture(null);
                Owner.GraphReadModel.Request = new MoveNode() { Id = GMapMarker.Id, Latitude = GMapMarker.Position.Lat, Longitude = GMapMarker.Position.Lng };
            }

            if (MainMap.IsInTraceDefiningMode)
                EndTraceDefinition();
            else if (MainMap.IsInFiberCreationMode)
                EndFiberCreation();
            e.Handled = true;
        }

        private void EndTraceDefinition()
        {
            MainMap.IsInTraceDefiningMode = false;
            Owner.GraphReadModel.Request = new RequestAddTrace() {NodeWithRtuId = MainMap.StartNode.Id, LastNodeId = GMapMarker.Id};
            Owner.SetBanner("");
        }

        private void EndFiberCreation()
        {
            MainMap.IsInFiberCreationMode = false;
            // it was a temporary fiber for creation purposes only
            MainMap.Markers.Remove(MainMap.Markers.Single(m => m.Id == MainMap.FiberUnderCreation));

            if (!MainMap.IsFiberWithNodes)
                Owner.GraphReadModel.Request = new AddFiber() { Node1 = MainMap.StartNode.Id, Node2 = GMapMarker.Id };
            else
                Owner.GraphReadModel.Request = new RequestAddFiberWithNodes() { Node1 = MainMap.StartNode.Id, Node2 = GMapMarker.Id, };

            MainMap.FiberUnderCreation = Guid.Empty;
            Cursor = Cursors.Arrow;
        }


        private Cursor _cursorBeforeEnter;
        void MarkerControl_MouseLeave(object sender, MouseEventArgs e)
        {
            GMapMarker.ZIndex -= 10000;
//            Cursor = Cursors.Arrow;
            Cursor = _cursorBeforeEnter;
            if (!string.IsNullOrEmpty(Title))
                _popup.IsOpen = false;
        }

        void MarkerControl_MouseEnter(object sender, MouseEventArgs e)
        {
            GMapMarker.ZIndex += 10000;
            _cursorBeforeEnter = Cursor;
            Cursor = Cursors.Hand;

            if (!string.IsNullOrEmpty(Title))
                _popup.IsOpen = true;
        }
    }
}
