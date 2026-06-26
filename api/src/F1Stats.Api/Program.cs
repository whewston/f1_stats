using F1Stats.Api.Endpoints;
using F1Stats.Core;
using F1Stats.Ingestion;
using Microsoft.EntityFrameworkCore;
using F1Stats.Api.Services;
using F1Stats.Api.Contracts;
using F1Stats.Core.Entities;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddDbContext<F1DbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));
builder.Services.AddJolpicaClient();
builder.Services.AddScoped<IngestionService>();
builder.Services.AddScoped<StatsService>(); 

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/health", () => Results.Ok(new { status = "ok" })).WithName("Health");

app.MapF1Endpoints();   // F3–F6

// Protected manual trigger. Later, a weekly GitHub Actions cron POSTs this with the key.
app.MapPost("/admin/ingest/{year:int}", async (
        int year, HttpRequest request, IngestionService ingestion, IConfiguration config, CancellationToken ct) =>
    {
        var key = config["Admin:ApiKey"];
        if (string.IsNullOrEmpty(key)
            || !request.Headers.TryGetValue("X-Admin-Key", out var sent)
            || sent.ToString() != key)
        {
            return Results.Unauthorized();
        }
        var result = await ingestion.IngestSeasonAsync(year, ct);
        return Results.Ok(result);
    })
    .WithName("IngestSeason");


app.MapPost("/admin/predictions/{year:int}/{round:int}", async (
    int year, int round, PredictionSubmissionDto body,
    HttpRequest request, F1DbContext db, IConfiguration config, CancellationToken ct) =>
{
    var key = config["Admin:ApiKey"];
    if (string.IsNullOrEmpty(key)
        || !request.Headers.TryGetValue("X-Admin-Key", out var sent)
        || sent.ToString() != key)
    {
        return Results.Unauthorized();
    }

    if (body.Predictions is null || body.Predictions.Count == 0)
        return Results.BadRequest(new { error = "No predictions supplied." });

    var submittedIds = body.Predictions.Select(p => p.DriverId).Distinct().ToList();
    var knownIds = await db.Drivers
        .Where(d => submittedIds.Contains(d.DriverId))
        .Select(d => d.DriverId).ToListAsync(ct);
    var unknown = submittedIds.Except(knownIds).ToList();
    if (unknown.Count > 0)
        return Results.BadRequest(new { error = "Unknown driverIds.", unknown });

    var existing = await db.Predictions.Where(p => p.Year == year && p.Round == round).ToListAsync(ct);
    db.Predictions.RemoveRange(existing);

    var now = DateTime.UtcNow;
    foreach (var p in body.Predictions)
        db.Predictions.Add(new Prediction
        {
            Year = year, Round = round, DriverId = p.DriverId,
            PredictedPosition = p.PredictedPosition, WinProbability = p.WinProbability,
            ModelVersion = body.ModelVersion, GeneratedAt = now,
        });

    await db.SaveChangesAsync(ct);
    return Results.Ok(new { year, round, count = body.Predictions.Count, modelVersion = body.ModelVersion });
}).WithName("SubmitPredictions");

app.Run();