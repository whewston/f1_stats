using F1Stats.Core;
using F1Stats.Ingestion;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddDbContext<F1DbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddJolpicaClient();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    // TEMPORARY: proves the client deserializes real data. Delete once ingestion lands.
    app.MapGet("/dev/jolpica/{year:int}", async (int year, JolpicaClient jolpica, CancellationToken ct) =>
    {
        var races = await jolpica.GetSeasonScheduleAsync(year, ct);
        return Results.Ok(races.Select(r => new
        {
            round = r.Round,
            name = r.RaceName,
            date = r.Date,
            circuit = r.Circuit.CircuitName
        }));
    });
}

app.MapGet("/health", () => Results.Ok(new { status = "ok" }))
    .WithName("Health");

app.MapGet("/api/seasons", async (F1DbContext db) =>
        await db.Seasons.OrderByDescending(s => s.Year).Select(s => s.Year).ToListAsync())
    .WithName("GetSeasons");

app.Run();