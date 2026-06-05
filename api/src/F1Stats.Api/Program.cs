using F1Stats.Api.Endpoints;
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

app.Run();