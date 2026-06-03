using F1Stats.Core;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddDbContext<F1DbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/health", () => Results.Ok(new { status = "ok" }))
    .WithName("Health");

// Real query now — returns [] until ingestion populates the DB.
app.MapGet("/api/seasons", async (F1DbContext db) =>
        await db.Seasons.OrderByDescending(s => s.Year)
            .Select(s => s.Year)
            .ToListAsync())
    .WithName("GetSeasons");

app.Run();