namespace Iit.Fibertest.Graph
{
    public class AccidentAsNewEvent : AccidentOnTrace
    {
        public double LeftNodeKm { get; set; }
        public int LeftLandmarkIndex { get; set; }
        public double RightNodeKm { get; set; }
        public int RightLandmarkIndex { get; set; }

    }
}