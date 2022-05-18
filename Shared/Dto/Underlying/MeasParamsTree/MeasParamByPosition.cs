namespace Iit.Fibertest.Dto
{
    public class MeasParamByPosition
    {
        public ServiceFunctionFirstParam Param { get; set; }
        public int Position { get; set; }
    }

    public class MeasParamByValue
    {
        public ServiceFunctionFirstParam Param { get; set; }
        public string ValueStr { get; set; }
    }
}