# Roadmap

Three phases, each shippable on its own. Don't start a phase until the previous one
actually works end to end.

## Phase 1 — The API (build this first)

The goal: a running API, fed weekly from Jolpica, that can answer "what were the
results of round N in year Y" and "what's the next race."

1. Pick the framework and confirm ADR 0002.
2. Define the database schema (start small — see architecture.md data model).
3. Build the Jolpica client: typed HTTP calls, retries, rate-limit-friendly delays.
4. Build the ingestion job: fetch → map → upsert. Make it idempotent and manually
   runnable.
5. Build the read endpoints (F3–F6).
6. Add OpenAPI/Swagger docs.
7. Schedule the ingestion (cron / hosted scheduled task / CI cron).
8. Deploy somewhere cheap.

**Done when:** you can hit a deployed URL, ask for a year + round, and get correct
results back, and the next-race endpoint is accurate.

## Phase 2 — Web + App

The goal: a front end people actually use, on web and mobile.

1. Build a responsive web app against the Phase 1 API: year picker → race list →
   results table.
2. Add the "next race" highlight (banner + countdown).
3. Make it a PWA so it's installable on phones (this may be all the "app" you need —
   see ADR 0003).
4. Only if you want app-store presence / push: build a native-ish client (React
   Native + Expo, or .NET MAUI) sharing the same API.

**Done when:** a phone user can search and read results comfortably, and sees the
next race surfaced.

## Phase 3 — Extras

Pick from the future requirements (F12–F16) based on what you find fun or useful.
Standings and qualifying are easy wins (the data's already in Jolpica). Push
notifications for race start is the feature that genuinely justifies a native app.

## Optional interview-prep track (Ruby)

Separate from shipping the product. If you want Ruby practice for the Softwire
interview, the cleanest option is to build the **same Phase 1 API in Rails (API
mode)** as a parallel exercise. The spec is small and well-defined, so a side-by-side
"same API, two stacks" makes a tidy comparison and forces you through real Ruby/Rails
idioms (Active Record, migrations, routing) rather than toy katas. Keep it in a
separate repo/folder so it doesn't entangle the .NET build you actually maintain.
