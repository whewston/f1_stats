var builder = WebApplication.CreateBuilder(args);

// Built-in OpenAPI (serves /openapi/v1.json in Development).
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Liveness probe — used by Docker/compose healthchecks and hosting platforms.
app.MapGet("/health", () => Results.Ok(new { status = "ok" }))
   .WithName("Health");

// --- Stub endpoints (real data arrives once EF Core + Jolpica ingestion land) ---
// TODO Phase 1: replace with DB-backed queries. See docs/architecture.md.
app.MapGet("/api/seasons", () => new[] { 2023, 2024, 2025 })
   .WithName("GetSeasons");

app.Run();