using System.Linq;
using System.ServiceProcess;
using Autofac;
using FluentAssertions;
using Iit.Fibertest.DataCenterCore;
using Iit.Fibertest.DataCenterService;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfServiceForClientInterface;
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

    public class WcfServiceForClientFacts
    {
        private IWcfServiceForClient _sut;

        public WcfServiceForClientFacts()
        {
            var builder = new ContainerBuilder()
                .WithProduction();

            builder.RegisterType<NullLog>().As<IMyLog>();

            var container = builder.Build();
            _sut = container.Resolve<IWcfServiceForClient>();
        }

        [Fact]
        public void FactMethodName()
        {
            
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