using System;
using System.IO;
using System.Reflection;
using System.Web.Http;
using Autofac;
using Autofac.Integration.WebApi;
using Fibertest.Datacenter.Web;
using Owin;
using Swashbuckle.Application;

[assembly: Microsoft.Owin.OwinStartup(typeof(Startup))]

namespace Fibertest.Datacenter.Web
{
    internal class Startup
    {
        private readonly ILifetimeScope _scope;

        // IIS will call that
        public Startup()
        {
            _scope = DefaultScope();
        }
        // Tests call that
        public Startup(ILifetimeScope scope) { _scope = scope ?? DefaultScope(); }

        private static IContainer DefaultScope()
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule<AutofacWeb>();
            return builder.Build();
        }

        public void Configuration(IAppBuilder app)
        {
            var http = new HttpConfiguration();
            http.MapHttpAttributeRoutes();
            http.DependencyResolver =
                new AutofacWebApiDependencyResolver(_scope);
            http.EnableSwagger(c =>
                {
                    c.SingleApiVersion("v1", "Sample API viewer");
                    c.IncludeXmlComments(XmlCommentsFilePath());
                })
                .EnableSwaggerUi();
            app.UseAutofacMiddleware(_scope);
            app.UseWebApi(http);
        }

        private static string XmlCommentsFilePath()
        {
            var root = AppDomain.CurrentDomain.BaseDirectory;
            var xml = Assembly.GetExecutingAssembly().GetName().Name + ".XML";
            return Path.Combine(root, "bin", xml);
        }
    }
}