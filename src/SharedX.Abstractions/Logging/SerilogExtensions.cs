using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Formatting.Compact;
using Serilog.Sinks.GrafanaLoki;
using Serilog.Sinks.GrafanaLoki.Common;
using Encoding = System.Text.Encoding;

namespace SharedX.Abstractions.Logging;

public static class SerilogExtensions
{
    public static IHostBuilder AddSerilogLogging(this IHostBuilder hostBuilder, string serviceName)
    {
        hostBuilder.UseSerilog((context, configuration) =>
        {
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

            // Instead of GrafanaLoki sink, use HTTP sink with Loki format
            if (!context.HostingEnvironment.IsDevelopment())
            {
                var lokiUrl = context.Configuration["Serilog:LokiUrl"];
                ArgumentException.ThrowIfNullOrWhiteSpace(lokiUrl);
                var lokiApiKey = context.Configuration["Serilog:LokiApiKey"];
                ArgumentException.ThrowIfNullOrWhiteSpace(lokiApiKey);

                var labels = new Dictionary<string, string>
                {
                    ["app"] = serviceName,
                    ["environment"] = environment
                };

                var parts = lokiApiKey.Split(':', 2);
                configuration.WriteTo.GrafanaLoki(
                    lokiUrl,
                    credentials: new GrafanaLokiCredentials { User = parts[0], Password = parts[1] },
                    labels: labels,
                    period: TimeSpan.FromSeconds(2),
                    httpClient: new LokiHttpClient());
            }
        });

        return hostBuilder;
    }
}

public class LokiHttpClient : IHttpClient
{
    private static readonly HttpClient _sharedHttpClient = new();

    public void SetCredentials(GrafanaLokiCredentials grafanaLokiCredentials)
    {
        // Set Basic Auth header and ASCII is the standard for HTTP Basic Auth!
        var authBytes = Encoding.ASCII.GetBytes($"{grafanaLokiCredentials.User}:{grafanaLokiCredentials.Password}");
        var authHeader = Convert.ToBase64String(authBytes);
        _sharedHttpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authHeader);
    }

    public async Task<HttpResponseMessage> PostAsync(string requestUri, Stream contentStream)
    {
        contentStream.Position = 0;
        using var content = new StreamContent(contentStream);
        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

        return await _sharedHttpClient.PostAsync(requestUri, content);
    }

    public bool DebugMode { get; set; } = false;

    public void Dispose()
    {
    }
}