namespace F1Stats.Core.Entities;

public class QualifyingResult
{
    public int Id { get; set; }
    public int RaceId { get; set; }                        // FK -> Race      \
    public string DriverId { get; set; } = default!;       // FK -> Driver    / natural key
    public string ConstructorId { get; set; } = default!;  // FK -> Constructor
    public int Position { get; set; }                      // qualifying classification (P1 = pole)
    public string? Q1 { get; set; }
    public string? Q2 { get; set; }
    public string? Q3 { get; set; }

    public Race Race { get; set; } = default!;
    public Driver Driver { get; set; } = default!;
    public Constructor Constructor { get; set; } = default!;
}