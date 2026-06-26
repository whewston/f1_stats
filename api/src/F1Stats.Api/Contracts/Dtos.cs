namespace F1Stats.Api.Contracts;
using System.Text.Json;

public record RaceSummaryDto(
    int Round, string RaceName, DateOnly Date, TimeOnly? Time,
    string CircuitName, string? Country, string? Locality);

public record ResultRowDto(
    int? Position, string PositionText, string DriverId, string Driver, string? Code,
    string ConstructorId, string Constructor, int? Grid, decimal Points, int Laps,
    string Status, string? Time, string? FastestLap, string? Nationality);

public record RaceResultsDto(
    int Season, int Round, string RaceName, DateOnly Date, string CircuitName,
    string? Country, IReadOnlyList<ResultRowDto> Results);

public record NextRaceDto(
    int Season, int Round, string RaceName, DateOnly Date, TimeOnly? Time,
    string CircuitName, string? Country, string? Locality);

public record DriverStandingDto(
    int Position, string DriverId, string Driver, string? Code,
    string? ConstructorId, string? Constructor, decimal Points, int Wins, string? Nationality);

public record ConstructorStandingDto(
    int Position, string ConstructorId, string Constructor, string? Nationality, decimal Points, int Wins);
    
public record StatTotals(int Races, int Wins, int Podiums, int Poles, decimal Points, int Championships, int? BestFinish);

public record DriverSeasonStat(int Year, string? Team, int StandingPosition, decimal Points, int Wins, int Podiums, int Races, int? BestFinish);
public record DriverProfileDto(string DriverId, string Name, string? Code, string? Nationality, StatTotals AllTime, IReadOnlyList<DriverSeasonStat> Seasons);

public record ConstructorSeasonStat(int Year, int StandingPosition, decimal Points, int Wins, int Podiums, int Races, int? BestFinish);
public record ConstructorProfileDto(string ConstructorId, string Name, string? Nationality, StatTotals AllTime, IReadOnlyList<ConstructorSeasonStat> Seasons);
public record CircuitWinDto(string DriverId, string Driver, int Wins);
public record PastEditionDto(int Year, int Round, string? WinnerDriverId, string? Winner, string? WinnerConstructor);
public record RacePreviewDto(
    int Year, int Round, string RaceName, DateOnly Date, TimeOnly? Time,
    string CircuitId, string CircuitName, string? Country, string? Locality,
    double? Latitude, double? Longitude,
    IReadOnlyList<CircuitWinDto> TopWinners,
    IReadOnlyList<PastEditionDto> PastEditions,
    int? LastEditionYear, int? LastEditionRound);

// --- Prediction read side ---
public record PredictionRowDto(
    int PredictedPosition, string DriverId, string Driver, string? Code,
    string? Nationality, string? ConstructorId, string? Constructor,
    double? WinProbability, IReadOnlyList<string> Reasons);

// --- Prediction submit side (request body) ---
public record PredictionInputDto(
    string DriverId, int PredictedPosition, double? WinProbability,
    IReadOnlyList<string>? Reasons);
public record PredictionSubmissionDto(string ModelVersion, IReadOnlyList<PredictionInputDto> Predictions);

public record RacePredictionDto(
    int Year, int Round, string ModelVersion, DateTime GeneratedAt,
    IReadOnlyList<PredictionRowDto> Rows);