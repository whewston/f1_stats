# F1 Stats

A Formula 1 statistics system: a self-hosted API plus a web/app front end that lets
users look up a race by year and see the results, with the next race weekend
highlighted.

## Why this exists

The public F1 data API everyone used (Ergast) was shut down at the start of 2025.
Its drop-in successor is **Jolpica-F1** (`api.jolpi.ca`), a free, volunteer-run,
open-source project. Rather than hit Jolpica directly from the front end on every
request, this project ingests from Jolpica on a schedule into its own database and
serves its own API. That gives us caching, a stable schema we control, resilience if
Jolpica is down or rate-limited, and a place to compute our own value-added fields
(like "next race").

## Project layout

```
f1-stats/
├── README.md              <- you are here
└── docs/
    ├── requirements.md     <- functional + non-functional requirements
    ├── roadmap.md          <- phased plan (API → web/app → extras)
    ├── architecture.md     <- data source, data model, ingestion, tech stack
    └── decisions/          <- Architecture Decision Records (ADRs)
        ├── 0001-data-source.md
        ├── 0002-api-framework.md
        └── 0003-app-approach.md
```

## Current status

Planning. No code yet — the next step is scaffolding the Phase 1 API once the
framework decision (ADR 0002) is confirmed.

## Quick links

- [Requirements](docs/requirements.md)
- [Roadmap](docs/roadmap.md)
- [Architecture](docs/architecture.md)
- [Decisions](docs/decisions/)
