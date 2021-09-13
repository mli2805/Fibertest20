// ReSharper disable InconsistentNaming
namespace Iit.Fibertest.Dto
{
    public class CreateOtauAddress
    {
        public string address { get; set; }
        public int port { get; set; }
    }
    
    public class CreateOtau
    {
        public CreateOtauAddress connectionParameters { get; set; }
    }
}