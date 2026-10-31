# Dikit — Progress State

> Tracks milestone status and what is actually implemented versus planned.
> Update this file whenever a milestone's state changes.

## Milestones

| Milestone | Scope | Status |
| --- | --- | --- |
| M0: Foundation | Development environment, WPF starter window, Git, and README | Complete |
| M1: Notes and persistence | Editable notes, local JSON storage, autosave, and window restoration | Implemented (pending Windows verification) |
| M2: Content blocks | Pasted images, safe website links, and checklists | Implemented (pending Windows verification) |
| M3: Theme Studio | Presets, live preview, and per-note customization | Implemented (pending Windows verification) |
| M4: Desktop integration and release | Tray controls, opt-in startup, recovery, and Windows distribution | Planned |

## Implemented on `devin/dikit-v1-ios-notes` (M1)

- MVVM layout: `Models/`, `ViewModels/`, `Views/`, `Services/`, `Infrastructure/`; custom `ObservableObject` and `RelayCommand` (no external MVVM package).
- Schema-versioned `Note` model (`schemaVersion: 1`) with typed `ContentBlock` hierarchy (`text`, `image`, `link`, `checklist`), `WindowPlacement`, `NoteTheme`, `AppSettings`, and `NoteManifest`.
- `NoteStore` under `%LOCALAPPDATA%\Dikit`: per-note `notes/<id>/note.json`, `manifest.json`, `app-settings.json`; atomic same-volume temp-file writes; rolling backups (5 per note) under `backups/<id>/`; deleted notes moved to `backups/deleted/`; unreadable files quarantined to `backups/quarantine/`; load attempts recovery from the newest readable backup and reports warnings in the launcher.
- Borderless movable/resizable/minimizable `NoteWindow` with iOS Notes-inspired styling (Inter with Segoe UI fallback, rounded card, subtle border and shadow).
- Text editing with debounced (~400 ms) autosave; title editing; create, duplicate, delete (with confirmation and a recoverable copy), close-as-hide, pin/always-on-top.
- Window geometry captured and restored with off-screen clamping to the virtual screen; open state persists so notes reopen on next launch.
- `MainWindow` acts only as a host/launcher: note list with open status, New note, Restore all, Hide all, Theme studio, Quit, and recovery notices.
- Five preset themes (Classic Yellow, Lavender, Matcha, Sakura, Midnight) are defined and resolved per note; per-note `themeOverride` and global `globalThemeId` are persisted.

## Implemented on `devin/dikit-v1-ios-notes` (M2)

- Per-note block toolbar (`+ Text`, `+ Checklist`, `+ Link`, `+ Image`) appends typed blocks; every block can be removed via its right-click "Remove block" menu.
- `Ctrl+V` inside a note saves a clipboard bitmap as a PNG under `notes/<id>/assets/` (downscaled past 2048 px on the longest edge, capped at 15 MB) and appends an image block; pasting a lone `http`/`https` URL outside a text field creates a link block.
- `+ Image` imports a `.png/.jpg/.jpeg/.gif/.bmp` file into the note's assets folder. Image blocks render the stored asset offline (decoded at bounded width) with an editable caption and an "Image unavailable" fallback; removing the last image block that references an asset deletes the file. Duplicating a note copies its assets.
- Link blocks expose editable label and URL fields plus an "Open ↗" action that is enabled only for `http`/`https` URIs and launches the default browser via `LinkLauncher`; other schemes are never executed.
- Checklist blocks persist `checked` state and render completed items struck through.

## Implemented on `devin/dikit-v1-ios-notes` (M3)

- `ThemeStudioWindow` (note's "Theme" button for per-note scope, launcher's "Theme…" for global): preset swatch cards, background/text/accent editing via hex field plus a shared swatch palette, font picker, corner-radius and opacity sliders, and a live preview card.
- "Apply to this note" stores the working theme as the note's `themeOverride` and updates the window live; "Inherit global" clears the override so the note follows the global theme again.
- "Apply to all notes" reuses a matching preset or persists a `user-*` theme JSON under `themes/` and sets `globalThemeId`; "Save as preset" writes a reusable theme file without changing scope.
- Custom theme fonts resolve with a `Segoe UI` fallback when the named family is unavailable.

## Verification status

- **Not yet verified:** `dotnet build` and runtime behavior — this branch was developed on Linux without a Windows/WPF-capable .NET SDK, so compilation and UX have not been exercised.
- **To verify on Windows:** build and launch; create/edit/move/resize/minimize/close notes; restart and confirm content, geometry, open state, and topmost restore; corrupt a `note.json` and confirm backup recovery plus the launcher warning; paste a screenshot and confirm it reloads after restart; open an `https://` link and confirm a `javascript:`/`file:` URL cannot be opened; toggle checklists and confirm persistence; edit themes per-note and globally with live preview and confirm persistence across restart.

## Remaining for later milestones

- Tray icon, opt-in launch at sign-in, single instance, release packaging (M4).
