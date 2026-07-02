using F1Stats.Api.Endpoints;
using F1Stats.Core;
using F1Stats.Ingestion;
using Microsoft.EntityFrameworkCore;
using F1Stats.Api.Services;
using F1Stats.Api.Contracts;
using F1Stats.Core.Entities;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddDbContext<F1DbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));
builder.Services.AddJolpicaClient();
builder.Services.AddScoped<IngestionService>();
builder.Services.AddScoped<StatsService>(); 

builder.Services.AddCors(o => o.AddPolicy("frontend", p => p
    .WithOrigins("https://f1stats.whewston.co.uk", "http://localhost:5173")
    .AllowAnyHeader()
    .AllowAnyMethod()));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<F1DbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("frontend");

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

    var allowedPhases = new[] { "pre_qualifying", "post_qualifying" };
    if (string.IsNullOrEmpty(body.Phase) || !allowedPhases.Contains(body.Phase))
        return Results.BadRequest(new { error = "Phase must be 'pre_qualifying' or 'post_qualifying'." });

    if (body.Predictions is null || body.Predictions.Count == 0)
        return Results.BadRequest(new { error = "No predictions supplied." });

    var submittedIds = body.Predictions.Select(p => p.DriverId).Distinct().ToList();
    var knownIds = await db.Drivers
        .Where(d => submittedIds.Contains(d.DriverId))
        .Select(d => d.DriverId).ToListAsync(ct);
    var unknown = submittedIds.Except(knownIds).ToList();
    if (unknown.Count > 0)
        return Results.BadRequest(new { error = "Unknown driverIds.", unknown });

    var existing = await db.Predictions
        .Where(p => p.Year == year && p.Round == round && p.Phase == body.Phase)
        .ToListAsync(ct);
    db.Predictions.RemoveRange(existing);

    var now = DateTime.UtcNow;
    foreach (var p in body.Predictions)
        db.Predictions.Add(new Prediction
        {
            Year = year, Round = round, DriverId = p.DriverId, Phase = body.Phase,
            PredictedPosition = p.PredictedPosition, WinProbability = p.WinProbability,
            Reasons = p.Reasons is { Count: > 0 } ? JsonSerializer.Serialize(p.Reasons) : null,
            ModelVersion = body.ModelVersion, GeneratedAt = now,
        });

    await db.SaveChangesAsync(ct);
    return Results.Ok(new { year, round, phase = body.Phase, count = body.Predictions.Count, modelVersion = body.ModelVersion });
}).WithName("SubmitPredictions");

app.Run();