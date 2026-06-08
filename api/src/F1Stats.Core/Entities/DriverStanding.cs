namespace F1Stats.Core.Entities;

public class DriverStanding
{
    public int Id { get; set; }
    public int Year { get; set; }                 // FK -> Season   \ natural
    public string DriverId { get; set; } = default!;  // FK -> Driver / key
    public string? ConstructorId { get; set; }    // FK -> Constructor (current team)
    public int Position { get; set; }
    public decimal Points { get; set; }
    public int Wins { get; set; }

    public Season Season { get; set; } = default!;
    public Driver Driver { get; set; } = default!;
    public Constructor? Constructor { get; set; }
}