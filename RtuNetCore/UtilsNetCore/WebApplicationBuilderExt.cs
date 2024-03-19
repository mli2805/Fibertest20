using Iit.Fibertest.UtilsLib;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog.Events;

namespace Iit.Fibertest.UtilsNetCore
{
    public static class WebApplicationBuilderExt
    {
        public static LogEventLevel GetLogEventLevelFromAppSettingJson(this WebApplicationBuilder builder)
        {
            var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(builder.Environment.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            var configurationRoot = configurationBuilder.Build();
            var logLevel =  configurationRoot.GetSection("Logging:LogLevel:Default").Value ?? "Information";
            if (!Enum.TryParse(logLevel, true, out LogEventLevel logEventLevel))
                logEventLevel = LogEventLevel.Information;
            return logEventLevel;
        }

        public static void SetCurrentDirectoryAndCreateDataDirectory(this WebApplicationBuilder builder)
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
