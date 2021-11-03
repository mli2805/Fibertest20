// ReSharper disable InconsistentNaming
namespace Iit.Fibertest.Dto
{
    public class ReinitializeOtdrStatus
    {
        public string id { get; set; }
        public int numberOfReconnectedOtdrs { get; set; }

        // "status" is one of: "pending", "processing", "processed".
        public string status { get; set; }
    }
}
