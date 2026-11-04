# HireNow

A marketplace that connects freelance service providers (sellers) with service seekers (buyers), with an admin area for moderation. MOPS lab project: 5 people, 5 one-week sprints.

## Where things are

- Product goal and original user stories (SP-, SS- and AD- IDs): `docs/USER_STORIES.md`
- Backlog items (HN-xx), acceptance criteria, the sprint plan and the Definition of Done: `docs/BACKLOG.md`. Once the issues are seeded, the GitHub Project is the live backlog
- Stack, architecture and GCP notes: `docs/TECH_STACK.md`
- Branching, PRs and the weekly rhythm: `docs/GITHUB_BACKLOG.md`
- Claude Code plugins and skills, and how to install them: `docs/CLAUDE_PLUGINS.md`

## Stack

- `frontend/`: React 19 + TypeScript (Vite, React Router, TanStack Query, MUI)
- `backend/`: ASP.NET Core Web API on .NET 10 (`Api`, `Application`, `Domain`, `Infrastructure`, `Tests`), EF Core, SQL Server
- Authentication: Keycloak (`keycloak/` holds the realm), `keycloak-js` in the frontend, JWT bearer in the API
- Payments: Stripe Connect, test mode only
- Hosting (set up last, in Sprint 5): Cloud Run, Cloud SQL for SQL Server, Cloud Storage and Firebase Hosting on GCP. CI/CD runs on GitHub Actions

## Commands

Everything at once: `docker compose up --build` (first run: `cp .env.example .env`).

Backend, from `backend/`:

- Build and test: `dotnet build`, `dotnet test`
- Run: `dotnet run --project src/HireNow.Api`
- Add a migration (run `dotnet tool restore` once first): `dotnet ef migrations add <Name> --project src/HireNow.Infrastructure --startup-project src/HireNow.Api --output-dir Persistence/Migrations`

Frontend, from `frontend/`:

- Run: `npm run dev`
- Lint, test and build: `npm run lint`, `npm test`, `npm run build`
- Format: `npm run format`

## Working rules

- Every change belongs to a backlog item. Name branches `feat/HN-xx-short-name`, use Conventional Commits, and start the PR body with `Closes #<issue>`.
- A change is done when it meets the item's acceptance criteria and the Definition of Done in `docs/BACKLOG.md`.
- Never commit secrets. Locally they go in .NET user secrets; in production they come from Secret Manager.
- Frontend structure: each feature lives in `frontend/src/features/<name>/` with its query hooks and components. Pages in `src/pages/` only compose those, and never call `useQuery` or the API directly. Routes are in `src/routes.tsx`, and role-protected ones go inside `RequireRole`. The app shell is in `src/components/`. `src/api/client.ts` is the only file that calls `fetch`.
- Order status changes go through the order state machine (the E4 diagram in `docs/BACKLOG.md`). Never set a status directly.

## Which tool to use when

- **Look and feel.** Once `DESIGN.md` exists at the repo root, it is the visual source of truth. Read it before any UI work, and express its tokens through the MUI theme. Don't add Tailwind or a second component library.
- **Public marketing pages** (the home/landing page, "how it works", about): use the `taste-skill` plugin. It is built for public-facing pages and says it is not for app screens.
- **App screens** (search, service and profile pages, forms, chat, orders, admin): use the `frontend-design` skill with MUI components.
- **Before a UI pull request:** run the `web-design-guidelines` skill on the changed components and fix the accessibility findings.
- **Library and API questions:** look up current docs instead of relying on memory. Use `context7` for React, MUI, TanStack Query, EF Core, Stripe.net and QuestPDF, and `microsoft-docs` for ASP.NET Core and SignalR.
- **Payments (epic E5):** use the `stripe` plugin, and keep everything in test mode.
- **Code navigation:** prefer the LSP tools from `csharp-lsp` and `typescript-lsp` over text search for definitions, references and type errors.
- **Review:** run `/code-review` on the branch before asking a teammate. For authentication, payments or admin code, also run `/security-review`.
