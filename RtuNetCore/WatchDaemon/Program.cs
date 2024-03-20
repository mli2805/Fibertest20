using Iit.Fibertest.Dto;
using Iit.Fibertest.RtuMngr;
using Iit.Fibertest.UtilsNetCore;
using Serilog;

namespace Iit.Fibertest.WatchDaemon;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.SetCurrentDirectoryAndCreateDataDirectory();

        builder.Services
            .AddDependencyGroup();

        var wdConfig = ConfigUtils.GetConfigManually<WatchDogConfig>("wd.json");
        var logger = LoggerConfigurationFactory
            .ConfigureLogger(wdConfig.LogLevelMinimum, wdConfig.LogRollingInterval)
            .CreateLogger();
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