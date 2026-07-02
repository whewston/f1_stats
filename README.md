# F1 Stats

A self-hosted Formula 1 statistics system: an API fed from Jolpica-F1 into its own
Postgres database, a React web app to browse seasons, races, results, qualifying,
standings and circuit maps, and an **offline machine-learning predictor** that
forecasts upcoming races and is scored against what actually happened.

## Why this exists

The public F1 data API everyone used (Ergast) was shut down at the start of 2025.
Its drop-in successor is **Jolpica-F1** (`api.jolpi.ca`), a free, volunteer-run,
open-source project. Rather than hit Jolpica directly from the front end on every
request, this project ingests from Jolpica on a schedule into its own database and
serves its own API. That gives us caching, a stable schema we control, resilience if
Jolpica is down or rate-limited, and a home for computed fields (like "next race")
and our own prediction data.

## What's built

- **API (Phase 1):** ingestion (schedule, results, qualifying, standings) into
  Postgres; read endpoints for seasons, races, results, qualifying, standings, next
  race, driver/constructor profiles, and circuit history. OpenAPI/Swagger in Dev.
- **Web (Phase 2):** React/Vite/TypeScript dashboard; one unified race page that
  shows results for past races and a preview (last time out, who's won most) for
  upcoming ones; circuit track maps, country/nationality flags, driver & constructor
  pages.
- **Predictions (Phase 3):** an explainable Python predictor that runs *offline*,
  pulls features from the public API, ranks the field with reasons, and submits via a
  protected admin endpoint. Two phases (pre- and post-qualifying) and per-race
  accuracy scoring (winner, podium hits, mean error, Spearman ρ).

## Project layout
```
f1_stats/
├── docker-compose.yml          # orchestrates api + app + db
├── .env / .env.example         # POSTGRES_PASSWORD (real .env is gitignored)
├── api/                        # ASP.NET Core 10 (.NET) API
│   ├── Dockerfile
│   └── src/
│       ├── F1Stats.Core/       # entities, F1DbContext, EF Core migrations
│       ├── F1Stats.Ingestion/  # Jolpica client + IngestionService
│       └── F1Stats.Api/        # minimal API, endpoints, StatsService
├── app/                        # React + Vite + TypeScript frontend
│   ├── Dockerfile / nginx.conf
│   └── public/f1-circuits.geojson   # vendored MIT circuit geometry (track maps)
├── predictor/                  # offline Python race predictor
└── docs/                       # documentation (start with operations.md)

```

## Stack

.NET 10 · EF Core · PostgreSQL · React/Vite/TypeScript · Docker · Python (predictor)

## Quick start

Setup, ingestion, the race-weekend runbook, and deployment all live in
[docs/operations.md](docs/operations.md). Shortest path to a running system:

```powershell
docker compose up --build          # api :8080, app :3000, db :5432
# apply migrations, then backfill data — see operations.md
```

## Docs

- [Operations & onboarding](docs/operations.md) — setup, ingestion, predictions, deploy
- [Requirements](docs/requirements.md) · [Roadmap](docs/roadmap.md) · [Architecture](docs/architecture.md)
- [Decisions](docs/) — ADRs 0001–0003