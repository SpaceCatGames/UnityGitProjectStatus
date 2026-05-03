# Unity Git Project Status

SCG Git Project Status is a lightweight Unity Editor package that shows Git working tree changes directly inside the Project window and the primary Inspector header.

The package focuses on one job:

- draw compact badges for changed assets and folders under `Assets/`,
- expose the current branch in a small editor window,
- stay editor-only and avoid Git client features that do not belong in this workflow.

## Features

- Resolves the Git repository root with `git rev-parse --show-toplevel`.
- Runs `git --no-optional-locks status --porcelain=v1 -z --untracked-files=all`.
- Normalizes repository-relative paths back to Unity project paths, including monorepo layouts.
- Draws status badges in the Project window for visible assets and parent folders.
- Can draw a matching badge in the primary Inspector header for the currently inspected persistent asset or folder.
- Supports locked Inspector windows by resolving Inspector badge state from the inspected editor target.
- Supports both right-aligned Project badges and Icon-corner Project badges near the asset icon.
- Supports a Calc Mode that swaps letters for symbols in both Project and Inspector badges while keeping the same status colors and tooltips.
- Remaps `.meta` changes to the visible asset or folder whenever possible.
- Shows deleted paths for the active Project folder in a dedicated footer inside the Project window.
- Coalesces refresh requests so repeated editor events do not spawn overlapping Git processes.

## What It Does Not Do

This package is not a Git client.

It intentionally does not implement staging, committing, reverting, discarding, branching, pushing, pulling, fetching, stash management, history browsing, merge tools, or any other repository mutation workflow.

Use your regular Git client for repository operations. This package only surfaces status information inside Unity.

## Requirements

- Unity `2021.3` or newer
- Git available on the system `PATH`
- A Unity project located inside a Git repository

## Installation

1. Open `Window > Package Manager`.
2. Click `+`.

### Git URL

Add the package through Package Manager or `Packages/manifest.json`:

`https://github.com/SpaceCatGames/UnityGitProjectStatus.git?path=Assets/SCG`

### Local package

1. Choose `Add package from disk...`.
2. Select `Assets/SCG/package.json` from this repository.

## Usage

- Open the window from `SCG > Git Project Status > Project Status Window` or press `Alt + G` (`Option + G` on macOS).
- Refresh manually from `SCG > Git Project Status > Refresh`.
- Use `SCG > Git Project Status > Refresh Mode` to switch between `Manual Only`, `Timed`, and `Event-Driven`.
- Use `SCG > Git Project Status > Badge Settings > Project > Enable Project Overlays` to turn Project badges on or off.
- Use `SCG > Git Project Status > Badge Settings > Inspector > Enable Inspector Badge` to show or hide the Inspector header badge for the currently inspected persistent asset or folder.
- Use `SCG > Git Project Status > Badge Settings > Appearance > Calc Mode` to swap letter markers for symbol markers.
- Use `SCG > Git Project Status > Badge Settings > Project > Right-Aligned Badges` to switch between the default right-aligned Project placement and Icon-corner Project badges near the asset icon.
- Use `SCG > Git Project Status > Badge Settings > Project > Deleted Files in Project` to show or hide the deleted footer in the Project window.
- Use `SCG > Git Project Status > Badge Settings > Project > Left Pane Overlays in Two Column` to control left-pane badges in two-column Project layout.

## Screenshots

Representative editor views are grouped below. Click any preview to open the full-size image.

### Status window overview

[<img src="Screenshots/Screenshot_1.png" alt="Git Status window with refresh controls, badge settings, change counts, and the changed paths list." width="100%">](Screenshots/Screenshot_1.png)

Shows the Git Status window with refresh controls, badge settings, change counts, and the current changed-paths list.

### Project window and status window side by side

[<img src="Screenshots/Screenshot_2.png" alt="Project window shown next to the Git Status window, including badges and the deleted footer." width="100%">](Screenshots/Screenshot_2.png)

Shows the Project window next to the Git Status window, including Project badges and the deleted-paths footer for the active folder.

### Project overlays and deleted footer

[<img src="Screenshots/Screenshot_3.png" alt="Project window overlays with right-aligned badges and the deleted footer." width="100%">](Screenshots/Screenshot_3.png)

Shows right-aligned Project badges together with the deleted-paths footer for the active folder.

### Badge settings menu

[<img src="Screenshots/Screenshot_4.png" alt="SCG Git Project Status menu with Refresh, Refresh Mode, Badge Settings, and Project Status Window entries." width="100%">](Screenshots/Screenshot_4.png)

Shows the top-level menu structure for refresh actions, badge settings groups, and the Project Status Window command.

The status window shows:

- repository availability in the window title indicator,
- current branch,
- last refresh time,
- refresh activity,
- refresh mode and timed interval,
- optional `.meta` entries in `Changed Paths`,
- visible changed asset count,
- deleted file count.

## Refresh Policy

The package supports three refresh modes.

- `Manual only` disables automatic refresh completely.
- `Timed` refreshes on a configurable interval from `1` to `30` seconds, with `5` seconds as the default.
- `Event-driven` is the default mode.

The current event-driven mode refreshes from:

- editor startup or domain reload,
- editor activation after focus returns to the Unity Editor,
- script compilation finished.

Manual refresh is always available from the status window or from the SCG menu.

Refresh mode is stored in editor settings and can be switched from the status window or from the dedicated `Refresh Mode` submenu.

If a refresh is already running, the package keeps at most one follow-up refresh pending.

## Status Markers

| Marker      | Status     | Color            |
| :---------: | ---------- | ---------------- |
| `M` / `*`   | Modified   | Yellow / orange  |
| `A` / `+`   | Added      | Green            |
| `D` / `-`   | Deleted    | Red              |
| `R` / `±`   | Renamed    | Purple / magenta |
| `C` / `/`   | Copied     | Cyan             |
|    `?`      | Untracked  | Blue / gray      |
|    `!`      | Conflicted | Bright red       |
| `I` / `X`   | Ignored    | Muted gray       |

Folder markers use the highest-priority descendant status:

1. Conflicted
2. Deleted
3. Renamed
4. Added
5. Modified
6. Copied
7. Untracked
8. Ignored

## Deleted Files

Deleted assets often no longer have a visible row or GUID in the Project window list.

When Git reports deleted paths under the currently opened Unity folder, the package shows them in a dedicated footer at the bottom of the Project window.

The footer can be collapsed so it does not get in the way, and deleted file paths are no longer duplicated in a separate `Deleted Files` section inside the status window.

There is no restore or checkout action in the package.

## Design Notes

- `git status --porcelain=v1 -z` is used because it is stable for tooling and preserves paths with spaces.
- `git --no-optional-locks` avoids optional Git index locks during editor refreshes.
- `.meta` paths are remapped because Unity displays the asset or folder, not the `.meta` file itself.
- Deleted entries are tracked separately from visible asset badges so a deleted child file does not make its parent folder look deleted.
- Rename and copy records preserve both paths. In porcelain v1 `-z`, Git emits the new path first and the source path as the next null-delimited token.
- The Inspector badge resolves status from the inspected editor target and reconstructs header geometry from the same Unity header layout data used by `Editor.DrawHeaderGUI`.
- The package intentionally stays small and editor-only.

## Troubleshooting

- `Git process could not be started`: install Git and make sure `git` is available on the system `PATH`.
- `No Git repository found at or above the Unity project root`: open a Unity project whose root or parent folder is tracked by Git.
- Paths with spaces are supported because Git commands use `ProcessStartInfo.WorkingDirectory`.
- Large repositories may refresh more slowly because Git status still has to scan the working tree.

## Compatibility Notes

- Unity `2021.3` is the minimum supported editor version.
- The Project Browser integration relies on reflection against Unity editor internals, so long-term compatibility depends on those internal APIs remaining stable.
- The Inspector badge mirrors Unity's large-header layout data to stay aligned with the built-in header across supported editor versions.

## License

MIT License

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
