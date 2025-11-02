using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Formatting.Compact;
using Serilog.Sinks.GrafanaLoki;

namespace SharedX.Abstractions.Logging;
public static class SerilogExtensions
{
    public static IHostBuilder AddSerilogLogging(this IHostBuilder hostBuilder, string serviceName)
    {
        hostBuilder.UseSerilog((context, configuration) =>
        {
            var lokiUrl = context.Configuration["Serilog:LokiUrl"];
            ArgumentException.ThrowIfNullOrWhiteSpace(lokiUrl);
            var lokiApiKey = context.Configuration["Serilog:LokiApiKey"]; // if you use auth
            var environment = context.HostingEnvironment.EnvironmentName;

            configuration
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Application", serviceName)
                .Enrich.WithProperty("Environment", environment)
                .Enrich.WithMachineName()
                .Enrich.WithThreadId()
                .Enrich.WithExceptionDetails() // Serilog.Exceptions
                .WriteTo.Console(new CompactJsonFormatter())
                .WriteTo.File(new CompactJsonFormatter(),
                    $"logs/{serviceName}-.json",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 13)
                .Filter.ByExcluding(le =>
                {
                    if (le.Properties.TryGetValue("RequestPath", out var v) &&
                        v is ScalarValue sv && sv.Value is string p)
                    {
                        return p.StartsWith("/health", StringComparison.OrdinalIgnoreCase)
                               || p.StartsWith("/metrics", StringComparison.OrdinalIgnoreCase);
                    }

                    return false;
                });


            var labels = new Dictionary<string, string>
            {
                ["app"] = serviceName,
                ["environment"] = environment
            };

            configuration.WriteTo.GrafanaLoki(
                lokiUrl,
                labels: labels,
                period: TimeSpan.FromSeconds(3));
        });

        return hostBuilder;
    }
}