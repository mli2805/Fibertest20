using Iit.Fibertest.RtuMngr;
using Iit.Fibertest.UtilsNetCore;
using Serilog;
using Serilog.Events;

namespace Iit.Fibertest.WatchDaemon;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

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

        app.Run();
    }
}