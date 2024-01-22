using Iit.Fibertest.Dto;
using Iit.Fibertest.RtuMngr;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.UtilsNetCore;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Serilog;
using Serilog.Events;

namespace Iit.Fibertest.RtuDaemon
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            SetCurrentDirectoryAndCreateDataDirectory(builder);
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

            using (var scope = app.Services.CreateScope())
            {
                var initializer = scope.ServiceProvider.GetRequiredService<RtuContextInitializer>();
                await initializer.InitializeAsync();
            }

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


        static void SetCurrentDirectoryAndCreateDataDirectory(WebApplicationBuilder builder)
        {
            if (builder.Environment.IsEnvironment("Test"))
            {
                // don't need to create data directory for functional tests
                // everything should be in memory
                return;
            }

            // By default Visual Studio sets current directory of .NET Core Web projects (including Api)
            // to the /app directory. This could be handy if there are some CSS or JS files
            // which user can change using Visual Studio and expect to see the result in browser immediately.
            // As a consequence of this, the serilog's log folder appear at /app directory as well as sqlite db.

            // We don't need to edit anything on the fly on our Api project, so 
            // let's change the current directory to project output directory.
            // var assemblyLocation = AppContext.BaseDirectory;
            // Directory.SetCurrentDirectory(Path.GetDirectoryName(assemblyLocation)!);
            //
            // // Create a directory for stored data (like sqlite database)
            // Directory.CreateDirectory("data");

            var fibertestPath = FileOperations.GetMainFolder();
            var dataFolder = Path.Combine(fibertestPath, @"data");

            if (!Directory.Exists(dataFolder))
            {
                Directory.CreateDirectory(dataFolder);
            }
        }

    }
}