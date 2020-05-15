using System.Linq;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

namespace Iit.Fibertest.DataCenterWebApi
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options => options.AddPolicy("Cors", builder =>
            {
                builder
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials()
                    .SetIsOriginAllowed(hostName => true);
            }));

            services.AddControllers()
                .AddNewtonsoftJson();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateLifetime = true,
                        IssuerSigningKey = AuthOptions.GetSymmetricSecurityKey(),
                        ValidateIssuerSigningKey = true,
                    };
                    // for SignalR
                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var accessToken = context.Request.Query["access_token"];

                            // если запрос направлен хабу
                            var path = context.HttpContext.Request.Path;
                            if (!string.IsNullOrEmpty(accessToken) &&
                                (path.StartsWithSegments("/webApiSignalRHub")))
                            {
                                // получаем токен из строки запроса
                                context.Token = accessToken;
                            }
                            return Task.CompletedTask;
                        }
                    };
                });

            services.AddSignalR();

            var iniFile = new IniFile();
            iniFile.AssignFile("webapi.ini");
            SetServerAddress(iniFile);
            SetLocalIpAddress(iniFile);
            services.AddSingleton(iniFile);

            var logFile = new LogFile(iniFile);
            logFile.AssignFile("webapi.log");
            services.AddSingleton<IMyLog>(logFile);
            logFile.AppendLine("Fibertest WebApi service started");

        }

        private static void SetServerAddress(IniFile iniFile)
        {
            var main = iniFile.Read(IniSection.ServerMainAddress, (int) TcpPorts.ServerListenToWebClient);
            if (main.Ip4Address == "0.0.0.0")
            {
                // as for now, WebApi is set up on the same machine as DataCenter
                main.Ip4Address = LocalAddressResearcher.GetAllLocalAddresses().First();
                iniFile.Write(main, IniSection.ServerMainAddress);
            }
        }

        private void SetLocalIpAddress(IniFile iniFile)
        {
            var serverDoubleAddress = iniFile.ReadDoubleAddress((int)TcpPorts.ServerListenToWebClient);
            var clientAddress = iniFile.Read(IniSection.ClientLocalAddress, 11080);
            if (clientAddress.IsAddressSetAsIp && clientAddress.Ip4Address == @"0.0.0.0" &&
                serverDoubleAddress.Main.Ip4Address != @"0.0.0.0")
            {
                clientAddress.Ip4Address = LocalAddressResearcher.GetLocalAddressToConnectServer(serverDoubleAddress.Main.Ip4Address);
                iniFile.Write(clientAddress, IniSection.ClientLocalAddress);
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors("Cors");

            //app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Trace}/{action=GetAll}/{id?}");
                endpoints.MapHub<SignalRHub>("/webApiSignalRHub");
            });
        }
    }
}
