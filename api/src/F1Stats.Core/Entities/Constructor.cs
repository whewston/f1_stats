namespace F1Stats.Core.Entities;

public class Constructor
{
    public string ConstructorId { get; set; } = default!;  // PK, e.g. "red_bull"
    public string Name { get; set; } = default!;
    public string? Nationality { get; set; }

    public ICollection<Result> Results { get; set; } = new List<Result>();
}