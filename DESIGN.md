# HireNow design

Hiring someone starts with a notice on a board and ends with a job ticket. The look comes from those two objects: paper notices with tear-off tabs, and carbon-copy tickets where each copy has its own colour.

All tokens live in `frontend/src/theme.ts`. Use them through the MUI theme, and don't add hard-coded colours or fonts to components.

## Colour

| Name | Value | Use |
|---|---|---|
| Carbon ink | `#16227A` | Text, top bar, primary actions, admin areas |
| Canary copy | `#FFD84D` | Service providers, and the home notice |
| Pink copy | `#FFC2CF` | Service seekers |
| Board | `#E4E8F2` | Page background |

Colour carries meaning. Canary always means provider and pink always means seeker, so don't use them as decoration.

## Type

- **Big Shoulders Display** (800) for headings and the wordmark. It's the poster voice, so keep it for short lines.
- **Atkinson Hyperlegible Next** for everything else.
- Sentence case everywhere, including buttons.

## Layout

- Left-aligned, with content up to 720px wide on notices.
- A dashed line means a tear-off edge. Use it only for that.
- The tilted notice on the home page is the one tilted element. Everything else sits straight.

## Words

Plain verbs and sentence case. Call people "service provider" and "service seeker" in choices, and say what happens when a button is used.
