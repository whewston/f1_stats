using System.Globalization;
using F1Stats.Core;
using F1Stats.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace F1Stats.Ingestion;

public record IngestionResult(int Year, int RacesUpserted, int RoundsIngested, int ResultsUpserted);

public class IngestionService(F1DbContext db, JolpicaClient jolpica, ILogger<IngestionService> logger)
{
    // Be a courteous Jolpica consumer: stay well under 4 req/s.
    private static readonly TimeSpan CallDelay = TimeSpan.FromMilliseconds(300);

    public async Task<IngestionResult> IngestSeasonAsync(int year, CancellationToken ct = default)
    {
        logger.LogInformation("Ingesting season {Year}", year);

        // --- Season ---
        if (!await db.Seasons.AnyAsync(s => s.Year == year, ct))
            db.Seasons.Add(new Season { Year = year });

        // --- Schedule -> circuits + races ---
        var schedule = await jolpica.GetSeasonScheduleAsync(year, ct);

        var circuits = await db.Circuits.ToDictionaryAsync(c => c.CircuitId, ct);
        var races = await db.Races.Where(r => r.Year == year).ToDictionaryAsync(r => r.Round, ct);

        var racesUpserted = 0;
        foreach (var jr in schedule)
        {
            var round = int.Parse(jr.Round, CultureInfo.InvariantCulture);

            if (!circuits.TryGetValue(jr.Circuit.CircuitId, out var circuit))
            {
                circuit = new Circuit { CircuitId = jr.Circuit.CircuitId };
                circuits[circuit.CircuitId] = circuit;
                db.Circuits.Add(circuit);
            }
            circuit.Name = jr.Circuit.CircuitName;
            circuit.Locality = jr.Circuit.Location?.Locality;
            circuit.Country = jr.Circuit.Location?.Country;
            circuit.Latitude = ParseDouble(jr.Circuit.Location?.Lat);
            circuit.Longitude = ParseDouble(jr.Circuit.Location?.Long);

            if (!races.TryGetValue(round, out var race))
            {
                race = new Race { Year = year, Round = round };
                races[round] = race;
                db.Races.Add(race);
            }
            race.RaceName = jr.RaceName;
            race.Date = ParseDate(jr.Date);
            race.Time = ParseTime(jr.Time);
            race.CircuitId = circuit.CircuitId;
            racesUpserted++;
        }

        await db.SaveChangesAsync(ct);   // races now have their generated Ids

        // --- Results, only for rounds that have already happened ---
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var pastRounds = races.Values.Where(r => r.Date <= today).OrderBy(r => r.Round).ToList();

        var drivers = await db.Drivers.ToDictionaryAsync(d => d.DriverId, ct);
        var constructors = await db.Constructors.ToDictionaryAsync(c => c.ConstructorId, ct);

        var roundsIngested = 0;
        var resultsUpserted = 0;

        foreach (var race in pastRounds)
        {
            var raceData = await jolpica.GetRaceResultsAsync(year, race.Round, ct);
            await Task.Delay(CallDelay, ct);

            var jResults = raceData?.Results;
            if (jResults is null || jResults.Count == 0)
                continue;   // round hasn't been classified yet

            var existing = await db.Results.Where(r => r.RaceId == race.Id)
                                           .ToDictionaryAsync(r => r.DriverId, ct);

            foreach (var jres in jResults)
            {
                var jd = jres.Driver;
                if (!drivers.TryGetValue(jd.DriverId, out var driver))
                {
                    driver = new Driver { DriverId = jd.DriverId };
                    drivers[driver.DriverId] = driver;
                    db.Drivers.Add(driver);
                }
                driver.Code = jd.Code;
                driver.GivenName = jd.GivenName;
                driver.FamilyName = jd.FamilyName;
                driver.Nationality = jd.Nationality;

                var jc = jres.Constructor;
                if (!constructors.TryGetValue(jc.ConstructorId, out var constructor))
                {
                    constructor = new Constructor { ConstructorId = jc.ConstructorId };
                    constructors[constructor.ConstructorId] = constructor;
                    db.Constructors.Add(constructor);
                }
                constructor.Name = jc.Name;
                constructor.Nationality = jc.Nationality;

                if (!existing.TryGetValue(jd.DriverId, out var result))
                {
                    result = new Result { RaceId = race.Id, DriverId = jd.DriverId };
                    db.Results.Add(result);
                }
                result.ConstructorId = jc.ConstructorId;
                result.Grid = ParseInt(jres.Grid);
                result.Position = ParseInt(jres.Position);
                result.PositionText = jres.PositionText;
                result.Points = ParseDecimal(jres.Points);
                result.Status = jres.Status;
                result.Laps = ParseInt(jres.Laps) ?? 0;
                result.FastestLapTime = jres.FastestLap?.Time?.Time;
                result.RaceTime = jres.Time?.Time;
                resultsUpserted++;
            }

            await db.SaveChangesAsync(ct);
            roundsIngested++;
            logger.LogInformation("Round {Round}: {Count} results", race.Round, jResults.Count);
        }

        logger.LogInformation("Season {Year} done: {Races} races, {Rounds} rounds, {Results} results",
            year, racesUpserted, roundsIngested, resultsUpserted);

        return new IngestionResult(year, racesUpserted, roundsIngested, resultsUpserted);
    }

    static int? ParseInt(string? s) =>
        int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v) ? v : null;

    static decimal ParseDecimal(string? s) =>
        decimal.TryParse(s, NumberStyles.Number, CultureInfo.InvariantCulture, out var v) ? v : 0m;

    static double? ParseDouble(string? s) =>
        double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var v) ? v : null;

    static DateOnly ParseDate(string s) =>
        DateOnly.ParseExact(s, "yyyy-MM-dd", CultureInfo.InvariantCulture);

    static TimeOnly? ParseTime(string? s)
    {
        if (string.IsNullOrWhiteSpace(s)) return null;
        s = s.TrimEnd('Z');                       // "15:00:00Z" -> "15:00:00"
        return TimeOnly.TryParse(s, CultureInfo.InvariantCulture, out var t) ? t : null;
    }
}