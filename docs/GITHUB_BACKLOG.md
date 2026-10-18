# How we run the backlog on GitHub

Everything lives in this repository: issues hold the work, milestones are the sprints, and a GitHub Project is the board. The script `scripts/create_github_backlog.py` creates all of it from [BACKLOG.md](BACKLOG.md), so nobody has to copy 43 issues by hand.

## How Scrum maps onto GitHub

| Scrum concept | On GitHub |
|---|---|
| Product backlog | The **HireNow Backlog** Project, in a table view of every open issue |
| Epic | An issue labelled `type:epic`, titled `[E4] Orders`. Its items are attached as **sub-issues** |
| User story / enabler | An issue labelled `type:story` or `type:enabler`, titled `[HN-16] Request a service from a seller` |
| Task | A checklist item in the story. When a task needs its own owner or PR, convert it to a sub-issue ("Convert to sub-issue") |
| Bug | An issue from the Bug template, labelled `type:bug` |
| Sprint | Milestones `Sprint 1` … `Sprint 5`, one week each, with due dates |
| Story points | The Project's number field `Story Points` |
| Priority (MoSCoW) | Labels `priority:*`, plus the Project's single-select field `Priority` |
| Sprint board | A Project board view with Todo, In Progress, In Review and Done columns, sliced by milestone |
| Burndown / burn-up | The milestone progress bar, plus a Project Insights chart of Story Points over time |
| Definition of Done | The PR template checklist |

Milestones are used for sprints, rather than the Project "Iteration" field, because the script can create them, they show on every issue and PR, and they come with a progress bar.

## One-time setup (about 15 minutes)

1. **Install the GitHub CLI and sign in.**
   ```bash
   brew install gh
   gh auth login
   gh auth refresh -s project   # lets the CLI create Projects
   ```
2. **Preview** what will be created. This calls nothing on GitHub:
   ```bash
   python3 scripts/create_github_backlog.py --dry-run
   ```
3. **Create** the labels, milestones and issues, plus the Project:
   ```bash
   python3 scripts/create_github_backlog.py --start-date 2026-10-19 --project
   ```
   Set `--start-date` to the Monday of Sprint 1. The script can be re-run safely: it updates labels and milestones and skips issues that already exist.
4. **Finish the Project in the browser.** GitHub's API can't create views or workflows.
   - **Status field:** add `In Review` between `In Progress` and `Done`.
   - **View "Backlog":** a Table, grouped by *Parent issue*, sorted by *Priority* and then *Story Points*. Show Title, Status, Priority, Story Points, Milestone and Assignees.
   - **View "Sprint board":** a Board with columns by *Status*, sliced by *Milestone*. Pick the current sprint in the slice panel.
   - **View "My work":** a Board filtered to `assignee:@me`.
   - **Workflows** (⋯ → Workflows): turn on *Auto-add to project* (filter `is:issue,pr is:open`), *Item added → Todo*, *Item closed → Done* and *Pull request merged → Done*.
   - **Insights:** add a chart with the X axis as *Time*, the Y axis as *Sum of Story Points*, filtered to the current milestone. This is the sprint burn-up.
5. **Repository settings.** Protect `main`: require a PR, 1 approval and, once HN-02 is merged, the CI checks. Allow squash merging only, and turn on "Automatically delete head branches".

## The weekly rhythm (1-week sprints)

| When | Event | Time-box | What happens on GitHub |
|---|---|---|---|
| Monday | Sprint planning | 30 min | Take items from the top of the Backlog view into the `Sprint N` milestone, up to about 16 SP. Split stories into tasks and assign owners |
| Mon / Wed / Fri | Stand-up (async is fine) | 5 min | Each person answers done / next / blocked in the team chat. Add the `blocked` label where needed |
| All week | Development | | Branch, PR, review, merge. Cards move by themselves through the workflows |
| Mid-week | Backlog refinement | 15 min, PO + 1 dev | Add acceptance criteria and estimates to upcoming items, and remove `needs-refinement` |
| Sunday | Sprint review | 20 min | Demo from the production URL, close finished items, and move unfinished ones to the next milestone |
| Sunday | Retrospective | 20 min | Keep / stop / try, recorded in an issue labelled `retro` or in a Discussion |

**Roles.** One **Product Owner** owns the order of the backlog. The **Scrum Master** role rotates each week. Everyone, PO and SM included, is a developer.

## Working on an issue

1. Assign yourself. The card moves to *In Progress* when you start.
2. Branch from `main` and name the branch after the item: `feat/HN-16-request-service`, `fix/HN-21-webhook-replay`.
3. Commit with Conventional Commits: `feat(orders): add request endpoint (#23)`.
4. Open a draft PR early. Its description starts with `Closes #23`, so merging closes the issue and moves the card to Done.
5. Merge once the PR has 1 approval, green CI and a complete Definition of Done checklist. Use squash merge.

## Adding new work

- Use **New issue** and pick *User story*, *Task* or *Bug*. Blank issues are turned off so every item has the same shape.
- New stories get the next free `HN-xx` ID and the `needs-refinement` label until they meet the Definition of Ready in [BACKLOG.md](BACKLOG.md#definition-of-ready).
- Estimate in planning poker with 1, 2, 3, 5 or 8 points. Anything at 8 gets split. After Sprint 1, replace the starting assumption of 1 SP ≈ 2 hours with the team's real velocity.
