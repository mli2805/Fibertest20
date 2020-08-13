namespace Iit.Fibertest.Dto
{
    public class RftsEventsDto
    {
        public ReturnCode ReturnCode;
        public string ErrorMessage;
        
        public RftsLevelDto[] LevelArray;
    }

    public class RftsLevelDto
    {
        public string Title;

    }
}
