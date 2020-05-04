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

    public class AccidentLineDto
    {
        public string Caption { get; set; }
        public string TopLeft { get; set; }
        public string TopCenter { get; set; }
        public string TopRight { get; set; }
        public string Bottom0 { get; set; }
        public string Bottom1 { get; set; }
        public string Bottom2 { get; set; }
        public string Bottom3 { get; set; }
        public string Bottom4 { get; set; }
        public string PngPath { get; set; } 

        public GeoPoint Position { get; set; }
    }
}