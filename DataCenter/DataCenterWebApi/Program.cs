using System;
using System.IO;
using Iit.Fibertest.Dto;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
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
            var hostBuilder = Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    // this setting is used when application starts not under IIS: 
                    webBuilder.UseUrls($"{GetApiProtocol()}://*:{(int)TcpPorts.WebApiListenTo}");
                })
                .UseWindowsService();
            return hostBuilder;
        }


        private static string GetApiProtocol()
        {
            var apiProtocol = "http";
            try
            {
                var builder = new ConfigurationBuilder()
                    .SetBasePath(AppContext.BaseDirectory + @"\..\ini\")
                    .AddJsonFile("settings.json");

                var configuration = builder.Build();
                apiProtocol = configuration["apiProtocol"];
            }
            catch (Exception e)
            {
                File.AppendAllText(@"c:\FibertestWebApi.err", e.Message);
            }

            return apiProtocol;
        }
    }

    /*
     * to install / uninstall manually from command line
     * !!! full path to exe is necessary
     * sc create FibertestWaService binPath="c:\VsGitProjects\Fibertest\DataCenter\DataCenterWebApi\bin\Debug\netcoreapp3.1\Iit.Fibertest.DataCenterWebApi.exe" start= "auto" DisplayName= "Fibertest2.0 WebApi Service"
     * sc delete FibertestWaService
     */
}
