using System.Linq;
using System.ServiceProcess;
using Autofac;
using DataCenterCore;
using FluentAssertions;
using Iit.Fibertest.DataCenterService;
using Iit.Fibertest.UtilsLib;
using Xunit;

namespace Tests
{
    public class ProductionAutofacExtensionsFacts
    {
        private readonly Service1 _service1;

        public ProductionAutofacExtensionsFacts()
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
            _service1.ServiceIni.FilePath.Should().EndWith("DcService.ini");
        }
    }
    public class DcManagerFacts
    {
        private DcManager _sut;

        public DcManagerFacts()
        {
            var builder = new ContainerBuilder()
                .WithProduction();

            builder.RegisterType<NullLog>().As<IMyLog>();

            var container = builder.Build();
            _sut = container.Resolve<DcManager>();
        }

        [Fact]
        public void Start()
        {
            //_sut.Start();
        }
    }
}