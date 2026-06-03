# ADR 0001 — Data source: Jolpica-F1

**Status:** Accepted

## Context

The system needs historical and current F1 data (races, results, schedule). For a
decade the standard free source was the Ergast API, but Ergast was shut down at the
start of 2025. We need a current, reliable source.

## Decision

Use **Jolpica-F1** (`https://api.jolpi.ca/ergast/f1/`) as the upstream data source.
It is the drop-in, Ergast-compatible successor: free, open source, and actively
maintained. We ingest from it into our own database rather than calling it from the
front end.

## Options considered

- **Jolpica-F1** — free, Ergast-compatible, covers everything we need. Volunteer-run
  and rate-limited; no DB dumps anymore. **Chosen.**
- **OpenF1** — excellent for real-time timing/telemetry, but that's a different use
  case than historical results lookup. Not needed.
- **Hyprace** — commercial, production-grade, paid. Overkill for a personal project.
- **Scrape / hand-enter** — brittle and a lot of work. No.

## Consequences

- We must be a courteous consumer: cache, throttle, back off, and never proxy raw
  front-end requests to Jolpica. Our own cached API is partly a consequence of this.
- If Jolpica's availability ever becomes a problem, the ingestion layer is the only
  thing that would need to change to swap in another source — the rest of the system
  reads from our own DB.
