namespace F1Stats.Core.Entities;

public class Season
{
    public int Year { get; set; }                       // PK, e.g. 2024
    public ICollection<Race> Races { get; set; } = new List<Race>();
}