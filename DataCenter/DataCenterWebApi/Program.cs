using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace Iit.Fibertest.DataCenterWebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //  CreateHostBuilder(args).Build().Run();
            CreateWebHostBuilder(args).Build().Run();
        }

//        private static IHostBuilder CreateHostBuilder(string[] args) =>
//            Host.CreateDefaultBuilder(args)
//                .ConfigureWebHostDefaults(webBuilder =>
//                {
//                    webBuilder.UseStartup<Startup>();
//                });

        private static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseUrls("http://*:8017;https://*:44317");
        }
    }
}
