using Iit.Fibertest.Dto;
using Iit.Fibertest.RtuMngr;
using Iit.Fibertest.UtilsNetCore;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Serilog;

namespace Iit.Fibertest.RtuDaemon;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.SetCurrentDirectoryAndCreateDataDirectory();
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

        var rtuConfig = ConfigUtils.GetConfigManually<RtuConfig>("rtu.json");
        var logger = LoggerConfigurationFactory
            .ConfigureLogger(rtuConfig.General.LogLevelMinimum, rtuConfig.General.LogRollingInterval)
            .CreateLogger();
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
}