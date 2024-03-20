namespace Iit.Fibertest.Dto
{
    public class WatchDogConfig
    {
        public string LogLevelMinimum { get; set; } = "Information";
        public string LogRollingInterval { get; set; } = "Month";
        
        public string RtuDaemonExecutiveName { get; set; } = "Iit.Fibertest.RtuDaemon";
        public string RtuDaemonName { get; set; } = "rtu";
        public int MaxGapBetweenMeasurements { get; set; } = 600;
        public int MaxGapBetweenAutoBaseMeasurements { get; set; } = 300;
    }
}