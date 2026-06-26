namespace F1Stats.Core.Entities;

public class Prediction
{
    public int Id { get; set; }
    public int Year { get; set; }                     // FK -> Season   \
    public int Round { get; set; }                    //                 } natural key
    public string DriverId { get; set; } = default!;  // FK -> Driver   /
    public int PredictedPosition { get; set; }
    public double? WinProbability { get; set; }       // optional, 0..1
    public string ModelVersion { get; set; } = default!;
    public DateTime GeneratedAt { get; set; }

    public Driver Driver { get; set; } = default!;
}