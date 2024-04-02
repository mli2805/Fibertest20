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

        var wdConfig = new WritableConfig<WatchDogConfig>("wd.json");
        var logger = LoggerConfigurationFactory
            .ConfigureLogger(wdConfig.Value.Logging.LogLevelMinimum, 
                wdConfig.Value.Logging.LogRollingSizeKb, wdConfig.Value.Logging.LogFileCount)
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