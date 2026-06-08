using F1Stats.Api.Contracts;
using F1Stats.Core;
using Microsoft.EntityFrameworkCore;
using F1Stats.Api.Services;

namespace F1Stats.Api.Endpoints;

public static class F1Endpoints
{
    public static RouteGroupBuilder MapF1Endpoints(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("/api");

        // F3 — seasons available
        api.MapGet("/seasons", async (F1DbContext db) =>
                await db.Seasons.OrderByDescending(s => s.Year).Select(s => s.Year).ToListAsync())
           .WithName("GetSeasons");

        // F4 — races for a season
        api.MapGet("/seasons/{year:int}/races", async (int year, F1DbContext db) =>
        {
            var races = await db.Races
                .Where(r => r.Year == year)
                .OrderBy(r => r.Round)
                .Select(r => new RaceSummaryDto(
                    r.Round, r.RaceName, r.Date, r.Time,
                    r.Circuit.Name, r.Circuit.Country, r.Circuit.Locality))
                .ToListAsync();

            return races.Count == 0 ? Results.NotFound() : Results.Ok(races);
        }).WithName("GetSeasonRaces");

        // F5 — results for one race (the core feature)
        api.MapGet("/seasons/{year:int}/races/{round:int}/results", async (int year, int round, F1DbContext db) =>
        {
            var race = await db.Races
                .Where(r => r.Year == year && r.Round == round)
                .Select(r => new RaceResultsDto(
                    r.Year, r.Round, r.RaceName, r.Date, r.Circuit.Name,
                    r.Results
                        .OrderBy(res => res.Position == null)   // classified finishers first
                        .ThenBy(res => res.Position)            // then by finishing position
                        .Select(res => new ResultRowDto(
                            res.Position,
                            res.PositionText,
                            res.Driver.GivenName + " " + res.Driver.FamilyName,
                            res.Driver.Code,
                            res.Constructor.Name,
                            res.Grid,
                            res.Points,
                            res.Laps,
                            res.Status,
                            res.RaceTime,
                            res.FastestLapTime))
                        .ToList()))
                .FirstOrDefaultAsync();

            return race is null ? Results.NotFound() : Results.Ok(race);
        }).WithName("GetRaceResults");

        // F6 — next upcoming race
        api.MapGet("/races/next", async (F1DbContext db) =>
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var next = await db.Races
                .Where(r => r.Date >= today)
                .OrderBy(r => r.Date).ThenBy(r => r.Round)
                .Select(r => new NextRaceDto(
                    r.Year, r.Round, r.RaceName, r.Date, r.Time,
                    r.Circuit.Name, r.Circuit.Country, r.Circuit.Locality))
                .FirstOrDefaultAsync();

            return next is null ? Results.NotFound() : Results.Ok(next);
        }).WithName("GetNextRace");
        
        // F12 — current drivers' standings
        api.MapGet("/seasons/{year:int}/standings/drivers", async (int year, F1DbContext db) =>
        {
            var standings = await db.DriverStandings
                .Where(s => s.Year == year)
                .OrderBy(s => s.Position)
                .Select(s => new DriverStandingDto(
                    s.Position,
                    s.Driver.GivenName + " " + s.Driver.FamilyName,
                    s.Driver.Code,
                    s.Constructor != null ? s.Constructor.Name : null,
                    s.Points,
                    s.Wins))
                .ToListAsync();

            return standings.Count == 0 ? Results.NotFound() : Results.Ok(standings);
        }).WithName("GetDriverStandings");
        
        // Driver profile + all-time + per-season
        api.MapGet("/drivers/{driverId}", async (string driverId, StatsService stats, CancellationToken ct) =>
        {
            var profile = await stats.GetDriverAsync(driverId, ct);
            return profile is null ? Results.NotFound() : Results.Ok(profile);
        }).WithName("GetDriver");

        // Constructor profile + all-time + per-season
        api.MapGet("/constructors/{constructorId}", async (string constructorId, StatsService stats, CancellationToken ct) =>
        {
            var profile = await stats.GetConstructorAsync(constructorId, ct);
            return profile is null ? Results.NotFound() : Results.Ok(profile);
        }).WithName("GetConstructor");

        return api;
    }
}