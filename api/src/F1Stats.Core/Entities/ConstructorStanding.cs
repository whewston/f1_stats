namespace F1Stats.Core.Entities;

public class ConstructorStanding
{
    public int Id { get; set; }
    public int Year { get; set; }                      // FK -> Season         \ natural
    public string ConstructorId { get; set; } = default!;  // FK -> Constructor / key
    public int Position { get; set; }
    public decimal Points { get; set; }
    public int Wins { get; set; }

    public Season Season { get; set; } = default!;
    public Constructor Constructor { get; set; } = default!;
}