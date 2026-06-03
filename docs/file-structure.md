f1-stats/
├── docker-compose.yml     <- root: orchestrates everything
├── .gitignore  .env.example
├── api/                   <- .NET backend, its own deployable
│   ├── F1Stats.sln
│   ├── Dockerfile  .dockerignore
│   └── src/F1Stats.Api/
├── app/                   <- web/PWA front end (Phase 2)
└── docs/