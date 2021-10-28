using Iit.Fibertest.Graph;

namespace Graph.Tests
{
    public class FakeMachineKeyProvider : IMachineKeyProvider
    {
        public string Get()
        {
            return "CpuIdMatherBoardSerialDiskDriveSerial";
        }
    }
}
