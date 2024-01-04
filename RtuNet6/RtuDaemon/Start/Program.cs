using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsNet6;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Serilog;
using Serilog.Events;

namespace Iit.Fibertest.RtuDaemon
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.WebHost
                .ConfigureKestrel(options =>
                {
                    options.ListenAnyIP((int)TcpPorts.RtuListenToGrpc, o => o.Protocols = HttpProtocols.Http2);
                    options.ListenAnyIP((int)TcpPorts.RtuListenToHttp, o => o.Protocols = HttpProtocols.Http1);
                });

            builder.Services.AddControllers();
            builder.Services.AddGrpc(o =>
            {
                o.Interceptors.Add<RtuLoggerInterceptor>();
            });

            builder.Services
                .AddDependencyGroup();

            var logLevel = LogEventLevel.Debug;
            var logger = LoggerConfigurationFactory.Configure(logLevel).CreateLogger();

            builder.Logging.ClearProviders();
            builder.Logging.AddSerilog(logger);

            var app = builder.Build();
            app.UseRouting();
            app.UseCors();

            // Configure the HTTP request pipeline.
            app.MapGrpcService<GreeterService>().RequireCors("AllowAll");
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers(); // check it: http://localhost:11980/misc/checkapi
            });

            app.Run();
        }
    }
}