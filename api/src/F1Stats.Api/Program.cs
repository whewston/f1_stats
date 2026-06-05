using F1Stats.Core;
using F1Stats.Ingestion;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddDbContext<F1DbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddJolpicaClient();
builder.Services.AddScoped<IngestionService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.MapGet("/dev/jolpica/{year:int}", async (int year, JolpicaClient jolpica, CancellationToken ct) =>
    {
        var races = await jolpica.GetSeasonScheduleAsync(year, ct);
        return Results.Ok(races.Select(r => new { round = r.Round, name = r.RaceName, date = r.Date, circuit = r.Circuit.CircuitName }));
    });
}

app.MapGet("/health", () => Results.Ok(new { status = "ok" }))
    .WithName("Health");

app.MapGet("/api/seasons", async (F1DbContext db) =>
        await db.Seasons.OrderByDescending(s => s.Year).Select(s => s.Year).ToListAsync())
    .WithName("GetSeasons");

// Protected manual trigger. Later a weekly GitHub Actions cron can POST this with the key.
app.MapPost("/admin/ingest/{year:int}", async (
        int year,
        HttpRequest request,
        IngestionService ingestion,
        IConfiguration config,
        CancellationToken ct) =>
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

app.Run();