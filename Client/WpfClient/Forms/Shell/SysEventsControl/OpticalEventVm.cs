﻿using System;
using System.Windows.Media;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Client
{
    public class OpticalEventVm
    {
        public int Nomer { get; set; }
        public DateTime EventTimestamp { get; set; }
        public string RtuTitle { get; set; }
        public string TraceTitle { get; set; }
        public Brush BaseRefTypeBrush { get; set; }
        public FiberState TraceState { get; set; }

        public string EventStatus { get; set; }
        public string StatusTimestamp { get; set; }
        public string StatusUsername { get; set; }

        public string Comment { get; set; }
    }
}