using Iit.Fibertest.UtilsLib;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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
            services.AddCors(options => options.AddPolicy("Cors", builder => {
                builder
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
            }));
            
            services.AddControllers()
                .AddNewtonsoftJson();

            var iniFile = new IniFile();
            iniFile.AssignFile("webproxy.ini");
            services.AddSingleton(iniFile);

            var logFile = new LogFile(iniFile);
            logFile.AssignFile("webproxy.log");
            services.AddSingleton<IMyLog>(logFile);
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
//            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Trace}/{action=GetAll}/{id?}");
            });
        }
    }
}
