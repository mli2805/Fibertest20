﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        #region Current mouse coordinates

        public CurrentGis CurrentGis
        {
            get => _currentGis;
            set
            {
                if (Equals(value, _currentGis)) return;
                _currentGis = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(MouseCurrentCoorsString));
            }
        }

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

        public MapLimits Limits = new MapLimits();
        public string MouseCurrentCoorsString => CurrentGis.IsGisOn
            ? Zoom + " ; " + _mouseCurrentCoors.ToDetailedString(CurrentGis.GpsInputMode) + " ; " + Limits.NodeCountString
            : "";

        #endregion

        public void SetPosition(PointLatLng position)
        {
            Position = position;
            EvaluateMapLimits();
            OnPropertyChanged(nameof(MouseCurrentCoorsString));
        }

        #region Distance measurement properties
        public bool IsInDistanceMeasurementMode { get; set; }

        public List<GMapMarker> DistanceMarkers;

        public List<int> Distances;

        private int _lastDistance;
        private CurrentGis _currentGis;

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
        #endregion

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                if (IsInDistanceMeasurementMode)
                    LeaveDistanceMeasurementMode();
            }
            base.OnKeyDown(e);
        }

        public void LeaveDistanceMeasurementMode()
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

        public void EvaluateMapLimits(double winHeight = 0, double winWidth = 0)
        {
            var leftTop = winWidth == 0
                ? FromLocalToLatLng(GetPointFromPosition(new Point(0, 0)))
                : FromLocalToLatLng(GetPointFromPosition(new Point(-600, 0)));
            var rightBottom = winHeight == 0
                ? FromLocalToLatLng(GetPointFromPosition(new Point(ActualWidth, ActualHeight)))
                : FromLocalToLatLng(GetPointFromPosition(new Point(winHeight - 165, winWidth)));
            Limits.Set(leftTop, rightBottom);
            OnPropertyChanged(nameof(MouseCurrentCoorsString));
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

                var mousePoint = e.GetPosition(this);
                mousePoint.X = mousePoint.X - 1;
                mousePoint.Y = mousePoint.Y - 1;
                var endMarkerPosition = FromLocalToLatLng(GetPointFromPosition(mousePoint));
                Markers.Add(new GMapRoute(DistanceFiberUnderCreation, StartNode.Id, Guid.Empty, Brushes.Black, 1,
                    new List<PointLatLng>() { StartNode.Position, endMarkerPosition }, this));
                LastDistance = (int)GisLabCalculator.GetDistanceBetweenPointLatLng(StartNode.Position, endMarkerPosition);
            }
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);
            EvaluateMapLimits();
            OnPropertyChanged(nameof(MouseCurrentCoorsString));
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            EvaluateMapLimits();
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
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
                        new List<PointLatLng>() { StartNode.Position, markerPosition }, this);
                    Markers.Add(routeMarker);
                    DistanceMarkers.Add(routeMarker);

                    Distances.Add((int)GisLabCalculator.GetDistanceBetweenPointLatLng(StartNode.Position, markerPosition));
                }

                Markers.Add(marker);
                DistanceMarkers.Add(marker);
                StartNode = marker;
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
