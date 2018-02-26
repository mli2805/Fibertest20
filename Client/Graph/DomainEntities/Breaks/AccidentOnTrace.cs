using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Graph
{
    public class AccidentOnTrace
    {
        public double BreakKm { get; set; } 
        public FiberState AccidentSeriousness { get; set; } 

        public OpticalAccidentType BreakType { get; set; }

    }
}