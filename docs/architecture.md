# Architecture

## Shape of the system

```
   Jolpica-F1 API                Your system
   (api.jolpi.ca)        ┌──────────────────────────────┐
        │                │                              │
        │  weekly fetch  │   ┌────────────┐             │
        └───────────────────▶│ Ingestion  │             │
                         │   │   job      │             │
                         │   └─────┬──────┘             │
                         │         │ upsert             │
                         │   ┌─────▼──────┐             │
                         │   │  Database  │             │
                         │   │ (your own) │             │
                         │   └─────┬──────┘             │
                         │         │ read               │
                         │   ┌─────▼──────┐             │
   Web / mobile  ◀───────────│  Read API  │             │
   front end      requests │ └────────────┘             │
                         └──────────────────────────────┘
```

The front end never talks to Jolpica directly. It only talks to your API, which is
served from your own database. Jolpica is touched only by the scheduled ingestion job.

## Data source: Jolpica-F1

- Base URL: `https://api.jolpi.ca/ergast/f1/`
- Ergast-compatible JSON. Existing Ergast docs largely still apply.
- Free, open source, volunteer-run, rate-limited. **Be polite**: cache, throttle,
  and don't hammer it. No more database dumps are published, so the API is the way in.
- Alternatives if Jolpica ever isn't enough: OpenF1 (real-time/telemetry, different
  use case) or Hyprace (commercial, production-grade). Not needed for this project.

## Data model (MVP)

Start with the minimum to answer "results for year + round" and "next race." Mirror
Jolpica's natural keys so ingestion upserts cleanly.

```
Season        (year PK)
Circuit       (circuit_id PK, name, locality, country, lat, long)
Race          (id PK, year FK, round, name, date, time, circuit_id FK)
              -- natural key: (year, round)
Driver        (driver_id PK, code, given_name, family_name, nationality)
Constructor   (constructor_id PK, name, nationality)
Result        (id PK, race_id FK, driver_id FK, constructor_id FK,
               grid, position, position_text, points, status, laps,
               fastest_lap_time, race_time)
              -- natural key: (race_id, driver_id)
```

Later additions (Phase 3): `QualifyingResult`, `SprintResult`, `DriverStanding`,
`ConstructorStanding`, `PitStop`, `Lap`.

## API surface (MVP)

| Method | Path | Returns |
|--------|------|---------|
| GET | `/api/seasons` | list of years available |
| GET | `/api/seasons/{year}/races` | races (round, name, date, circuit) for that year |
| GET | `/api/seasons/{year}/races/{round}/results` | finishing order for that race |
| GET | `/api/races/next` | the next upcoming race + (later) session times |

## Ingestion

- Trigger: scheduled weekly (a hosted cron, a platform scheduled task, or a CI cron
  job hitting a protected admin endpoint). Also expose a manual run.
- Flow: fetch from Jolpica → map JSON to entities → **upsert** on natural keys.
- Idempotent: re-running never duplicates. Safe to re-run after a failure.
- Throttled: short delays between paged calls; honour rate limits; retry with backoff.
- Smart cadence (optional): detect whether a new completed round exists before writing,
  rather than blindly overwriting everything weekly.

## Tech stack (recommended — see ADR 0002)

- **API:** ASP.NET Core Web API (.NET 9), since you're strongest in .NET and the
  domain is relational and read-heavy.
- **ORM:** EF Core. SQLite for dev (and fine for prod given the tiny write volume);
  PostgreSQL if you want a managed prod DB.
- **HTTP client:** typed `HttpClient` for Jolpica, with Polly for retry/backoff.
- **Ingestion:** a `BackgroundService`/hosted worker, or a separate console app run on
  a schedule. Keep it decoupled from the read path.
- **Docs:** OpenAPI/Swagger.

A Rails (API mode) implementation is entirely viable too and is the suggested
Ruby-practice track in the roadmap; the data model and endpoints above are
framework-agnostic.
