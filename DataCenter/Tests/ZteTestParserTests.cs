using FluentAssertions;
using Iit.Fibertest.DataCenterCore;
using Xunit;

namespace Tests
{
    public class ZteTestParserTests
    {
        [Fact]
        public void GetSlotTest()
        {
            285282576.ExtractNumberFromZteCode(1).Should().Be(17);
        }

        [Fact]
        public void GetGponInterfaceTest()
        {
            285282576.ExtractNumberFromZteCode(0).Should().Be(16);
        }
    }
}