using Iit.Fibertest.Dto;
using Iit.Fibertest.RtuMngr;
using Iit.Fibertest.UtilsNet6;
using Microsoft.EntityFrameworkCore;

namespace Iit.Fibertest.RtuDaemon;

public static class RtuDependencyCollectionExtension
{
    public static IServiceCollection AddDependencyGroup(this IServiceCollection services)
    {
        return services
            .AddConfigAsInstance()
            .AddBootAndBackgroundServices()
            .AddDb()
            .AddOther();
    }

    private static IServiceCollection AddConfigAsInstance(this IServiceCollection services)
    {
        return services
            .AddSingleton<IWritableConfig<RtuConfig>>(_ => new WritableConfig<RtuConfig>("rtu.json"));
    }

    private static IServiceCollection AddBootAndBackgroundServices(this IServiceCollection services)
    {
        services.AddSingleton<Boot>();
        services.AddHostedService(x => x.GetService<Boot>()!);
        services.AddSingleton<MonitoringService>();
        services.AddHostedService(x => x.GetService<MonitoringService>()!);
        return services;
    }

    private static IServiceCollection AddDb(this IServiceCollection services)
    {
        // !!!! DO NOT USE for c-tor injection !!!!
        // get them in methods using IServiceProvider => scope => resolve

        services.AddDbContext<RtuContext>(c => c.UseSqlite("Data Source=data/rtu.db;Cache=Shared"));
        services.AddScoped<RtuContextInitializer>();
        services.AddScoped<EventsRepository>();
        services.AddScoped<MonitoringResultsRepository>();
        services.AddScoped<LongOperationRepository>();
        return services;
    }

    private static IServiceCollection AddOther(this IServiceCollection services)
    {
        services.AddSingleton<GreeterService>();

        services.AddSingleton<InterOpWrapper>();
        services.AddSingleton<OtdrManager>();
        services.AddSingleton<RtuManager>();

        services.AddSingleton<CommandProcessor>();
        return services;
    }
}