using Microsoft.Extensions.DependencyInjection;

namespace F1Stats.Ingestion;

public static class JolpicaServiceCollectionExtensions
{
    public static IServiceCollection AddJolpicaClient(this IServiceCollection services)
    {
        services.AddHttpClient<JolpicaClient>(http =>
            {
                http.BaseAddress = new Uri("https://api.jolpi.ca/ergast/f1/");
                // A descriptive UA lets the maintainers identify our traffic — good etiquette.
                http.DefaultRequestHeaders.UserAgent.ParseAdd("f1-stats/0.1 (github.com/whewston/f1_stats)");
                http.Timeout = TimeSpan.FromSeconds(30);
            })
            .AddStandardResilienceHandler();

        return services;
    }
}