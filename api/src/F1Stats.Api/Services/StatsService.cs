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
}