using Iit.Fibertest.Dto;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Iit.Fibertest.DataCenterWebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            // just as example
//            var builder = new ConfigurationBuilder()
//                .SetBasePath(Directory.GetCurrentDirectory())
//                .AddJsonFile("appsettings.json");
//
//            var configuration = builder.Build();
//            var logLevel = configuration["Logging:LogLevel:Default"];
//            Console.Write(logLevel);
            //

            return Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    // this setting is used when application starts in console: 
                    webBuilder.UseUrls($"http://*:{(int)TcpPorts.WebApiListenTo}");
//                    webBuilder.UseUrls("https://*:11080");
                })
                .UseWindowsService();
        }
    }
}
