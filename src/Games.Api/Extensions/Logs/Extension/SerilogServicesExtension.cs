using Microsoft.AspNetCore.Builder;
using Serilog;
using Serilog.Exceptions;
using System.Linq;

namespace Games.Api.Extensions.Logs.Extension
{
    public static class SerilogServicesExtension
    {
        public static WebApplicationBuilder AddSerilogConfiguration(this WebApplicationBuilder builder)
        {
            builder.Host.UseSerilog((context, logger) =>
            {
                logger.Enrich.FromLogContext();
                logger.Enrich.WithExceptionDetails();
                logger.Enrich.WithMachineName();
                logger.MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning);
                logger.MinimumLevel.Override("System", Serilog.Events.LogEventLevel.Warning);
                logger.Filter.ByExcluding(c => c.Properties.Any(p => p.Value.ToString().Contains("/ready")));
                logger.Filter.ByExcluding(c => c.Properties.Any(p => p.Value.ToString().Contains("/liveness")));
                logger.Filter.ByExcluding(c => c.Properties.Any(p => p.Value.ToString().Contains("/hc")));
                logger.ReadFrom.Configuration(context.Configuration);
            });

            return builder;
        }
    }
}
