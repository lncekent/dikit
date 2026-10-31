# Dikit — Progress State

> Tracks milestone status and what is actually implemented versus planned.
> Update this file whenever a milestone's state changes.

## Milestones

| Milestone | Scope | Status |
| --- | --- | --- |
| M0: Foundation | Development environment, WPF starter window, Git, and README | Complete |
| M1: Notes and persistence | Editable notes, local JSON storage, autosave, and window restoration | Implemented (pending Windows verification) |
| M2: Content blocks | Pasted images, safe website links, and checklists | Planned (content-block model in place) |
| M3: Theme Studio | Presets, live preview, and per-note customization | Planned (theme model and presets in place) |
| M4: Desktop integration and release | Tray controls, opt-in startup, recovery, and Windows distribution | Planned |

## Implemented on `devin/dikit-v1-ios-notes` (M1)

- MVVM layout: `Models/`, `ViewModels/`, `Views/`, `Services/`, `Infrastructure/`; custom `ObservableObject` and `RelayCommand` (no external MVVM package).
- Schema-versioned `Note` model (`schemaVersion: 1`) with typed `ContentBlock` hierarchy (`text`, `image`, `link`, `checklist`), `WindowPlacement`, `NoteTheme`, `AppSettings`, and `NoteManifest`.
- `NoteStore` under `%LOCALAPPDATA%\Dikit`: per-note `notes/<id>/note.json`, `manifest.json`, `app-settings.json`; atomic same-volume temp-file writes; rolling backups (5 per note) under `backups/<id>/`; deleted notes moved to `backups/deleted/`; unreadable files quarantined to `backups/quarantine/`; load attempts recovery from the newest readable backup and reports warnings in the launcher.
- Borderless movable/resizable/minimizable `NoteWindow` with iOS Notes-inspired styling (Inter with Segoe UI fallback, rounded card, subtle border and shadow).
- Text editing with debounced (~400 ms) autosave; title editing; create, duplicate, delete (with confirmation and a recoverable copy), close-as-hide, pin/always-on-top.
- Window geometry captured and restored with off-screen clamping to the virtual screen; open state persists so notes reopen on next launch.
- `MainWindow` acts only as a host/launcher: note list with open status, New note, Restore all, Hide all, Quit, and recovery notices.
- Five preset themes (Classic Yellow, Lavender, Matcha, Sakura, Midnight) are defined and resolved per note; per-note `themeOverride` and global `globalThemeId` are persisted.

## Verification status

- **Not yet verified:** `dotnet build` and runtime behavior — this branch was developed on Linux without a Windows/WPF-capable .NET SDK, so compilation and UX have not been exercised.
- **To verify on Windows:** build and launch; create/edit/move/resize/minimize/close notes; restart and confirm content, geometry, open state, and topmost restore; corrupt a `note.json` and confirm backup recovery plus the launcher warning.

## Remaining for later milestones

- Clickable safe links, clipboard image paste into `notes/<id>/assets/`, checklist editing UI polish (M2).
- Theme Studio UI and live preview (M3).
- Tray icon, opt-in launch at sign-in, single instance, release packaging (M4).
