#!/usr/bin/env python3
"""Turn docs/BACKLOG.md into a GitHub backlog.

Creates labels, one milestone per sprint, one issue per epic, and one issue per
backlog item linked to its epic as a sub-issue. With --project it also creates
a GitHub Project with "Story Points" and "Priority" fields and fills them in.

Requirements: Python 3.9+ and the GitHub CLI.
    brew install gh && gh auth login
    gh auth refresh -s project        # only needed for --project

Examples:
    python3 scripts/create_github_backlog.py --dry-run
    python3 scripts/create_github_backlog.py --start-date 2026-10-19
    python3 scripts/create_github_backlog.py --start-date 2026-10-19 --project

Safe to re-run: labels and milestones are updated in place, and issues whose
title already exists are reused instead of being created again.
"""

from __future__ import annotations

import argparse
import datetime as dt
import json
import re
import shutil
import subprocess
import sys
from dataclasses import dataclass, field
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
BACKLOG_PATH = ROOT / "docs" / "BACKLOG.md"
PROJECT_TITLE = "HireNow Backlog"
PRIORITIES = ["Must", "Should", "Could", "Won't"]

LABELS = {
    "type:epic": ("5319E7", "Large feature made of several stories"),
    "type:story": ("1D76DB", "User story: delivers value to a seller, buyer or admin"),
    "type:enabler": ("0E8A16", "Technical work that other stories depend on"),
    "type:task": ("C5DEF5", "Piece of work inside a story"),
    "type:bug": ("D73A4A", "Something isn't working"),
    "priority:must": ("B60205", "MoSCoW: Must have for the MVP"),
    "priority:should": ("D93F0B", "MoSCoW: Should have"),
    "priority:could": ("FBCA04", "MoSCoW: Could have"),
    "priority:wont": ("CCCCCC", "MoSCoW: Won't have in this release"),
    "role:seller": ("006B75", "Service provider"),
    "role:buyer": ("C2E0C6", "Service seeker"),
    "role:admin": ("F9D0C4", "Platform admin"),
    "area:frontend": ("BFDADC", "React app"),
    "area:backend": ("D4C5F9", "ASP.NET Core API and database"),
    "area:devops": ("FEF2C0", "CI/CD and Google Cloud"),
    "needs-refinement": ("E99695", "Missing acceptance criteria or estimate"),
    "blocked": ("000000", "Waiting on something outside the team"),
}

EPIC_HEADING = re.compile(r"^## (E\d+) · (.+)$")
ITEM_HEADING = re.compile(r"^### (HN-\d+) · (.+)$")
SPRINT_ROW = re.compile(r"^\| Sprint (\d+) \| ([^|]+) \|")


@dataclass
class Item:
    key: str
    title: str
    meta: dict[str, str]
    body: str

    @property
    def issue_title(self) -> str:
        return f"[{self.key}] {self.title}"

    @property
    def sprint(self) -> int | None:
        value = self.meta["Sprint"]
        return int(value) if value.isdigit() else None

    @property
    def points(self) -> int:
        return int(self.meta["Points"])

    @property
    def priority(self) -> str:
        return self.meta["Priority"]

    @property
    def labels(self) -> list[str]:
        labels = [f"type:{self.meta['Type'].lower()}", f"priority:{self.priority.lower()}"]
        extra = self.meta.get("Labels", "")
        labels += [label.strip() for label in extra.split(",") if label.strip() not in ("", "—")]
        return labels


@dataclass
class Epic:
    key: str
    title: str
    intro_lines: list[str] = field(default_factory=list)
    items: list[Item] = field(default_factory=list)

    @property
    def issue_title(self) -> str:
        return f"[{self.key}] {self.title}"


def table_cells(line: str) -> list[str]:
    return [cell.strip() for cell in line.strip().strip("|").split("|")]


def trim(lines: list[str]) -> str:
    text = "\n".join(lines).strip()
    while text.endswith("---"):
        text = text[:-3].rstrip()
    return text


def parse_item(key: str, title: str, lines: list[str]) -> Item:
    start = next((i for i, line in enumerate(lines) if line.startswith("|")), None)
    if start is None or start + 2 >= len(lines):
        raise ValueError(f"{key}: missing the metadata table under the heading")
    meta = dict(zip(table_cells(lines[start]), table_cells(lines[start + 2])))
    missing = {"Type", "Priority", "Points", "Sprint"} - meta.keys()
    if missing:
        raise ValueError(f"{key}: metadata table is missing {', '.join(sorted(missing))}")
    if meta["Priority"] not in PRIORITIES:
        raise ValueError(f"{key}: unknown priority {meta['Priority']!r}")
    return Item(key, title, meta, trim(lines[start + 3 :]))


def parse_backlog(text: str) -> tuple[list[Epic], dict[int, str]]:
    """Return the epics (with their items) and the sprint goals by sprint number."""
    epics: list[Epic] = []
    sprint_goals: dict[int, str] = {}
    epic: Epic | None = None
    current: tuple[str, str, list[str]] | None = None

    def flush() -> None:
        nonlocal current
        if current and epic:
            epic.items.append(parse_item(*current))
        current = None

    for line in text.splitlines():
        row = SPRINT_ROW.match(line)
        if row:
            sprint_goals[int(row.group(1))] = row.group(2).strip()
        if line.startswith("## "):
            flush()
            match = EPIC_HEADING.match(line)
            epic = Epic(match.group(1), match.group(2).strip()) if match else None
            if epic:
                epics.append(epic)
        elif line.startswith("### "):
            flush()
            match = ITEM_HEADING.match(line)
            if match and epic:
                current = (match.group(1), match.group(2).strip(), [])
        elif current is not None:
            current[2].append(line)
        elif epic is not None and not epic.items:
            epic.intro_lines.append(line)
    flush()
    return epics, sprint_goals


def next_monday(today: dt.date | None = None) -> dt.date:
    today = today or dt.date.today()
    return today + dt.timedelta(days=(7 - today.weekday()) % 7)


def sprint_dates(start: dt.date, number: int) -> tuple[dt.date, dt.date]:
    first = start + dt.timedelta(weeks=number - 1)
    return first, first + dt.timedelta(days=6)


def repo_from_git() -> str:
    url = subprocess.run(
        ["git", "-C", str(ROOT), "remote", "get-url", "origin"], capture_output=True, text=True
    ).stdout.strip()
    match = re.search(r"github\.com[:/]([^/]+)/(.+?)(?:\.git)?$", url)
    if not match:
        sys.exit("Could not read the GitHub repository from `git remote`; pass --repo owner/name.")
    return f"{match.group(1)}/{match.group(2)}"


def item_body(item: Item, epic: Epic) -> str:
    sprint = f"Sprint {item.sprint}" if item.sprint else "Stretch"
    traces = item.meta.get("Traces to", "—")
    header = (
        f"> **Epic:** {epic.issue_title} · **Priority:** {item.priority} · "
        f"**Story points:** {item.points} · **Planned:** {sprint} · **Traces to:** {traces}"
    )
    return f"{header}\n\n{item.body}\n\n<sub>Seeded from `docs/BACKLOG.md`.</sub>"


def epic_body(epic: Epic) -> str:
    rows = "\n".join(
        f"- **{item.key}** {item.title} · {item.priority} · {item.points} SP" for item in epic.items
    )
    total = sum(item.points for item in epic.items)
    return (
        f"{trim(epic.intro_lines)}\n\n**Items ({total} SP)**, linked below as sub-issues:\n\n{rows}"
        "\n\n<sub>Seeded from `docs/BACKLOG.md`.</sub>"
    )


def print_plan(repo: str, epics: list[Epic], goals: dict[int, str], start: dt.date) -> None:
    items = [item for epic in epics for item in epic.items]
    print(f"Repository: {repo}\n\nMilestones")
    for number, goal in sorted(goals.items()):
        first, last = sprint_dates(start, number)
        planned = sum(item.points for item in items if item.sprint == number)
        print(f"  Sprint {number}  {first} → {last}  {planned:>2} SP  {goal}")
    print("\nIssues")
    for epic in epics:
        print(f"  {epic.issue_title}  ({sum(i.points for i in epic.items)} SP)")
        for item in epic.items:
            sprint = f"Sprint {item.sprint}" if item.sprint else "Stretch"
            print(f"     {item.issue_title}  | {item.priority} | {item.points} SP | {sprint} | {', '.join(item.labels)}")
    stretch = [item for item in items if item.sprint is None]
    print(
        f"\nTotal: {len(epics)} epics, {len(items)} items, {sum(i.points for i in items)} SP; "
        f"{len(stretch)} stretch items ({sum(i.points for i in stretch)} SP)\n"
    )


class GitHub:
    def __init__(self, repo: str) -> None:
        self.repo = repo
        self.owner, self.name = repo.split("/")

    def run(self, *args: str, payload: dict | None = None):
        command = ["gh", *args]
        result = subprocess.run(
            command,
            input=json.dumps(payload) if payload is not None else None,
            capture_output=True,
            text=True,
        )
        if result.returncode != 0:
            raise RuntimeError(f"`{' '.join(command)}` failed: {result.stderr.strip()}")
        output = result.stdout.strip()
        return json.loads(output) if output[:1] in ("{", "[") else output

    def api(self, method: str, path: str, payload: dict | None = None):
        extra = ["--input", "-"] if payload is not None else []
        return self.run("api", "--method", method, f"repos/{self.repo}/{path}", *extra, payload=payload)


def create_labels(gh: GitHub) -> None:
    for name, (color, description) in LABELS.items():
        gh.run("label", "create", name, "--color", color, "--description", description, "--force", "--repo", gh.repo)
    print(f"✓ {len(LABELS)} labels")


def create_milestones(gh: GitHub, goals: dict[int, str], start: dt.date) -> dict[int, int]:
    existing = {m["title"]: m["number"] for m in gh.api("GET", "milestones?state=all&per_page=100")}
    numbers = {}
    for sprint, goal in sorted(goals.items()):
        title = f"Sprint {sprint}"
        # Midday UTC keeps the due date on the same calendar day in every timezone.
        payload = {"title": title, "description": goal, "due_on": f"{sprint_dates(start, sprint)[1]}T12:00:00Z"}
        if title in existing:
            numbers[sprint] = gh.api("PATCH", f"milestones/{existing[title]}", payload)["number"]
        else:
            numbers[sprint] = gh.api("POST", "milestones", payload)["number"]
    print(f"✓ {len(numbers)} milestones")
    return numbers


def create_issues(gh: GitHub, epics: list[Epic], milestones: dict[int, int]) -> list[tuple[dict, Item | None]]:
    listed = gh.run("issue", "list", "--repo", gh.repo, "--state", "all", "--limit", "1000", "--json", "number,title")
    existing = {issue["title"]: issue["number"] for issue in listed}
    created: list[tuple[dict, Item | None]] = []

    def ensure(title: str, body: str, labels: list[str], milestone: int | None) -> tuple[dict, bool]:
        if title in existing:
            print(f"  = #{existing[title]} {title} (already exists)")
            return gh.api("GET", f"issues/{existing[title]}"), False
        payload: dict = {"title": title, "body": body, "labels": labels}
        if milestone:
            payload["milestone"] = milestone
        issue = gh.api("POST", "issues", payload)
        print(f"  + #{issue['number']} {title}")
        return issue, True

    for epic in epics:
        epic_issue, _ = ensure(epic.issue_title, epic_body(epic), ["type:epic"], None)
        created.append((epic_issue, None))
        for item in epic.items:
            milestone = milestones.get(item.sprint) if item.sprint else None
            issue, is_new = ensure(item.issue_title, item_body(item, epic), item.labels, milestone)
            created.append((issue, item))
            try:
                gh.api("POST", f"issues/{epic_issue['number']}/sub_issues", {"sub_issue_id": issue["id"]})
            except RuntimeError as error:
                if is_new:  # existing issues are usually linked already
                    print(f"    ! could not link {item.key} under {epic.key}: {error}")
    print(f"✓ {len(created)} issues")
    return created


def setup_project(gh: GitHub, issues: list[tuple[dict, Item | None]]) -> None:
    listed = gh.run("project", "list", "--owner", gh.owner, "--format", "json", "--limit", "100")["projects"]
    project = next((p for p in listed if p["title"] == PROJECT_TITLE), None)
    if project is None:
        project = gh.run("project", "create", "--owner", gh.owner, "--title", PROJECT_TITLE, "--format", "json")
    number, project_id = str(project["number"]), project["id"]
    try:
        gh.run("project", "link", number, "--owner", gh.owner, "--repo", gh.name)
    except RuntimeError:
        print("  ! could not link the project to the repo; do it from the project's settings")

    def fields() -> dict[str, dict]:
        data = gh.run("project", "field-list", number, "--owner", gh.owner, "--format", "json", "--limit", "100")
        return {f["name"]: f for f in data["fields"]}

    current = fields()
    if "Story Points" not in current:
        gh.run("project", "field-create", number, "--owner", gh.owner, "--name", "Story Points", "--data-type", "NUMBER")
    if "Priority" not in current:
        gh.run(
            "project", "field-create", number, "--owner", gh.owner, "--name", "Priority",
            "--data-type", "SINGLE_SELECT", "--single-select-options", ",".join(PRIORITIES),
        )
    current = fields()
    points_field = current["Story Points"]["id"]
    priority_field = current["Priority"]["id"]
    priority_options = {option["name"]: option["id"] for option in current["Priority"]["options"]}

    for issue, item in issues:
        added = gh.run("project", "item-add", number, "--owner", gh.owner, "--url", issue["html_url"], "--format", "json")
        if item is None:
            continue
        edit = ["project", "item-edit", "--id", added["id"], "--project-id", project_id]
        gh.run(*edit, "--field-id", points_field, "--number", str(item.points))
        gh.run(*edit, "--field-id", priority_field, "--single-select-option-id", priority_options[item.priority])
    print(f"✓ project '{PROJECT_TITLE}': {project.get('url', '')}")


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("--repo", help="owner/name (default: read from the git remote 'origin')")
    parser.add_argument(
        "--start-date",
        type=dt.date.fromisoformat,
        default=next_monday(),
        help="first day of Sprint 1, YYYY-MM-DD (default: the coming Monday)",
    )
    parser.add_argument("--project", action="store_true", help=f"also create and fill the '{PROJECT_TITLE}' Project")
    parser.add_argument("--dry-run", action="store_true", help="only print what would be created")
    args = parser.parse_args()

    epics, goals = parse_backlog(BACKLOG_PATH.read_text(encoding="utf-8"))
    repo = args.repo or repo_from_git()
    print_plan(repo, epics, goals, args.start_date)
    if args.dry_run:
        return
    if not shutil.which("gh"):
        sys.exit("The GitHub CLI is not installed: brew install gh && gh auth login")

    gh = GitHub(repo)
    create_labels(gh)
    milestones = create_milestones(gh, goals, args.start_date)
    issues = create_issues(gh, epics, milestones)
    if args.project:
        setup_project(gh, issues)


if __name__ == "__main__":
    main()
