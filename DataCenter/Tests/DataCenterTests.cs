using System.Linq;
using System.ServiceProcess;
using Autofac;
using FluentAssertions;
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

  
}