using Prometheus;

namespace ApiGateway.Composition;

public static class ObservabilityConfiguration
{
    public static WebApplication ConfigureObservability(this WebApplication app)
    {
        app.UseMetricServer();
        app.UseHttpMetrics(options =>
        {
            options.RequestCount.Enabled = true;
            options.RequestDuration.Enabled = true;
        });
        
        return app;
    }
}

