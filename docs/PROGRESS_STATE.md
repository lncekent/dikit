# Dikit — Progress State

> Tracks milestone status and what is actually implemented versus planned.
> Update this file whenever a milestone's state changes.

## Milestones

| Milestone | Scope | Status |
| --- | --- | --- |
| M0: Foundation | Development environment, WPF starter window, Git, and README | Complete |
| M1: Notes and persistence | Editable notes, local JSON storage, autosave, and window restoration | Implemented and Windows smoke-tested |
| M2: Content blocks | Pasted images, safe website links, and checklists | Implemented and Windows smoke-tested |
| M3: Theme Studio | Presets, live preview, and per-note customization | Implemented and Windows smoke-tested |
| M4: Desktop integration and release | Tray controls, opt-in startup, recovery, and Windows distribution | Integration implemented and Windows smoke-tested; packaging planned |

## Implemented on `devin/dikit-v1-ios-notes` (M1)

- MVVM layout: `Models/`, `ViewModels/`, `Views/`, `Services/`, `Infrastructure/`; custom `ObservableObject` and `RelayCommand` (no external MVVM package).
- Schema-versioned `Note` model (`schemaVersion: 1`) with typed `ContentBlock` hierarchy (`text`, `image`, `link`, `checklist`), `WindowPlacement`, `NoteTheme`, `AppSettings`, and `NoteManifest`.
- `NoteStore` under `%LOCALAPPDATA%\Dikit`: per-note `notes/<id>/note.json`, `manifest.json`, `app-settings.json`; atomic same-volume temp-file writes; rolling backups (5 per note) under `backups/<id>/`; deleted notes moved to `backups/deleted/`; unreadable files quarantined to `backups/quarantine/`; load attempts recovery from the newest readable backup with a supported schema version and reports warnings in the launcher.
- Borderless movable/resizable/minimizable `NoteWindow` with iOS Notes-inspired styling (Inter bundled under `Resources/Fonts/` with Segoe UI fallback, rounded card, subtle border and shadow).
- Text editing with debounced (~400 ms) autosave; title editing; create, duplicate, delete (with confirmation and a recoverable copy), close-as-hide, pin/always-on-top.
- Window geometry captured and restored with off-screen clamping to the virtual screen; open state persists so notes reopen on next launch.
- `MainWindow` acts only as a host/launcher: note list with open status, New note, Restore all, Hide all, Theme studio, Quit, and recovery notices.
- Five preset themes (Classic Yellow, Lavender, Matcha, Sakura, Midnight) are defined and resolved per note; per-note `themeOverride` and global `globalThemeId` are persisted.

## Implemented on `devin/dikit-v1-ios-notes` (M2)

- Per-note block toolbar (`+ Text`, `+ Checklist`, `+ Link`, `+ Image`) appends typed blocks; every block can be removed via a focusable per-block remove button or its right-click "Remove block" menu.
- `Ctrl+V` inside a note saves a clipboard bitmap as a PNG under `notes/<id>/assets/` (downscaled past 2048 px on the longest edge, capped at 15 MB) and appends an image block; pasting a lone `http`/`https` URL outside a text field creates a link block.
- `+ Image` imports a `.png/.jpg/.jpeg/.gif/.bmp` file into the note's assets folder. Image blocks render the stored asset offline (decoded at bounded width) with an editable caption and an "Image unavailable" fallback; removing an image block retains its asset while retained JSON backups still reference it, then prunes it safely. Duplicating a note copies only referenced assets.
- Link blocks expose editable label and URL fields plus an "Open ↗" action that is enabled only for `http`/`https` URIs and launches the default browser via `LinkLauncher`; other schemes are never executed.
- Checklist blocks persist `checked` state and render completed items struck through.

## Implemented on `devin/dikit-v1-ios-notes` (M3)

- `ThemeStudioWindow` (note's "Theme" button for per-note scope, launcher's "Theme…" for global): preset swatch cards, background/text/accent editing via hex field plus a shared swatch palette, font picker, corner-radius and opacity sliders, and a live preview card.
- "Apply to this note" stores the working theme as the note's `themeOverride` and updates the window live; "Inherit global" clears the override so the note follows the global theme again.
- "Apply to all notes" reuses a matching preset or persists a `user-*` theme JSON under `themes/` and sets `globalThemeId`; "Save as preset" writes a reusable theme file without changing scope.
- Custom theme fonts resolve with a `Segoe UI` fallback when the named family is unavailable.

## Implemented on `devin/dikit-v1-ios-notes` (M4 — desktop integration)

- `TrayIconService` hosts a system-tray icon (embedded `Resources/dikit.ico`, also the application icon) with a menu: New note, All notes, Hide all, Restore all, Settings, Quit. Left-click opens All notes. Uses the Windows Forms `NotifyIcon` via `UseWindowsForms` — no third-party packages.
- `ShutdownMode="OnExplicitShutdown"` plus `MainWindow.OnClosing` → hide: closing the All notes window or any note never quits; only the tray/window Quit exits. `NotesManagerViewModel.IsShuttingDown` lets real shutdowns through.
- `SingleInstanceGuard`: named mutex `Local\Dikit.SingleInstance`; a second launch signals `Local\Dikit.ShowRequest` (retrying briefly until the running instance has created the event) and exits; the running instance shows/activates its All notes window. Fails open if the named objects cannot be created.
- `StartupService`: opt-in `HKCU\Software\Microsoft\Windows\CurrentVersion\Run` value named `Dikit` (user-level, no admin, reversible). `SyncStartupOption` re-applies the stored preference at launch so a moved executable is re-registered.
- All notes manager gained a search box filtering titles and text/link/caption block content via `NoteViewModel.MatchesSearch` over `NotesView`.
- `SettingsWindow`: "launch at Windows sign-in" toggle, per-note backup retention (1–30), and data-folder display/open.
- `ErrorLog`: appends timestamped errors to `logs/dikit.log`; `DispatcherUnhandledException` and note/settings save failures are logged without crashing the app.
- `WindowGeometry` verifies the restored title strip intersects an attached monitor's work area (per-monitor check via `Screen.AllScreens`, pixel→DIP scaled per screen via `GetDpiForMonitor` so mixed-DPI setups clamp correctly) and otherwise centers on the primary screen.
- `WindowCoordinator` owns note/settings/theme-studio window lifecycles so `NotesManagerViewModel` holds state and intent only; a first-launch welcome note is created when no notes exist; deleting a note cancels its pending autosave first; active search results refresh when note titles or block content change.
- Interactive controls across the windows expose `AutomationProperties.Name` for screen readers (title and block fields, remove buttons, swatches, search, settings fields).

## Verification status

- **Verified on Windows (.NET 10.0.403):** `dotnet restore` and `dotnet build` pass with zero errors; clean launch shows the welcome note and All notes list; note create/edit/drag/resize/minimize-restore/pin/close-as-hide/reopen/duplicate/delete works; delete moves note data and assets to `backups/deleted/` and stays deleted after relaunch; text, checklist, link and image blocks render and persist; `Ctrl+V` creates image assets from a clipboard PNG and link blocks from a lone HTTPS URL; `Open ↗` launches HTTPS in the default browser while `file:` and `javascript:` remain disabled; Theme Studio presets, hex colors, sliders, live preview, per-note and global application persist; the tray menu, Hide all/Restore all, Settings and Quit work; a second instance activates the primary's All notes window; a corrupt `note.json` recovered from the newest valid backup with a notice; `taskkill /F` reopened without note corruption; launch-at-sign-in writes/removes the per-user `Run` value; restarting reopened notes with content, position, pin, theme, checklist and image intact; `manifest.json`, `app-settings.json`, `notes/<id>/note.json`, backups and logs were created under the expected `Dikit` data root.
- **Windows verification fixes:** `5c0f29a` fixed .NET 10 markup/global-usings compile errors; `e3e0bdc` fixed a `Run.Text` two-way binding crash that hid notes and flooded `dikit.log`, a theme-override aliasing bug, and minimize-time geometry capture. Review-hardening commits `5a72053` and `0f0fc77` were also Windows-verified: retention-1 and missing-file recovery, preview cancellation, retained image assets after forced kill, duplicate offset/size, rapid close-after-edit, schema-mismatch reads, and mutex fail-open were exercised.
- **Still to verify before release:** standalone executable launch without IDE/network, a 10-note workload, quarantine when no valid backup exists, actual Windows sign-in after opting in, monitor-detach clamping, Wi-Fi-disabled use, RAM/storage/startup measurement, and Windows packaging/distribution.

## Remaining for later milestones

- Windows release packaging/distribution and resource-usage validation (remaining M4 scope).
