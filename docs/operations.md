# Operations & onboarding

Everything needed to stand this up on a new machine, keep the data fresh each race
weekend, run the predictor, and eventually deploy.

## 1. Prerequisites

| Tool | Version | For |
|------|---------|-----|
| Docker Desktop | current | Postgres (and the full-stack compose) |
| .NET SDK | 10.x | the API + EF Core migrations |
| Node.js | 22.x | the frontend (Vite dev server + build) |
| Python | 3.11+ | the predictor |
| git | any | cloning |

## 2. First-time setup (new machine)

```powershell
git clone https://github.com/whewston/f1_stats.git
cd f1_stats

# root env: database password used by compose
Copy-Item .env.example .env         # then set POSTGRES_PASSWORD (dev: devpassword)
```

Bring up the stack (API, web, database):

```powershell
docker compose up --build           # api :8080 · app :3000 · db :5432
```

> Lighter dev loop (less strain than full compose): run only the DB in Docker and
> the API/front end natively —
> `docker compose up -d db`, then `dotnet run --project src/F1Stats.Api` from `api/`,
> and `npm install; npm run dev` from `app/` (Vite on :5173, proxies `/api` to :8080).

### Apply database migrations

From `api/` (the env var lets EF read the Development connection string):

```powershell
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet ef database update --project src/F1Stats.Core --startup-project src/F1Stats.Api
```

> Fresh clone has no `.venv`, and `app/public/f1-circuits.geojson` should already be
> committed. If the track maps are blank, re-fetch it:
> `curl.exe -L -o app/public/f1-circuits.geojson https://cdn.jsdelivr.net/gh/bacinger/f1-circuits@master/f1-circuits.geojson`

### Backfill all historical data

One command per season. This pulls schedule → results → qualifying → standings for
each year (idempotent and throttled to be polite to Jolpica, so it takes a few
minutes for the full range):

```powershell
2019..2026 | ForEach-Object {
  Invoke-RestMethod -Method Post -Uri "http://localhost:8080/admin/ingest/$_" -Headers @{ "X-Admin-Key" = "dev-secret" }
}
```

(bash: `for y in $(seq 2019 2026); do curl -fsS -X POST "http://localhost:8080/admin/ingest/$y" -H "X-Admin-Key: dev-secret"; done`)

You're now fully set up.

## 3. How ingestion works

There is **one** ingestion endpoint. What it refreshes just depends on *when* you run
it — you don't need separate calls for results vs qualifying:

```
POST /admin/ingest/{year}      header: X-Admin-Key: dev-secret
```

A single run of `IngestSeasonAsync` does, for that year:
- upserts the season schedule (circuits + races),
- for every round whose date has passed: race **results** + **qualifying**,
- **qualifying for the next upcoming round** too (so a Saturday grid is available
  before Sunday's race),
- driver + constructor **standings**.

It's idempotent (upserts on natural keys), so re-running never duplicates.

## 4. Race-weekend runbook

The routine, per round. All commands are the same season number (the current year).

**Early in the week — pre-qualifying prediction**
```powershell
# (predictor venv active — see §5)
python predict.py                       # auto-detects "pre", submits pre_qualifying
```

**Saturday, after qualifying — refresh grid, then post-quali prediction**
```powershell
Invoke-RestMethod -Method Post -Uri "http://localhost:8080/admin/ingest/2026" -Headers @{ "X-Admin-Key" = "dev-secret" }
python predict.py --phase post          # grid now available → sharper prediction
```

**Sunday, after the race — refresh results, then score both predictions**
```powershell
Invoke-RestMethod -Method Post -Uri "http://localhost:8080/admin/ingest/2026" -Headers @{ "X-Admin-Key" = "dev-secret" }
Invoke-RestMethod -Method Post -Uri "http://localhost:8080/admin/predictions/2026/{round}/score" -Headers @{ "X-Admin-Key" = "dev-secret" }
```

That's the whole loop: predict → (quali) re-predict → (race) re-ingest → score.
Order matters — always **ingest before predicting or scoring**, so the model sees the
latest data.

## 5. The predictor

Offline batch inference: it only talks to the public API (for features) and the admin
endpoint (to submit). It never touches the database, so it can run anywhere.

**One-time setup** (from `predictor/`):
```powershell
python -m venv .venv
.\.venv\Scripts\Activate.ps1            # bash/zsh: source .venv/bin/activate
pip install -r requirements.txt
Copy-Item .env.example .env             # F1_API_BASE, F1_ADMIN_KEY
```

**Running it:**
```powershell
python predict.py                       # next race, phase auto (post if quali exists, else pre)
python predict.py --dry-run             # print only, submit nothing
python predict.py --phase pre           # force pre-qualifying
python predict.py --phase post          # force post-qualifying (needs ingested grid)
python predict.py --year 2026 --round 9 # a specific race
```

After the season's final race, `/api/races/next` returns nothing and the predictor
exits cleanly — there's nothing to predict in the off-season.

## 6. Environments & secrets

| Where | Key | Local dev value |
|-------|-----|-----------------|
| root `.env` | `POSTGRES_PASSWORD` | `devpassword` |
| `api/src/F1Stats.Api/appsettings.Development.json` | `Admin:ApiKey` | `dev-secret` |
| `predictor/.env` | `F1_API_BASE`, `F1_ADMIN_KEY` | `http://localhost:8080`, `dev-secret` |

`.env` files are gitignored. **Change `Admin:ApiKey` to a real secret before deploying.**

## 7. Deployment

Not yet deployed — this is the plan. The design goal is *cheap*: a small
always-on API + DB, and **no ML compute on the server** (the predictor runs offline
and submits results).

**Host the API + database.** Any container host works (Fly.io, Render, Railway, a
small VM). Postgres as a managed instance or a container with a persistent volume.
Set production environment variables:
- `ASPNETCORE_ENVIRONMENT=Production`
- `ConnectionStrings__Default=<prod postgres connection string>`
- `Admin__ApiKey=<a real, long secret>`

**Apply migrations in prod.** Either enable startup migration in `Program.cs`:
```csharp
using (var scope = app.Services.CreateScope())
    scope.ServiceProvider.GetRequiredService<F1DbContext>().Database.Migrate();
```
…or generate and run a SQL script against the prod DB:
```powershell
dotnet ef migrations script --idempotent --project src/F1Stats.Core --startup-project src/F1Stats.Api
```

**Serve the front end.** The `app/` Dockerfile builds static files and serves them
with nginx, proxying `/api` to the API container (same-origin). Point it at the
deployed API, or host the static build on any static host and set the API origin.

**Schedule weekly ingestion.** A GitHub Actions cron POSTs the ingest endpoint with
the admin key stored as a repo secret — no server-side scheduler needed.
`.github/workflows/weekly-ingest.yml`:
```yaml
name: weekly-ingest
on:
  schedule:
    - cron: '0 6 * * 1'          # Mondays 06:00 UTC, after Sunday races
  workflow_dispatch:              # manual "run now" button
jobs:
  ingest:
    runs-on: ubuntu-latest
    steps:
      - run: curl -fsS -X POST "$API_BASE/admin/ingest/$(date +%Y)" -H "X-Admin-Key: $ADMIN_KEY"
        env:
          API_BASE: ${{ secrets.F1_API_BASE }}
          ADMIN_KEY: ${{ secrets.F1_ADMIN_KEY }}
```

**Predictions stay offline.** Run the predictor locally (or as its own scheduled
GitHub Action) and let it POST to the deployed admin endpoint. This is the whole
point of the batch-inference design: the server never runs the model, so it stays
small and cheap.
