using System.Collections.Generic;
using System.Threading.Tasks;
using Autofac;
using Dto;
using Iit.Fibertest.Graph;
using WcfServiceForClientLibrary;

namespace Iit.Fibertest.Client
{
    public sealed class AutofacEventSourcing : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<Aggregate>().SingleInstance();
            builder.RegisterType<ReadModel>().SingleInstance();
            builder.RegisterType<TreeOfRtuModel>().SingleInstance();
            builder.RegisterType<WriteModel>().SingleInstance();
            builder.RegisterType<Bus>().SingleInstance();
            builder.RegisterType<GraphReadModel>().SingleInstance();
            //builder.RegisterType<WcfFactory>().SingleInstance();

            builder.Register<IWcfServiceForClient>(ctx => new FakeWcfServiceForClient()).SingleInstance();

            builder.Register(ioc => new ClientPoller(
                ioc.Resolve<IWcfServiceForClient>(), 
                new List<object>
                {
                    ioc.Resolve<ReadModel>(),
                    ioc.Resolve<TreeOfRtuModel>(),
                    ioc.Resolve<GraphReadModel>()
                }))
                .SingleInstance();

            builder.RegisterType<AdministrativeDb>().SingleInstance();
        }
    }

    public class FakeWcfServiceForClient : IWcfServiceForClient
    {
        public string SendCommand(string json)
        {
            return null;
        }

        public string[] GetEvents(int revision)
        {
            return new string[0];
        }

        public Task<ClientRegisteredDto> MakeExperimentAsync(RegisterClientDto dto)
        {
            throw new System.NotImplementedException();
        }

        public void RegisterClient(RegisterClientDto dto)
        {
            throw new System.NotImplementedException();
        }

        public void UnRegisterClient(UnRegisterClientDto dto)
        {
            throw new System.NotImplementedException();
        }

        public bool CheckServerConnection(CheckServerConnectionDto dto)
        {
            throw new System.NotImplementedException();
        }

        public bool CheckRtuConnection(CheckRtuConnectionDto rtuAddress)
        {
            throw new System.NotImplementedException();
        }

        public bool InitializeRtu(InitializeRtuDto rtu)
        {
            throw new System.NotImplementedException();
        }

        public bool StartMonitoring(StartMonitoringDto dto)
        {
            throw new System.NotImplementedException();
        }

        public bool StopMonitoring(StopMonitoringDto dto)
        {
            throw new System.NotImplementedException();
        }

        public bool ApplyMonitoringSettings(ApplyMonitoringSettingsDto settings)
        {
            throw new System.NotImplementedException();
        }

        public bool AssignBaseRef(AssignBaseRefDto baseRef)
        {
            throw new System.NotImplementedException();
        }
    }
}