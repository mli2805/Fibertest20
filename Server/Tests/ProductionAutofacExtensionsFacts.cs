using System.Linq;
using System.ServiceProcess;
using Autofac;
using FluentAssertions;
using Iit.Fibertest.DataCenterService;
using Xunit;

namespace Tests
{
    public class ProductionAutofacExtensionsFacts
    {
        [Fact]
        public void Injected_Ini_File_Should_Have_Correct_File_Assigned()
        {
            var builder = new ContainerBuilder()
                .WithProduction();

            var container = builder.Build();
            var service1 = container.Resolve<ServiceBase[]>()
                .Single().Should().BeOfType<Service1>()
                .Which;
            service1.ServiceIni.FilePath.Should().EndWith("DcService.ini");
        }
    }
}