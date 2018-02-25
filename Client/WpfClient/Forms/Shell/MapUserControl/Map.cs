using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using GMap.NET;
using GMap.NET.WindowsPresentation;
using Iit.Fibertest.Graph;
using JetBrains.Annotations;

namespace Iit.Fibertest.Client
{
    /// <summary>
    /// the custom map of GMapControl 
    /// </summary>
    [Localizable(false)]
    public class Map : GMapControl, INotifyPropertyChanged
    {
        #region Debug
#if DEBUG
        public long ElapsedMilliseconds;
        DateTime _start;
        DateTime _end;
        int _delta;

        private int _counter;
        readonly Typeface _tf = new Typeface("GenericSansSerif");
        readonly FlowDirection fd = new FlowDirection();

        /// <summary>
        /// any custom drawing here
        /// </summary>
        /// <param name="drawingContext"></param>
        protected override void OnRender(DrawingContext drawingContext)
        {
            _start = DateTime.Now;

            base.OnRender(drawingContext);

            _end = DateTime.Now;
            _delta = (int)(_end - _start).TotalMilliseconds;

            var text = new FormattedText(string.Format(CultureInfo.InvariantCulture, "{0:0.0}", Zoom) + "z, " + MapProvider + ", refresh: " + _counter++ + ", load: " + ElapsedMilliseconds + "ms, render: " + _delta + "ms", CultureInfo.InvariantCulture, fd, _tf, 20, Brushes.Blue);
            drawingContext.DrawText(text, new Point(text.Height, text.Height));
        }
#endif
        #endregion

        #region Current mouse coordinates
        public GpsInputMode CurrentGpsInputMode { get; set; } = GpsInputMode.DegreesMinutesAndSeconds;

        private PointLatLng _mouseCurrentCoors;
        public PointLatLng MouseCurrentCoors
        {
            get => _mouseCurrentCoors;
            set
            {
                if (value.Equals(_mouseCurrentCoors)) return;
                _mouseCurrentCoors = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(MouseCurrentCoorsString));
            }
        }

        public string MouseCurrentCoorsString =>
            _mouseCurrentCoors.ToDetailedString(CurrentGpsInputMode);
        #endregion

        public bool IsInDistanceMeasurementMode { get; set; }
        

        public List<GMapMarker> DistanceMarkers;

        public List<int> Distances;

        private int _lastDistance;
        public int LastDistance
        {
            get => _lastDistance;
            set
            {
                if (value == _lastDistance) return;
                _lastDistance = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(MeasuredDistance));
            }
        }

        public string MeasuredDistance => IsInDistanceMeasurementMode ? $"{_lastDistance} m  / {Distances.Sum() + _lastDistance} m" : "";

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                if (IsInDistanceMeasurementMode)
                {
                    IsInDistanceMeasurementMode = false;
                    if (DistanceFiberUnderCreation != Guid.Empty)
                        Markers.Remove(Markers.Single(m => m.Id == DistanceFiberUnderCreation));
                    foreach (var marker in DistanceMarkers)
                    {
                        Markers.Remove(marker);
                    }
                    DistanceFiberUnderCreation = Guid.Empty;
                    Distances = new List<int>();
                    LastDistance = 0;
                }
            }

            if (e.Key == Key.Z)
            {
                if (IsInDistanceMeasurementMode)
                {
                    var markerPosition = FromLocalToLatLng(GetPointFromPosition(Mouse.GetPosition(this)));
                    var marker = new GMapMarker(Guid.NewGuid(), markerPosition, false);

                    if (StartNode == null)
                    {
                        DistanceMarkers = new List<GMapMarker>();
                        Distances = new List<int>();
                    }
                    else
                    {
                        var routeMarker = new GMapRoute(FiberUnderCreation, StartNode.Id, marker.Id, Brushes.Blue, 2,
                            new List<PointLatLng>() { StartNode.Position, markerPosition });
                        Markers.Add(routeMarker);
                        DistanceMarkers.Add(routeMarker);

                        Distances.Add((int)GpsCalculator.GetDistanceBetweenPointLatLng(StartNode.Position, markerPosition));
                    }

                    Markers.Add(marker);
                    DistanceMarkers.Add(marker);
                    StartNode = marker;
                }
            }

            base.OnKeyDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            MouseCurrentCoors = FromLocalToLatLng(GetPointFromPosition(e.GetPosition(this)));

            if (IsInDistanceMeasurementMode && StartNode != null)
            {
                if (DistanceFiberUnderCreation == Guid.Empty)
                    DistanceFiberUnderCreation = Guid.NewGuid();
                else
                    Markers.Remove(Markers.Single(m => m.Id == DistanceFiberUnderCreation));

                var endMarkerPosition = FromLocalToLatLng(GetPointFromPosition(e.GetPosition(this)));
                Markers.Add(new GMapRoute(DistanceFiberUnderCreation, StartNode.Id, Guid.Empty, Brushes.Black, 1,
                    new List<PointLatLng>() { StartNode.Position, endMarkerPosition }));
                LastDistance = (int)GpsCalculator.GetDistanceBetweenPointLatLng(StartNode.Position, endMarkerPosition);
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
