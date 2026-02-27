# TaxCal

### Run locally

**.NET CLI** (from repo root):

```bash
dotnet run --project src/TaxCal.Api
```

Runs with the default profile (HTTP at `http://localhost:5132`). To use HTTPS or a specific profile:

```bash
dotnet run --project src/TaxCal.Api --launch-profile https
```

Then open `http://localhost:5132/swagger` (or the URL shown in the console).

**Visual Studio**

1. Open `TaxCal.sln`.
2. Set **TaxCal.Api** as the startup project (right‑click → *Set as Startup Project*).
3. Press **F5** (or *Debug → Start Debugging*) to run with debugging, or **Ctrl+F5** to run without.

Swagger opens at the URL from your selected launch profile (e.g. `http://localhost:5132/swagger`).

---

### Docker

**Run from GitHub Container Registry** (after the image is published by CI):

```bash
docker pull ghcr.io/muhfred/taxcal-api:latest
docker run --rm -p 5132:8080 -p 5133:8081 ghcr.io/muhfred/taxcal-api:latest
```

Then browse `http://localhost:5132/swagger` or `https://localhost:5133/swagger`.

**Build and run from source** (clone the repo first):

```bash
docker build -t taxcal-api:local -f src/TaxCal.Api/Dockerfile .
docker run --rm -p 5132:8080 -p 5133:8081 taxcal-api:local
```

**Run with Docker Compose** (builds from source and uses Development env for Swagger):

```bash
docker compose up --build
```

Then browse `http://localhost:5132/swagger`.

### Publish to GitHub Container Registry (GHCR)

On pushes to `main` and tags like `v1.2.3`, the workflow **Build and publish Docker image** publishes to GHCR as:

- `ghcr.io/<OWNER>/taxcal-api:latest` (main only)
- `ghcr.io/<OWNER>/taxcal-api:<git-sha>`
- `ghcr.io/<OWNER>/taxcal-api:vX.Y.Z` (tag only)