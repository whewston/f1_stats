using System.Text.Json.Serialization;

namespace F1Stats.Ingestion.Models;

// The envelope every Jolpica response shares: { "MRData": { ... } }
public class JolpicaResponse
{
    [JsonPropertyName("MRData")] public MRData MRData { get; set; } = new();
}

public class MRData
{
    [JsonPropertyName("limit")] public string Limit { get; set; } = "";
    [JsonPropertyName("offset")] public string Offset { get; set; } = "";
    [JsonPropertyName("total")] public string Total { get; set; } = "";
    [JsonPropertyName("RaceTable")] public RaceTable RaceTable { get; set; } = new();
}

public class RaceTable
{
    [JsonPropertyName("Races")] public List<JolpicaRace> Races { get; set; } = new();
}

public class JolpicaRace
{
    [JsonPropertyName("season")] public string Season { get; set; } = "";
    [JsonPropertyName("round")] public string Round { get; set; } = "";
    [JsonPropertyName("raceName")] public string RaceName { get; set; } = "";
    [JsonPropertyName("date")] public string Date { get; set; } = "";
    [JsonPropertyName("time")] public string? Time { get; set; }
    [JsonPropertyName("Circuit")] public JolpicaCircuit Circuit { get; set; } = new();
    [JsonPropertyName("Results")] public List<JolpicaResult>? Results { get; set; }
    [JsonPropertyName("QualifyingResults")] public List<JolpicaQualifyingResult>? QualifyingResults { get; set; }
}

public class JolpicaCircuit
{
    [JsonPropertyName("circuitId")] public string CircuitId { get; set; } = "";
    [JsonPropertyName("circuitName")] public string CircuitName { get; set; } = "";
    [JsonPropertyName("Location")] public JolpicaLocation? Location { get; set; }
}

public class JolpicaLocation
{
    [JsonPropertyName("lat")] public string? Lat { get; set; }
    [JsonPropertyName("long")] public string? Long { get; set; }   // "long" is a C# keyword, hence the rename
    [JsonPropertyName("locality")] public string? Locality { get; set; }
    [JsonPropertyName("country")] public string? Country { get; set; }
}

public class JolpicaResult
{
    [JsonPropertyName("position")] public string? Position { get; set; }
    [JsonPropertyName("positionText")] public string PositionText { get; set; } = "";
    [JsonPropertyName("points")] public string Points { get; set; } = "0";
    [JsonPropertyName("grid")] public string? Grid { get; set; }
    [JsonPropertyName("laps")] public string? Laps { get; set; }
    [JsonPropertyName("status")] public string Status { get; set; } = "";
    [JsonPropertyName("Driver")] public JolpicaDriver Driver { get; set; } = new();
    [JsonPropertyName("Constructor")] public JolpicaConstructor Constructor { get; set; } = new();
    [JsonPropertyName("Time")] public JolpicaTime? Time { get; set; }
    [JsonPropertyName("FastestLap")] public JolpicaFastestLap? FastestLap { get; set; }
}

public class JolpicaQualifyingResult
{
    [JsonPropertyName("position")] public string? Position { get; set; }
    [JsonPropertyName("Q1")] public string? Q1 { get; set; }
    [JsonPropertyName("Q2")] public string? Q2 { get; set; }
    [JsonPropertyName("Q3")] public string? Q3 { get; set; }
    [JsonPropertyName("Driver")] public JolpicaDriver Driver { get; set; } = new();
    [JsonPropertyName("Constructor")] public JolpicaConstructor Constructor { get; set; } = new();
}

public class JolpicaDriver
{
    [JsonPropertyName("driverId")] public string DriverId { get; set; } = "";
    [JsonPropertyName("code")] public string? Code { get; set; }
    [JsonPropertyName("givenName")] public string GivenName { get; set; } = "";
    [JsonPropertyName("familyName")] public string FamilyName { get; set; } = "";
    [JsonPropertyName("nationality")] public string? Nationality { get; set; }
    [JsonPropertyName("ConstructorStandings")] public List<JolpicaConstructorStanding> ConstructorStandings { get; set; } = new();
}

public class JolpicaConstructor
{
    [JsonPropertyName("constructorId")] public string ConstructorId { get; set; } = "";
    [JsonPropertyName("name")] public string Name { get; set; } = "";
    [JsonPropertyName("nationality")] public string? Nationality { get; set; }
}

public class JolpicaTime
{
    [JsonPropertyName("time")] public string? Time { get; set; }
}

public class JolpicaFastestLap
{
    [JsonPropertyName("Time")] public JolpicaTime? Time { get; set; }
}

public class StandingsResponse
{
    [JsonPropertyName("MRData")] public StandingsMRData MRData { get; set; } = new();
}
public class StandingsMRData
{
    [JsonPropertyName("StandingsTable")] public StandingsTable StandingsTable { get; set; } = new();
}
public class StandingsTable
{
    [JsonPropertyName("StandingsLists")] public List<StandingsList> StandingsLists { get; set; } = new();
}
public class StandingsList
{
    [JsonPropertyName("DriverStandings")] public List<JolpicaDriverStanding> DriverStandings { get; set; } = new();
    [JsonPropertyName("ConstructorStandings")] public List<JolpicaConstructorStanding> ConstructorStandings { get; set; } = new();
}
public class JolpicaDriverStanding
{
    [JsonPropertyName("position")] public string? Position { get; set; }
    [JsonPropertyName("points")] public string Points { get; set; } = "0";
    [JsonPropertyName("wins")] public string Wins { get; set; } = "0";
    [JsonPropertyName("Driver")] public JolpicaDriver Driver { get; set; } = new();
    [JsonPropertyName("Constructors")] public List<JolpicaConstructor> Constructors { get; set; } = new();
}

public class JolpicaConstructorStanding
{
    [JsonPropertyName("position")] public string? Position { get; set; }
    [JsonPropertyName("points")] public string Points { get; set; } = "0";
    [JsonPropertyName("wins")] public string Wins { get; set; } = "0";
    [JsonPropertyName("Constructor")] public JolpicaConstructor Constructor { get; set; } = new();
}