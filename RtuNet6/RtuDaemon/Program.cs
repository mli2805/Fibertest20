using Fibertest.RtuDaemon.Services;
using Iit.Fibertest.Dto;
using Microsoft.AspNetCore.Server.Kestrel.Core;

namespace Fibertest.RtuDaemon
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.WebHost
                .ConfigureKestrel(options =>
                {
                    options.ListenAnyIP((int)TcpPorts.RtuListenToGrpc, o => o.Protocols = HttpProtocols.Http2);
                    options.ListenAnyIP((int)TcpPorts.RtuListenToHttp, o => o.Protocols = HttpProtocols.Http1);
                });

            builder.Services.AddControllers();
            builder.Services.AddGrpc();

            var app = builder.Build();
            app.UseRouting();
            app.UseCors();

            // Configure the HTTP request pipeline.
            app.MapGrpcService<GreeterService>().RequireCors("AllowAll");
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers(); // check it: http://localhost:11980/misc/checkapi
            });

            app.Run();
        }
    }
}