using Iit.Fibertest.Dto;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using RtuDaemon.Services;

namespace RtuDaemon
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

            // Additional configuration is required to successfully run gRPC on macOS.
            // For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682

            // Add services to the container.
            builder.Services.AddGrpc();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            app.MapGrpcService<GreeterService>();
            app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

            app.Run();
        }
    }
}