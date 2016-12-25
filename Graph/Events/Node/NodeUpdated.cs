﻿using System;

namespace Iit.Fibertest.Graph.Events
{
    public class NodeUpdated
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }

    }
}