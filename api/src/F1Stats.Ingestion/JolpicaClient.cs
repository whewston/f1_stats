using System.Net.Http.Json;
using F1Stats.Ingestion.Models;

namespace F1Stats.Ingestion;

// Typed client. BaseAddress + resilience are configured at registration.
public class JolpicaClient(HttpClient http)
{
    // Full season schedule, including rounds that haven't happened yet.
    public async Task<IReadOnlyList<JolpicaRace>> GetSeasonScheduleAsync(int year, CancellationToken ct = default)
    {
        var data = await http.GetFromJsonAsync<JolpicaResponse>($"{year}/races.json?limit=100", ct);
        return data?.MRData.RaceTable.Races ?? [];
    }

    // Results for a single round (one page comfortably covers a ~20-car field).
    public async Task<JolpicaRace?> GetRaceResultsAsync(int year, int round, CancellationToken ct = default)
    {
        var data = await http.GetFromJsonAsync<JolpicaResponse>($"{year}/{round}/results.json?limit=100", ct);
        return data?.MRData.RaceTable.Races.FirstOrDefault();
    }
}