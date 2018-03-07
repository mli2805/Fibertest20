﻿using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Graph
{
    public class AccidentOnTrace
    {
        public int BrokenRftsEventNumber { get; set; }

        public double AccidentDistanceKm { get; set; } 
        public FiberState AccidentSeriousness { get; set; } 

        public OpticalAccidentType OpticalTypeOfAccident { get; set; }

    }
}