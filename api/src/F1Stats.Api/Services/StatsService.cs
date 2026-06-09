using F1Stats.Api.Contracts;
using F1Stats.Core;
using Microsoft.EntityFrameworkCore;

namespace F1Stats.Api.Services;

public class StatsService(F1DbContext db)
{
    public async Task<DriverProfileDto?> GetDriverAsync(string driverId, CancellationToken ct = default)
    {
        var driver = await db.Drivers.FirstOrDefaultAsync(d => d.DriverId == driverId, ct);
        if (driver is null) return null;

        var results = await db.Results
            .Where(r => r.DriverId == driverId)
            .Select(r => new { r.Race.Year, r.Position, r.Grid })
            .ToListAsync(ct);

        var standings = await db.DriverStandings
            .Where(s => s.DriverId == driverId)
            .Select(s => new { s.Year, s.Position, s.Points, s.Wins, Team = s.Constructor != null ? s.Constructor.Name : null })
            .ToListAsync(ct);

        var allTime = new StatTotals(
            Races: results.Count,
            Wins: standings.Sum(s => s.Wins),
            Podiums: results.Count(r => r.Position is >= 1 and <= 3),
            Poles: results.Count(r => r.Grid == 1),
            Points: standings.Sum(s => s.Points),
            Championships: standings.Count(s => s.Position == 1),
            BestFinish: results.Select(r => r.Position).Min());

        var seasons = standings.OrderByDescending(s => s.Year).Select(s =>
        {
            var sr = results.Where(r => r.Year == s.Year).ToList();
            return new DriverSeasonStat(s.Year, s.Team, s.Position, s.Points, s.Wins,
                sr.Count(r => r.Position is >= 1 and <= 3), sr.Count, sr.Select(r => r.Position).Min());
        }).ToList();

        return new DriverProfileDto(driver.DriverId, $"{driver.GivenName} {driver.FamilyName}",
            driver.Code, driver.Nationality, allTime, seasons);
    }

    public async Task<ConstructorProfileDto?> GetConstructorAsync(string constructorId, CancellationToken ct = default)
    {
        var constructor = await db.Constructors.FirstOrDefaultAsync(c => c.ConstructorId == constructorId, ct);
        if (constructor is null) return null;

        var results = await db.Results
            .Where(r => r.ConstructorId == constructorId)
            .Select(r => new { r.RaceId, r.Race.Year, r.Position, r.Grid })
            .ToListAsync(ct);

        var standings = await db.ConstructorStandings
            .Where(s => s.ConstructorId == constructorId)
            .Select(s => new { s.Year, s.Position, s.Points, s.Wins })
            .ToListAsync(ct);

        var allTime = new StatTotals(
            Races: results.Select(r => r.RaceId).Distinct().Count(),   // distinct events (two cars per race)
            Wins: standings.Sum(s => s.Wins),
            Podiums: results.Count(r => r.Position is >= 1 and <= 3),
            Poles: results.Count(r => r.Grid == 1),
            Points: standings.Sum(s => s.Points),
            Championships: standings.Count(s => s.Position == 1),
            BestFinish: results.Select(r => r.Position).Min());

        var seasons = standings.OrderByDescending(s => s.Year).Select(s =>
        {
            var sr = results.Where(r => r.Year == s.Year).ToList();
            return new ConstructorSeasonStat(s.Year, s.Position, s.Points, s.Wins,
                sr.Count(r => r.Position is >= 1 and <= 3), sr.Select(r => r.RaceId).Distinct().Count(),
                sr.Select(r => r.Position).Min());
        }).ToList();

        return new ConstructorProfileDto(constructor.ConstructorId, constructor.Name, constructor.Nationality, allTime, seasons);
    }
    
    public async Task<RacePreviewDto?> GetRacePreviewAsync(int year, int round, CancellationToken ct = default)
    {
        var race = await db.Races
            .Where(r => r.Year == year && r.Round == round)
            .Select(r => new { r.Year, r.Round, r.RaceName, r.Date, r.Time, r.CircuitId,
                CircuitName = r.Circuit.Name, r.Circuit.Country, r.Circuit.Locality })
            .FirstOrDefaultAsync(ct);
        if (race is null) return null;

        // every earlier race at this circuit (most recent first)
        var pastRaces = await db.Races
            .Where(r => r.CircuitId == race.CircuitId && r.Date < race.Date)
            .OrderByDescending(r => r.Date)
            .Select(r => new { r.Id, r.Year, r.Round })
            .ToListAsync(ct);
        var pastIds = pastRaces.Select(r => r.Id).ToList();

        var winners = await db.Results
            .Where(r => pastIds.Contains(r.RaceId) && r.Position == 1)
            .Select(r => new { r.RaceId, r.DriverId,
                Driver = r.Driver.GivenName + " " + r.Driver.FamilyName,
                Constructor = r.Constructor.Name })
            .ToListAsync(ct);
        var winnerByRace = winners.ToDictionary(w => w.RaceId);

        var pastEditions = pastRaces.Select(r =>
        {
            winnerByRace.TryGetValue(r.Id, out var w);
            return new PastEditionDto(r.Year, r.Round, w?.DriverId, w?.Driver, w?.Constructor);
        }).ToList();

        var topWinners = winners
            .GroupBy(w => new { w.DriverId, w.Driver })
            .Select(g => new CircuitWinDto(g.Key.DriverId, g.Key.Driver, g.Count()))
            .OrderByDescending(c => c.Wins).ThenBy(c => c.Driver)
            .Take(6).ToList();

        var last = pastRaces.FirstOrDefault();

        return new RacePreviewDto(
            race.Year, race.Round, race.RaceName, race.Date, race.Time,
            race.CircuitId, race.CircuitName, race.Country, race.Locality,
            topWinners, pastEditions, last?.Year, last?.Round);
    }
}