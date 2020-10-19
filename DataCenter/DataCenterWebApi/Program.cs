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
                    // no need: 
                    //  there are settings on Debug page for start under VS
                    //  there are binding settings for site for start under IIS 
                    // webBuilder.UseUrls("http://*:11080");
                    webBuilder.UseUrls("https://*:44335");
                });
        }
    }
}
