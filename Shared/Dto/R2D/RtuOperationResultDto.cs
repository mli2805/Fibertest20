namespace Iit.Fibertest.Dto
{
    public class RtuOperationResultDto : RequestAnswer
    {
        public string ResultJson { get; set; } // RtuInitializedDto, MonitoringSettingsApplied ...

        public RtuOperationResultDto() {}

        public RtuOperationResultDto(ReturnCode returnCode) : base(returnCode)
        {
        }
    }
}