﻿using System;

namespace Iit.Fibertest.Graph
{
    public class Node
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public bool IsAdjustmentPoint { get; set; }
        public string Comment { get; set; }
    }
}