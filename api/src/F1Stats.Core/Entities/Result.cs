namespace F1Stats.Core.Entities;

public class Result
{
    public int Id { get; set; }                         // surrogate PK
    public int RaceId { get; set; }                     // FK   \ natural
    public string DriverId { get; set; } = default!;    // FK   / key
    public string ConstructorId { get; set; } = default!;  // FK

    public int? Grid { get; set; }                      // start position
    public int? Position { get; set; }                  // finish position (null if DNF)
    public string PositionText { get; set; } = default!; // "1", "R", "D"...
    public decimal Points { get; set; }
    public string Status { get; set; } = default!;      // "Finished", "+1 Lap", "Engine"...
    public int Laps { get; set; }
    public string? FastestLapTime { get; set; }
    public string? RaceTime { get; set; }               // total race time string

    public Race Race { get; set; } = default!;
    public Driver Driver { get; set; } = default!;
    public Constructor Constructor { get; set; } = default!;
}