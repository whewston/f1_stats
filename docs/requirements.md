# Requirements

Status legend: ✅ agreed · 🟡 needs a decision · 🔭 future / out of MVP scope

## Functional requirements

### Phase 1 — API (current focus)

| # | Requirement | Status | Notes |
|---|-------------|--------|-------|
| F1 | Ingest F1 data from Jolpica-F1 into our own database | ✅ | See architecture.md. Source of truth is Jolpica. |
| F2 | Ingestion runs on a schedule (target: weekly) | ✅ | Races are on weekends, so weekly is the natural cadence. Make it idempotent and also manually triggerable. |
| F3 | Expose seasons (years) available | ✅ | `GET /api/seasons` |
| F4 | Expose races for a given year | ✅ | `GET /api/seasons/{year}/races` |
| F5 | Expose results for a given race | ✅ | `GET /api/seasons/{year}/races/{round}/results` — this is the core "search a race and a year, see results" feature. |
| F6 | Expose the next upcoming race | ✅ | `GET /api/races/next` — backs the "highlight next race" feature. |
| F7 | Define what "results" includes | 🟡 | Race finishing order at minimum. Decide whether MVP also covers qualifying, sprint, and championship standings, or whether those come later. Recommendation: race results only for MVP. |
| F8 | Searching by race | 🟡 | Decide the lookup key. Recommendation: `(year, round)` is the clean canonical key; allow looking up the round list by `(year)` first, and optionally search by circuit/Grand Prix name as a convenience. |

### Phase 2 — Web + App

| # | Requirement | Status | Notes |
|---|-------------|--------|-------|
| F9 | Web UI to search by year + race and view results | ✅ | Consumes the Phase 1 API. |
| F10 | Same functionality available on mobile (iOS + Android) | ✅ | Approach is an open decision — see ADR 0003. |
| F11 | Highlight / surface the next race weekend in the UI | ✅ | Uses F6. Could be a banner, countdown, or session schedule. |

### Phase 3 — Future / nice-to-have

| # | Requirement | Status | Notes |
|---|-------------|--------|-------|
| F12 | Championship standings (drivers + constructors) | 🔭 | Jolpica provides these. |
| F13 | Qualifying and sprint results | 🔭 | |
| F14 | Per-race detail: grid vs finish, pit stops, fastest lap | 🔭 | |
| F15 | Push notification before a race / session starts | 🔭 | This is the main feature that would justify a true native app over a web app. |
| F16 | Driver and constructor profile pages with historical stats | 🔭 | |

## Non-functional requirements

| # | Requirement | Notes |
|---|-------------|-------|
| N1 | Be a courteous consumer of Jolpica | It is volunteer-run (~$45/month hosting). Respect its rate limits, add delays between calls, cache aggressively, and never proxy raw front-end traffic straight through to it. Our own cached API is partly *because* of this. |
| N2 | Cheap to host | Read-heavy, tiny write volume. SQLite or a small Postgres instance is plenty. |
| N3 | API documented | Ship an OpenAPI/Swagger spec. |
| N4 | Ingestion is idempotent | Re-running it must not duplicate rows. Upsert on natural keys (year+round, driverId, etc.). |
| N5 | Timezone-correct schedule data | "Next race" and session times must handle the user's timezone, not just UTC. |

## Commentary on your self-set requirements

**"Self-made API, only updated weekly."** Good instinct, and worth being explicit
about *why* it's the right call: it is really a caching/aggregation layer over
Jolpica. That buys you rate-limit protection, a schema you own, resilience, and a
home for computed fields like "next race." Two refinements: (1) also expose a manual
trigger so you're not waiting a week to fix bad data, and (2) consider making
ingestion *event-driven* — check whether a new completed round exists and only then
write — rather than blindly overwriting weekly.

**"Search a race and a year, see results."** Clean MVP. The main thing to pin down is
what "results" means (F7) — scope it to race finishing order first or it balloons.

**"Highlight the next race week."** This is a great low-cost, high-impact feature, and
it's the natural seed for the one genuinely app-worthy feature later: a push
notification before lights-out (F15).
