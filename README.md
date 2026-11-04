# HireNow

A marketplace that connects freelance service providers with new clients.

**Team and cadence:** 5 people · 5 one-week sprints · 40 hours per week for the whole team

**Stack:** React + TypeScript · ASP.NET Core (.NET 10) · SQL Server · Stripe Connect · Google Cloud · GitHub Actions

## Documentation

| Document | What's in it |
|---|---|
| [Product vision and user stories](docs/USER_STORIES.md) | The goal, the user roles, the original user stories (SP, SS, AD) and the assumptions to confirm |
| [Product backlog](docs/BACKLOG.md) | Epics, refined stories with acceptance criteria, estimates, the sprint plan and the Definitions of Ready and Done |
| [Technical baseline](docs/TECH_STACK.md) | The stack, the GCP architecture and practical notes |
| [How we run the backlog on GitHub](docs/GITHUB_BACKLOG.md) | Issues, labels, milestones, the Project board and the weekly rhythm |
| [Claude Code plugins and skills](docs/CLAUDE_PLUGINS.md) | The plugins and skills we use for frontend, backend, cloud and CI, and how to install them |

## Getting started

You need Docker Desktop. To work outside Docker, you also need the .NET 10 SDK and Node.js 22.

```bash
cp .env.example .env
docker compose up --build
```

| What | Where |
|---|---|
| Frontend | http://localhost:5173 |
| API health check | http://localhost:5000/health |
| OpenAPI UI | http://localhost:5000/swagger |
| SQL Server | `localhost:1433`, user `sa`, password from `.env` |

The API applies EF Core migrations when it starts in Development, so the database and the service categories are created for you. To delete the local database, run `docker compose down -v`.

On macOS, AirPlay Receiver uses port 5000. Turn it off in System Settings if the API can't bind to the port.

### Running the apps without Docker

Start only the database, then give the API its connection string through user secrets. Use the password from `.env`.

```bash
docker compose up -d db
cd backend
dotnet user-secrets set "ConnectionStrings:Default" "Server=localhost;Database=HireNow;User Id=sa;Password=<password>;TrustServerCertificate=True" --project src/HireNow.Api
dotnet run --project src/HireNow.Api
```

In a second terminal:

```bash
cd frontend
npm install
npm run dev
```
