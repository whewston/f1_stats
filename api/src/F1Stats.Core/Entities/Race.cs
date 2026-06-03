namespace F1Stats.Core.Entities;

public class Race
{
    public int Id { get; set; }                         // surrogate PK
    public int Year { get; set; }                       // FK -> Season   \ natural
    public int Round { get; set; }                      //                 / key
    public string RaceName { get; set; } = default!;
    public DateOnly Date { get; set; }                  // race day
    public TimeOnly? Time { get; set; }                 // race start (UTC), if known

    public string CircuitId { get; set; } = default!;   // FK -> Circuit

    public Season Season { get; set; } = default!;
    public Circuit Circuit { get; set; } = default!;
    public ICollection<Result> Results { get; set; } = new List<Result>();
}