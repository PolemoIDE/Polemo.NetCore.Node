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
            services.AddSmtpEmailSender("smtp.exmail.qq.com", 25, "码锋科技", "service@codecomb.com", "service@codecomb.com", "Yuuko19931101");
        }

        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(LogLevel.Debug);
            app.UseCors("Polemo");
            app.UseSignalR();
        }

        public static void Main(string[] args) => WebApplication.Run<Startup>(args);
    }
}
