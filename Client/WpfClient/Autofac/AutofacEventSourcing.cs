using System.Collections.Generic;
using Autofac;
using Iit.Fibertest.Graph;
using WcfConnections.C2DWcfServiceReference;
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
            builder.Register(ioc => new ClientPoller(
                ioc.Resolve<IWcfServiceForClient>(), new List<object>
                {
                    ioc.Resolve<ReadModel>(),
                    ioc.Resolve<TreeOfRtuModel>(),
                    ioc.Resolve<GraphReadModel>()
                }))
                .SingleInstance();

            builder.RegisterType<AdministrativeDb>().SingleInstance();

        }
    }
}