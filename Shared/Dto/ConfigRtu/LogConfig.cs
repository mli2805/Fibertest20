namespace Iit.Fibertest.Dto
{
    public class LogConfig
    {
        public string LogLevelMinimum { get; set; } = "Debug";
        // public string LogRollingInterval { get; set; } = "Month";
        public int LogRollingSizeKb { get; set; } = 20_000;
        public int LogFileCount { get; set; } = 2;

    }
}