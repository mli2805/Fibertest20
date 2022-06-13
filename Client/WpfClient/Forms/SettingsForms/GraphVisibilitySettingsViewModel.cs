﻿using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.Client
{
    public class GraphVisibilitySettingsViewModel : Screen
    {
        private readonly IniFile _iniFile;
        private readonly CurrentGis _currentGis;

        private bool _isHighDensityGraph;

        public bool IsHighDensityGraph
        {
            get => _isHighDensityGraph;
            set
            {
                if (value == _isHighDensityGraph) return;
                _isHighDensityGraph = value;
                SelectedZoom = _isHighDensityGraph ? 16 : 12;
                SetZoomList();
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(ZoomList));
                NotifyOfPropertyChange(nameof(SelectedZoom));
            }
        }

        private List<int> _zoomList;
        public List<int> ZoomList
        {
            get => _zoomList;
            private set
            {
                if (Equals(value, _zoomList)) return;
                _zoomList = value;
                NotifyOfPropertyChange();
            }
        }

        private int _selectedZoom;
        public int SelectedZoom
        {
            get => _selectedZoom;
            set
            {
                if (value == _selectedZoom) return;
                _selectedZoom = value;
                NotifyOfPropertyChange();
            }
        }


        public List<int> ShiftList { get; } = new List<int>() { 10, 20, 30, 40, 50, 60, 70, 80, 90, 100, 150, 200, 250, 300 };
        public int SelectedShift { get; set; }
        public Visibility ShiftVisibility { get; set; }

        public GraphVisibilitySettingsViewModel(IniFile iniFile, CurrentGis currentGis, CurrentUser currentUser)
        {
            _iniFile = iniFile;
            _currentGis = currentGis;

            IsHighDensityGraph = currentGis.IsHighDensityGraph;
            SelectedZoom = currentGis.ThresholdZoom;
            SelectedShift = (int)(currentGis.ScreenPartAsMargin * 100);
            ShiftVisibility = currentUser.Role == Role.Developer ? Visibility.Visible : Visibility.Collapsed;
        }

        protected override void OnViewLoaded(object view)
        {
            SetZoomList();
            DisplayName = Resources.SID_Graph_visibility_settings;
        }

        private void SetZoomList()
        {
            ZoomList = _isHighDensityGraph ? Enumerable.Range(14, 8).ToList() : Enumerable.Range(10, 12).ToList();

        }

        public void Save()
        {
            _currentGis.IsHighDensityGraph = IsHighDensityGraph;
            _iniFile.Write(IniSection.Map, IniKey.IsHighDensityGraph, IsHighDensityGraph);
            _currentGis.ThresholdZoom = SelectedZoom;
            _iniFile.Write(IniSection.Map, IniKey.ThresholdZoom, SelectedZoom);
            _currentGis.ScreenPartAsMargin = SelectedShift / 100.0;
            _iniFile.Write(IniSection.Map, IniKey.ScreenPartAsMargin, _currentGis.ScreenPartAsMargin);

            TryClose();
        }

        public void Cancel()
        {
            TryClose();
        }
    }
}