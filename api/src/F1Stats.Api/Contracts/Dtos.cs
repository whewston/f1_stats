namespace F1Stats.Api.Contracts;

public record RaceSummaryDto(
    int Round, string RaceName, DateOnly Date, TimeOnly? Time,
    string CircuitName, string? Country, string? Locality);

public record ResultRowDto(
    int? Position, string PositionText, string Driver, string? Code,
    string Constructor, int? Grid, decimal Points, int Laps,
    string Status, string? Time, string? FastestLap);

public record RaceResultsDto(
    int Season, int Round, string RaceName, DateOnly Date, string CircuitName,
    IReadOnlyList<ResultRowDto> Results);

public record NextRaceDto(
    int Season, int Round, string RaceName, DateOnly Date, TimeOnly? Time,
    string CircuitName, string? Country, string? Locality);