using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.GrafanaLoki;

namespace SharedX.Abstraction.Logging;

public static class SerilogExtensions
{
    public static IHostBuilder AddSerilogLogging(
        this IHostBuilder hostBuilder,
        string serviceName)
    {
        hostBuilder.UseSerilog((context, configuration) =>
        {
            var lokiUrl = context.Configuration["Serilog:LokiUrl"];
            var lokiApiKey = context.Configuration["Serilog:LokiApiKey"];
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
                .WriteTo.Console(
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
                .WriteTo.File(
                    $"logs/{serviceName}-.log",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 7,
                    outputTemplate:
                    "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}");

            // Add Grafana Loki if configured
            if (string.IsNullOrWhiteSpace(lokiUrl)) return;
            var lokiLabels = new Dictionary<string, string>
            {
                { "app", serviceName },
                { "environment", environment }
            };

            configuration.WriteTo.GrafanaLoki(
                lokiUrl,
                labels: new Dictionary<string, string>(lokiLabels));
        });

        return hostBuilder;
    }
}