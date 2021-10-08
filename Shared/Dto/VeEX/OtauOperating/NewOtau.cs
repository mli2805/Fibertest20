// ReSharper disable InconsistentNaming
namespace Iit.Fibertest.Dto
{
    public class VeexOtauAddress
    {
        public string address { get; set; }
        public int port { get; set; }
    }
    
    public class NewOtau
    {
        public string id { get; set; }
        public VeexOtauAddress connectionParameters { get; set; }
    }
}