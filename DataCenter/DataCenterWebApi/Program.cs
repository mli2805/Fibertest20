using System;
using System.IO;
using System.Net;
using Iit.Fibertest.InstallLib;
using Iit.Fibertest.UtilsLib;
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
            var webApiSettings = GetWebApiSettings();

            if (webApiSettings.ApiProtocol == "http")
            {
                return Host.CreateDefaultBuilder(args)
                    .ConfigureWebHostDefaults(webBuilder =>
                    {
                        webBuilder.ConfigureKestrel(serverOptions =>
                            {
                                serverOptions.Listen(IPAddress.Any, 11080);
                            })
                            .UseStartup<Startup>();
                    })
                    .UseWindowsService();
            }
            else
            {
                return Host.CreateDefaultBuilder(args)
                    .ConfigureWebHostDefaults(webBuilder =>
                    {
                        webBuilder.ConfigureKestrel(serverOptions =>
                            {
                                serverOptions.Listen(IPAddress.Any, 11080,
                                    listenOptions =>
                                    {
                                        listenOptions.UseHttps(
                                            webApiSettings.SslCertificatePath,
                                            webApiSettings.SslCertificatePassword);
                                    });
                            })
                            .UseStartup<Startup>();
                    })
                    .UseWindowsService();
            }
        }


        private static WebApiSettings GetWebApiSettings()
        {
            var settings = new WebApiSettings();
            try
            {
                var builder = new ConfigurationBuilder()
                    .SetBasePath(AppContext.BaseDirectory + @"\..\ini\")
                    .AddJsonFile("settings.json");

                var configuration = builder.Build();
                settings.ApiProtocol = configuration["apiProtocol"];
                settings.SslCertificatePath = configuration["sslCertificatePath"];
                settings.SslCertificatePassword = AesExt.Decrypt(configuration["sslCertificatePassword"]);
            }
            catch (Exception e)
            {
                File.AppendAllText(@"c:\FibertestWebApi.err", e.Message);
            }

            return settings;
        }
    }

    /*
     * to install / uninstall manually from command line
     * !!! full path to exe is necessary
     * sc create FibertestWaService binPath="c:\VsGitProjects\Fibertest\DataCenter\DataCenterWebApi\bin\Debug\netcoreapp3.1\Iit.Fibertest.DataCenterWebApi.exe" start= "auto" DisplayName= "Fibertest2.0 WebApi Service"
     * sc delete FibertestWaService
     */
}
