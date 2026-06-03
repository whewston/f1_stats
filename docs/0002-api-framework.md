# ADR 0002 — API framework

**Status:** Proposed (confirm before building Phase 1)

## Context

The API is a relational, read-heavy service with tiny, infrequent writes (a weekly
ingest). The developer is strongest in **.NET with EF Core** and is separately
learning **Ruby** for a Softwire interview, but has said Ruby would only be for
practice, not the tool they'd otherwise choose.

## Decision

Build the real, maintained API in **ASP.NET Core Web API + EF Core**.

Treat Ruby/Rails as a *separate, optional* learning track (see roadmap) rather than
the foundation of the product, so that learning friction doesn't slow down the thing
you actually want to ship and keep running.

## Rationale

- The domain is a textbook relational model (seasons → races → results → drivers /
  constructors). EF Core models this cleanly, with migrations and LINQ queries.
- Read-heavy with a single weekly write path — well within what either framework
  handles, so pick the one you're fastest and most confident in.
- You already like and know .NET/EF Core. For a project you intend to maintain weekly,
  building it in your strongest stack means it actually gets finished and stays
  healthy.
- Learning a new language *and* debugging a real, scheduled, deployed system at the
  same time multiplies friction. Decoupling the two is lower risk.

## Options considered

- **ASP.NET Core + EF Core** — strongest fit for the dev + the domain. **Chosen for
  the product.**
- **Ruby on Rails (API mode)** — genuinely capable of this exact app; Active Record
  suits the relational model well. Best used as the *interview-practice* build of the
  same spec, in a separate repo. The MVP spec is small enough that doing it in both
  stacks makes a tidy side-by-side.
- Node/Express, Python/FastAPI, etc. — fine in the abstract, but no reason to pick
  them over the dev's strongest stack here.

## Consequences

- The data model and endpoint contract in architecture.md are written to be
  framework-agnostic, so the optional Rails build can target the identical API.
- Revisit only if the Softwire role turns out to specifically want a Rails portfolio
  piece — in which case promote the Rails track from "practice" to "primary."
