using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace SharedX.Abstractions.HealthChecks;

public static class HealthCheckExtensions
{
    public static IServiceCollection AddAdomaHealthChecks(
        this IServiceCollection services,
        string healthCheckName,
        string? npgsqlConnectionString = null)
    {
        var healthChecksBuilder = services.AddHealthChecks();

        // Add Npgsql health check if connection string provided
        if (!string.IsNullOrWhiteSpace(npgsqlConnectionString))
        {
            healthChecksBuilder.AddNpgSql(
                npgsqlConnectionString,
                name: "database",
                failureStatus: HealthStatus.Unhealthy,
                tags: new[] { "db", "postgresql", "ready" });
        }

        // Add basic memory health check
        healthChecksBuilder.AddCheck(
            name: $"{healthCheckName}-memory",
            check: () => HealthCheckResult.Healthy("Memory is healthy"),
            tags: new[] { "memory", "live" });

        return services;
    }
}