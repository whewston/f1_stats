namespace F1Stats.Core.Entities;

public class Circuit
{
    public string CircuitId { get; set; } = default!;   // PK, e.g. "monza"
    public string Name { get; set; } = default!;
    public string? Locality { get; set; }
    public string? Country { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    public ICollection<Race> Races { get; set; } = new List<Race>();
}