namespace Iit.Fibertest.Dto
{
    public class WatchDogConfig
    {
        public LogConfig Logging { get; set; } = new LogConfig();
        
        public string RtuDaemonExecutiveName { get; set; } = "Iit.Fibertest.RtuDaemon";
        public string RtuDaemonName { get; set; } = "rtu";
        public int MaxGapBetweenMeasurements { get; set; } = 600;
        public int MaxGapBetweenAutoBaseMeasurements { get; set; } = 300;
    }
}