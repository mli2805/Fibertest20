namespace Iit.Fibertest.RtuMngr;

public class RtuSettingsEf
{
    public int Id { get; set; }

        public bool IsMonitoringOn { get; set; }
        public bool IsAutoBaseMeasurementInProgress { get; set; }

    public DateTime LastMeasurement { get; set; }
    public DateTime LastAutoBase { get; set; }

    public DateTime LastRestartByWatchDog { get; set; }
    public DateTime LastCheckedByWatchDog { get; set; }
}