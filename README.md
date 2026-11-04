# HireNow

A marketplace that connects freelance service providers with new clients.

**Team and cadence:** 5 people · 5 one-week sprints · 40 hours per week for the whole team

**Stack:** React + TypeScript · ASP.NET Core (.NET 10) · SQL Server · Keycloak · Stripe Connect · Google Cloud · GitHub Actions

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
| Keycloak admin console | http://localhost:8081, user `admin`, password `KEYCLOAK_ADMIN_PASSWORD` from `.env` |
| SQL Server | `localhost:1433`, user `sa`, password from `.env` |

The API applies EF Core migrations when it starts in Development, so the database and the service categories are created for you. To delete the local database, run `docker compose down -v`.

After someone adds a frontend dependency, start with `docker compose up --build -V`, because the container otherwise keeps its old `node_modules`.

On macOS, AirPlay Receiver uses port 5000. Turn it off in System Settings if the API can't bind to the port.

### Signing in

Click **Sign in** on the home page. The local test users are `seller`, `buyer` and `admin`, each with the `KEYCLOAK_DEV_USER_PASSWORD` from `.env`. These are users of the `hirenow` realm, so the `admin` test user is not the Keycloak console's `admin`.

If you already have a `.env`, add the two Keycloak passwords from `.env.example` to it.

### Running the apps without Docker

Start only the database and Keycloak, then give the API its connection string through user secrets. Use the password from `.env`.

```bash
docker compose up -d db keycloak
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
