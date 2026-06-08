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

public record DriverStandingDto(
    int Position, string Driver, string? Code, string? Constructor, decimal Points, int Wins);
    
public record StatTotals(int Races, int Wins, int Podiums, int Poles, decimal Points, int Championships, int? BestFinish);

public record DriverSeasonStat(int Year, string? Team, int StandingPosition, decimal Points, int Wins, int Podiums, int Races, int? BestFinish);
public record DriverProfileDto(string DriverId, string Name, string? Code, string? Nationality, StatTotals AllTime, IReadOnlyList<DriverSeasonStat> Seasons);

public record ConstructorSeasonStat(int Year, int StandingPosition, decimal Points, int Wins, int Podiums, int Races, int? BestFinish);
public record ConstructorProfileDto(string ConstructorId, string Name, string? Nationality, StatTotals AllTime, IReadOnlyList<ConstructorSeasonStat> Seasons);