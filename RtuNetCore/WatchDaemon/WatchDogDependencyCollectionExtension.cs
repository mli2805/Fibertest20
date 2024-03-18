using Iit.Fibertest.Dto;
using Iit.Fibertest.RtuMngr;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.UtilsNetCore;
using Microsoft.EntityFrameworkCore;

namespace Iit.Fibertest.WatchDaemon;

public static class WatchDogDependencyCollectionExtension
{
    public static IServiceCollection AddDependencyGroup(this IServiceCollection services)
    {
        return services
            .AddConfigAsInstance()
            .AddBootAndBackgroundServices()
            .AddDb()
            .AddOthers();
    }

    private static IServiceCollection AddConfigAsInstance(this IServiceCollection services)
    {
        return services
            .AddSingleton<IWritableConfig<WatchDogConfig>>(_ => new WritableConfig<WatchDogConfig>("wd.json"));
    }

    private static IServiceCollection AddBootAndBackgroundServices(this IServiceCollection services)
    {
        services.AddSingleton<RtuWatch>();
        services.AddSingleton<Boot>();
        services.AddHostedService(x => x.GetService<Boot>()!);
        return services;
    }

    private static IServiceCollection AddDb(this IServiceCollection services)
    {
        // !!!! DO NOT USE for c-tor injection !!!!
        // get them in methods using IServiceProvider => scope => resolve

        var fibertestPath = FileOperations.GetMainFolder();
        var dataFolder = Path.Combine(fibertestPath, @"data");

        services.AddDbContext<RtuContext>(c =>
            c.UseSqlite($"Data Source={dataFolder}/rtu.db;Cache=Shared"));
        services.AddScoped<RtuContextInitializer>();
        services.AddScoped<RtuSettingsRepository>();
        return services;
    }

    private static IServiceCollection AddOthers(this IServiceCollection services)
    {
        services.AddSingleton<DebianServiceManager>();
        return services;
    }
}