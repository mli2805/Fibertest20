﻿using System;
using System.Collections.Generic;
using Caliburn.Micro;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public class TraceInfoModel : PropertyChangedBase
    {
        public Guid TraceId { get; set; }
        public Rtu Rtu;
        public List<Guid> TraceEquipments;
        public List<Guid> TraceNodes;


        public string RtuTitle { get; set; }
        public string PortNumber { get; set; }

        public List<TraceInfoTableItem> NodesRows { get; set; }
        public List<TraceInfoTableItem> EquipmentsRows { get; set; }

        private string _title;
        public string Title
        {
            get => _title;
            set
            {
                if (value == _title) return;
                _title = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(IsButtonSaveEnabled));
            }
        }

        private bool _isTraceModeLight;
        public bool IsTraceModeLight
        {
            get => _isTraceModeLight;
            set
            {
                if (value == _isTraceModeLight) return;
                _isTraceModeLight = value;
                NotifyOfPropertyChange();
            }
        }

        private bool _isTraceModeDark;
        public bool IsTraceModeDark
        {
            get => _isTraceModeDark;
            set
            {
                if (value == _isTraceModeDark) return;
                _isTraceModeDark = value;
                NotifyOfPropertyChange();
            }
        }

        private string _comment;
        public string Comment
        {
            get { return _comment; }
            set
            {
                if (value == _comment) return;
                _comment = value;
                NotifyOfPropertyChange();
            }
        }

        public bool IsButtonSaveEnabled => !string.IsNullOrEmpty(_title);
    }
}