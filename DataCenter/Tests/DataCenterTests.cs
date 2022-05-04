using System.Linq;
using System.ServiceProcess;
using Autofac;
using FluentAssertions;
using Iit.Fibertest.DataCenterCore;
using Iit.Fibertest.DataCenterService;
using Iit.Fibertest.UtilsLib;
using Xunit;

namespace Tests
{
    public class DataCenterTests
    {
        private readonly Service1 _service1;

        public DataCenterTests()
        {
            var builder = new ContainerBuilder()
                .WithProduction();

            builder.RegisterType<NullLog>().As<IMyLog>();

            var container = builder.Build();
            _service1 = container.Resolve<ServiceBase[]>()
                .Single().Should().BeOfType<Service1>()
                .Which;
        }

        [Fact]
        public void Injected_Ini_File_Should_Have_Correct_File_Assigned()
        {
            _service1.IniFile.FilePath.Should().EndWith("DataCenter.ini");
        }
    }

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