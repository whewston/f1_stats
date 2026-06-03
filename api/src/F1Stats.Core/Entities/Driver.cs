namespace F1Stats.Core.Entities;

public class Driver
{
    public string DriverId { get; set; } = default!;    // PK, e.g. "max_verstappen"
    public string? Code { get; set; }                    // e.g. "VER"
    public string GivenName { get; set; } = default!;
    public string FamilyName { get; set; } = default!;
    public string? Nationality { get; set; }

    public ICollection<Result> Results { get; set; } = new List<Result>();
}