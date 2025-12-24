using Prometheus;

namespace OrderService.Infrastructure.Observability;

public static class MetricsSetup
{
    public static WebApplication ConfigureMetrics(this WebApplication app)
    {
        app.UseMetricServer();
        app.UseHttpMetrics();
        return app;
    }
}

