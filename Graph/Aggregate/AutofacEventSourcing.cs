using System.Collections.Generic;
using Autofac;

namespace Iit.Fibertest.Graph
{
    public sealed class AutofacEventSourcing : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<Aggregate>().SingleInstance();
            builder.RegisterType<ReadModel>().SingleInstance();
            builder.RegisterType<Db>().SingleInstance();
            builder.RegisterType<Bus>().SingleInstance();
            builder.Register(ioc => new ClientPoller(
                ioc.Resolve<Db>(), new List<object> { ioc.Resolve<ReadModel>() }));
        }
    }
}