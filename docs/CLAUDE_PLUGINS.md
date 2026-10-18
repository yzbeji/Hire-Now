# Claude Code plugins and skills for HireNow

Which plugins, skills and integrations the team uses with Claude Code, and how to set them up. A small core set is turned on for everyone through `.claude/settings.json`. The rest are opt-in, depending on what you're working on.

`CLAUDE.md` tells Claude which tool to use for which kind of task. This page is for us.

## One-time setup (everyone)

1. **Open the repo in Claude Code and accept the "trust this folder" prompt.** That registers the taste-skill marketplace and turns on the core plugins listed in `.claude/settings.json`: `typescript-lsp`, `csharp-lsp`, `frontend-design`, `context7` and `taste-skill`.
2. **Install the language servers.** The two LSP plugins need them on your `PATH`:
   ```bash
   dotnet tool install --global csharp-ls
   npm install -g typescript-language-server typescript
   ```
3. **Add Vercel's web design guidelines skill.** One person runs this once and commits the resulting `.claude/skills/` folder, so everyone gets it:
   ```bash
   npx skills add vercel-labs/agent-skills --skill web-design-guidelines -a claude-code
   ```
4. **Check it worked.** Run `/plugin` and open the **Installed** tab, or run `claude plugin list` in your shell.

To turn off a core plugin on your machine only, set it to `false` in `.claude/settings.local.json`.

## Frontend

| Tool | What it is | How to get it | Use it for |
|---|---|---|---|
| **Taste Skill** ([Leonxlnx/taste-skill](https://github.com/Leonxlnx/taste-skill)) | Third-party plugin | Core (automatic). Manual install: `/plugin marketplace add Leonxlnx/taste-skill`, then `/plugin install taste-skill@taste-skill` | Public pages: the home/landing page, "how it works", about. It gives art direction and checks its work in a real browser, so it needs Node 22+ and Chrome. **It isn't meant for app screens**, which its own description says |
| **Web Design Guidelines** ([vercel-labs/agent-skills](https://github.com/vercel-labs/agent-skills)) | Skill | Step 3 above | Auditing UI code against Vercel's Web Interface Guidelines (accessibility, forms, focus states, UX) before every UI pull request |
| **Awesome Design** ([VoltAgent/awesome-design-md](https://github.com/VoltAgent/awesome-design-md)) | A collection of `DESIGN.md` files. Nothing to install | See [Making our DESIGN.md](#making-our-designmd) | The starting point for HireNow's own `DESIGN.md` |
| `frontend-design` | Official Anthropic plugin | Core (automatic) | App screens: search, service and profile pages, forms, chat, orders, admin |
| `typescript-lsp` | Official plugin | Core, plus the binary from step 2 | Go to definition, find references, and type errors straight after an edit |
| `playwright` | Official marketplace (Microsoft's MCP server) | `/plugin install playwright@claude-plugins-official` | Letting Claude click through flows, take screenshots and write end-to-end tests |
| `react-best-practices` | Skill from vercel-labs/agent-skills | `npx skills add vercel-labs/agent-skills --skill react-best-practices -a claude-code` | Optional. React performance rules. They're written for React and Next.js, so skip the Next.js-only parts |
| `chrome-devtools-mcp` | Official marketplace | `/plugin install chrome-devtools-mcp@claude-plugins-official` | Optional. Debugging performance and network requests in a live Chrome |

### How the three design tools fit together

**Awesome Design** decides how HireNow looks: it gives us a `DESIGN.md`. **Taste Skill** and `frontend-design` build pages that follow it. **Web Design Guidelines** checks the result.

### Making our DESIGN.md

This is worth doing in Sprint 2, alongside HN-07, before many screens exist.

1. Pick one starting point and don't mix them:
   - `airbnb`: a consumer marketplace with photo cards and generous spacing. It's the closest match to HireNow's public side.
   - `linear.app` or `stripe`: dense, calm app screens. They suit orders and admin.
2. Download it to the repo root:
   ```bash
   curl -o DESIGN.md https://raw.githubusercontent.com/VoltAgent/awesome-design-md/main/design-md/airbnb/DESIGN.md
   ```
3. Ask Claude to make it ours: *"Adapt DESIGN.md into HireNow's own design system. Keep the structure and the spacing, elevation and component rules. Replace the brand name, colour palette and fonts with our own, using free Google Fonts. Then map the tokens into an MUI theme in `frontend/src/theme.ts`."*
4. Commit both files. From then on, `CLAUDE.md` tells Claude to read `DESIGN.md` before any UI work.

These files describe real brands. Keep their structure and principles, but don't copy their logos, names or proprietary fonts (Airbnb Cereal, for example).

## Backend: .NET, SQL Server and Stripe

| Tool | What it is | How to get it | Use it for |
|---|---|---|---|
| `csharp-lsp` | Official plugin | Core, plus `csharp-ls` from step 2 | Code navigation and compiler errors in the C# solution |
| `context7` | Official marketplace (Upstash) | Core (automatic) | Up-to-date, version-specific docs for EF Core, FluentValidation, Stripe.net, QuestPDF, MUI and TanStack Query |
| `microsoft-docs` | Official marketplace (Microsoft Learn) | `/plugin install microsoft-docs@claude-plugins-official` | Current docs for ASP.NET Core, Identity, SignalR and EF Core |
| `stripe` | Official marketplace (Stripe) | `/plugin install stripe@claude-plugins-official` | Connect onboarding, Checkout, webhooks and transfers (HN-20 to HN-23) |
| `security-guidance` | Official Anthropic plugin | `/plugin install security-guidance@claude-plugins-official` | Warnings while you edit, plus a review at the end of each turn. Turn it on for authentication (HN-05), payments (E5) and admin (HN-30). The end-of-turn review costs extra tokens, so turn it off for routine work |
| `feature-dev` | Official Anthropic plugin | `/plugin install feature-dev@claude-plugins-official` | Optional. A guided explore → design → implement workflow for bigger stories |

## Cloud: GCP

For whoever owns HN-03 and HN-04. These tools act on real cloud resources with your own gcloud credentials, so read each command before you approve it.

| Tool | What it is | How to get it | Use it for |
|---|---|---|---|
| `cloud-sql-sqlserver` | Official marketplace (Google) | `/plugin install cloud-sql-sqlserver@claude-plugins-official` | Connecting to and inspecting the Cloud SQL for SQL Server database |
| `google-cloud-storage` | Official marketplace (Google) | `/plugin install google-cloud-storage@claude-plugins-official` | Buckets, CORS, IAM and signed URLs (HN-03, HN-08) |
| `firebase` | Official marketplace | `/plugin install firebase@claude-plugins-official` | Optional. Managing Firebase Hosting for the frontend |
| Cloud Run MCP server ([GoogleCloudPlatform/cloud-run-mcp](https://github.com/GoogleCloudPlatform/cloud-run-mcp)) | MCP server, not a plugin | `gcloud auth login && gcloud auth application-default login`, then `claude mcp add cloud-run -- npx -y @google-cloud/cloud-run-mcp` | Listing services, reading service logs and debugging failed deploys (HN-04) |

## CI, GitHub and code review

| Tool | What it is | How to get it | Use it for |
|---|---|---|---|
| **Claude Code GitHub Action** ([docs](https://code.claude.com/docs/en/github-actions)) | GitHub App plus a workflow | Run `/install-github-app` in Claude Code. You need the `gh` CLI signed in and admin access to the repo | `@claude` in issues and PRs, and optionally an automatic review on every PR. It uses GitHub Actions minutes plus Claude usage, through an API key or a subscription token. Set it up once HN-02 is merged |
| `github` | Official marketplace (GitHub's MCP server) | Create a personal access token, set `GITHUB_PERSONAL_ACCESS_TOKEN` in your shell, then `/plugin install github@claude-plugins-official` | Reading Actions runs and logs, and working with issues and the backlog Project from Claude |
| `/code-review`, `/security-review` | Built into Claude Code | Nothing to install | Reviewing your branch before asking a teammate |
| `commit-commands` | Official Anthropic plugin | `/plugin install commit-commands@claude-plugins-official` | Optional. `/commit` and `/commit-push-pr` shortcuts |

## Also useful

| Tool | How to get it | Use it for |
|---|---|---|
| `claude-md-management` | `/plugin install claude-md-management@claude-plugins-official` | Keeping `CLAUDE.md` accurate as the project grows: it audits the file and captures what you learn in each session |
| `learning-output-style` | `/plugin install learning-output-style@claude-plugins-official` | For a lab where everyone must understand the code: Claude asks you to write the key parts yourself at decision points |
| `explanatory-output-style` | `/plugin install explanatory-output-style@claude-plugins-official` | A lighter version: Claude explains why it chose each implementation |

## Keep it lean and safe

- Every enabled plugin adds tokens to each turn. Run `claude plugin details <name>` to see the cost, and disable what you don't use from the **Installed** tab in `/plugin`.
- Anthropic doesn't review third-party sources such as Taste Skill and Vercel's skills. Look at what a plugin adds before you install it. The `/plugin` details pane lists it.
- Plugins and MCP servers that touch GitHub, GCP or Stripe use your own credentials. Never paste tokens or keys into a chat, an issue or a commit.
