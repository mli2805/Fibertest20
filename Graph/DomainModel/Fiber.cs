﻿using System;

namespace Iit.Fibertest.Graph
{
    public class Fiber
    {
        public Guid Id { get; set; }
        public Guid Node1 { get; set; }
        public Guid Node2 { get; set; }

//        public double GpsLength { get; set; } лучше не хранить, а вычислять
        public double UserInputedLength { get; set; }
        public double OpticalLength { get; set; }
    }
}