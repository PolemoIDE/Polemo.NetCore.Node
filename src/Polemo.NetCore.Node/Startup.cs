using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Polemo.NetCore.Node
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddConfiguration();

            services.AddCors(c => c.AddPolicy("Polemo", x =>
                x.AllowCredentials()
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
            ));

            services.AddSignalR(options =>
            {
                options.Hubs.EnableDetailedErrors = true;
            });

            services.AddAesCrypto();
        }

        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
        {
            app.UseCors("Polemo");
            app.UseWebSockets();
            app.UseSignalR();
            loggerFactory.AddConsole(LogLevel.Debug);
        }

        public static void Main(string[] args) => WebApplication.Run<Startup>(args);
    }
}
