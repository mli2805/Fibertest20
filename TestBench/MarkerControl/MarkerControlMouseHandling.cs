using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using GMap.NET.WindowsPresentation;
using Iit.Fibertest.Graph;
using Iit.Fibertest.Graph.Commands;

namespace Iit.Fibertest.TestBench
{
    /// <summary>
    /// MarkerControl's mouse handlers
    /// </summary>
    public partial class MarkerControl
    {
        private void MarkerControl_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_type == EquipmentType.Rtu)
                OpenRtuContextMenu();
            else
                OpenNodeContextMenu();
            e.Handled = true;
        }

        // происходит при запуске приложения поэтому используется для инициализации offset
        // offset задает смещение пиктограммы относительно координат
        void MarkerControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            _marker.Offset = new Point(-e.NewSize.Width / 2, -e.NewSize.Height / 2);
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
            Point p = e.GetPosition(_mainMap);
            _marker.Position = _mainMap.FromLocalToLatLng((int)(p.X), (int)(p.Y));

            foreach (var route in _mainMap.Markers.Where(m => m is GMapRoute))
            {
                var r = (GMapRoute)route;
                if (r.LeftId != _marker.Id && r.RightId != _marker.Id)
                    continue;
                if (r.LeftId == _marker.Id)
                    r.Points[0] = _marker.Position;
                if (r.RightId == _marker.Id)
                    r.Points[1] = _marker.Position;
                r.RegenerateShape(_mainMap);
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
                _owner.GraphVm.Command = new MoveNode() { Id = _marker.Id, Latitude = _marker.Position.Lat, Longitude = _marker.Position.Lng };
            }

            if (_mainMap.IsInTraceDefiningMode)
                EndTraceDefinition();
            else if (_mainMap.IsInFiberCreationMode)
                EndFiberCreation();
            e.Handled = true;
        }

        private void EndTraceDefinition()
        {
            _mainMap.IsInTraceDefiningMode = false;
            _owner.GraphVm.Command = new AskAddTrace() {NodeWithRtuId = _mainMap.StartNode.Id, LastNodeId = _marker.Id};
        }

        private void EndFiberCreation()
        {
            _mainMap.IsInFiberCreationMode = false;
            // it was a temporary fiber for creation purposes only
            _mainMap.Markers.Remove(_mainMap.Markers.Single(m => m.Id == _mainMap.FiberUnderCreation));

            if (!_mainMap.IsFiberWithNodes)
                _owner.GraphVm.Command = new AddFiber() { Node1 = _mainMap.StartNode.Id, Node2 = _marker.Id };
            else
                _owner.GraphVm.Command = new AskAddFiberWithNodes() { Node1 = _mainMap.StartNode.Id, Node2 = _marker.Id, };

            _mainMap.FiberUnderCreation = Guid.Empty;
            Cursor = Cursors.Arrow;
        }


        private Cursor _cursorBeforeEnter;
        void MarkerControl_MouseLeave(object sender, MouseEventArgs e)
        {
            _marker.ZIndex -= 10000;
//            Cursor = Cursors.Arrow;
            Cursor = _cursorBeforeEnter;
            if (!string.IsNullOrEmpty(_title))
                _popup.IsOpen = false;
        }

        void MarkerControl_MouseEnter(object sender, MouseEventArgs e)
        {
            _marker.ZIndex += 10000;
            _cursorBeforeEnter = Cursor;
            Cursor = Cursors.Hand;

            if (!string.IsNullOrEmpty(_title))
                _popup.IsOpen = true;
        }
    }
}
