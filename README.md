# Dikit

*Stick your thoughts, your way.*

Dikit is a Windows desktop app being built for personal sticky notes and screenshot references. The goal is to keep ideas, images, links, and checklists in customizable note windows, saved locally and ready when you return.

Designed for students, developers, and anyone who wants quick references beside their work. Core workflows are planned to work offline, without accounts, cloud storage, or a background server.

## Project status

**Early development.** On `devin/dikit-v1-ios-notes`, M1–M3 are implemented: editable sticky-note windows with typed content blocks (text, images, links, checklists), debounced autosave to local JSON, independent movable/resizable/minimizable note windows, close-as-hide, pin-to-top, window-state restoration with off-screen recovery, backups, a launcher window, and a Theme Studio with presets and per-note/global customization. Compilation and runtime behavior still need verification on Windows (see [progress state](docs/PROGRESS_STATE.md)).

## Implemented (M1–M3, branch)

- Create, edit, duplicate, and delete notes; deleting keeps a recoverable copy under `backups/deleted/`.
- Typed content blocks per note: text, pasted or imported images stored under `notes/<id>/assets/`, `http`/`https`-only clickable links, and checklists with persistent checked state.
- Paste (`Ctrl+V`) a screenshot to attach it, or a lone URL to create a link block; blocks are removable by right-click.
- Independent note windows that restore position, size, open state, and pin status.
- Local storage under `%LOCALAPPDATA%\Dikit` (per-note JSON, manifest, settings, backups).
- Theme Studio: presets (Classic Yellow, Lavender, Matcha, Sakura, Midnight), background/text/accent colors, font, corner radius, opacity, live preview, per-note overrides, and global themes saved under `themes/`.

## Planned features

- Manage notes from the system tray and opt in to launch at Windows sign-in (M4).

See the [product requirements](docs/PRD/V1.0.md) for scope, acceptance criteria, and open decisions.

## Installation and local setup

The current way to try Dikit is to build it from source. These steps launch the starter window.

### Requirements

- **Windows 11** is the initial target. Windows 10 compatibility is not yet validated.
- [.NET 10 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0), not just the runtime.
- [Git](https://git-scm.com/downloads/win) to clone the repository.
- Optional: [Visual Studio Code](https://code.visualstudio.com/) with Microsoft's [C# extension](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csharp).

Full Visual Studio and C# Dev Kit are optional. Node.js, Docker, and a database server are not required. Initial setup may need internet access to download tools and restore dependencies.

### Clone and run

Open PowerShell in the folder where you keep your projects:

```powershell
git clone https://github.com/lncekent/dikit.git
cd dikit
dotnet restore
dotnet build
dotnet run
```

If you already have the project, open its folder and start with `dotnet restore`.

### Build commands

| Command | Purpose |
| --- | --- |
| `dotnet restore` | Restore NuGet dependencies. |
| `dotnet build` | Compile the application. |
| `dotnet run` | Build and launch the application. |
| `dotnet clean` | Clean generated build output. |

Use `dotnet run --no-build` only after a successful build. For setup checks and known build issues, see the [development setup and troubleshooting guide](docs/DEVELOPMENT_SETUP.md).

## Roadmap

| Milestone | Scope | Status |
| --- | --- | --- |
| M0: Foundation | Development environment, WPF starter window, Git, and README | Complete |
| M1: Notes and persistence | Editable notes, local JSON storage, autosave, and window restoration | Implemented on `devin/dikit-v1-ios-notes` (pending Windows verification) |
| M2: Content blocks | Pasted images, safe website links, and checklists | Implemented on `devin/dikit-v1-ios-notes` (pending Windows verification) |
| M3: Theme Studio | Presets, live preview, and per-note customization | Implemented on `devin/dikit-v1-ios-notes` (pending Windows verification) |
| M4: Desktop integration and release | Tray controls, opt-in startup, recovery, and Windows distribution | Planned |

## Development

Dikit uses **C#, WPF/XAML, and .NET 10** with an MVVM structure: `Models/`, `ViewModels/`, `Views/`, `Services/` (persistence), and `Infrastructure/` (paths, atomic file writes, debouncing, MVVM helpers). There are no external package dependencies.

Notes persist under `%LOCALAPPDATA%\Dikit` as per-note JSON (`notes/<id>/note.json`), a `manifest.json`, `app-settings.json`, rolling per-note backups, and `backups/deleted`/`backups/quarantine` recovery folders. Accounts, cloud sync, collaboration, AI features, and telemetry are outside the MVP scope.

Before contributing, read the [PRD](docs/PRD/V1.0.md) and [development guide](docs/DEVELOPMENT_SETUP.md). Keep changes focused on the current milestone and document confirmed behavior. Commit source files and project configuration; leave generated `bin/` and `obj/` folders out of version control.

## Documentation

- [Product requirements](docs/PRD/V1.0.md): product direction, feature scope, architecture, and release criteria.
- [Development setup](docs/DEVELOPMENT_SETUP.md): prerequisites, commands, and troubleshooting history.
