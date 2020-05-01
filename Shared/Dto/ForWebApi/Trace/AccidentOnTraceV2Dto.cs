namespace Iit.Fibertest.Dto
{
    public class AccidentOnTraceV2Dto
    {
        public int BrokenRftsEventNumber;

        public FiberState AccidentSeriousness;
        public OpticalAccidentType OpticalTypeOfAccident;

        public bool IsAccidentInOldEvent;
        public bool IsAccidentInLastNode;
        public GeoPoint AccidentCoors;

        public int AccidentLandmarkIndex;
        public double AccidentToRtuOpticalDistanceKm;
        public string AccidentTitle;
        public double AccidentToRtuPhysicalDistanceKm;

        public double AccidentToLeftOpticalDistanceKm;
        public double AccidentToLeftPhysicalDistanceKm;
        public double AccidentToRightOpticalDistanceKm;
        public double AccidentToRightPhysicalDistanceKm;

        public AccidentNeighbourDto Left;
        public AccidentNeighbourDto Right;
    }
}