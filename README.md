# Dikit

*Stick your thoughts, your way.*

Dikit is a Windows desktop app being built for personal sticky notes and screenshot references. The goal is to keep ideas, images, links, and checklists in customizable note windows, saved locally and ready when you return.

Designed for students, developers, and anyone who wants quick references beside their work. Core workflows are planned to work offline, without accounts, cloud storage, or a background server.

## Project status

**Early development.** The .NET 10 WPF project is set up and the starter window launches. Note editing, storage, and the features below are planned; they are not available yet.

## Planned features

- Create, edit, duplicate, and delete independent, movable note windows.
- Mix text, pasted screenshots, website links, and checklists.
- Save automatically and restore note content, size, position, and appearance.
- Choose theme presets or customize colors, fonts, corners, and opacity per note.
- Pin notes above other windows and manage them from the system tray.
- Opt in to launch at Windows sign-in.
- Keep notes and image assets on-device, with local backups and recovery.

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
| M1: Notes and persistence | Editable notes, local JSON storage, autosave, and window restoration | Next |
| M2: Content blocks | Pasted images, safe website links, and checklists | Planned |
| M3: Theme Studio | Presets, live preview, and per-note customization | Planned |
| M4: Desktop integration and release | Tray controls, opt-in startup, recovery, and Windows distribution | Planned |

## Development

Dikit uses **C#, WPF/XAML, and .NET 10**. The planned architecture uses MVVM to separate interface and application state, with services for persistence, themes, assets, and desktop integration. Build these layers incrementally as features need them.

The proposed storage location is `%LOCALAPPDATA%\Dikit`, using JSON note records and local image assets. Persistence is not implemented yet. Accounts, cloud sync, collaboration, AI features, and telemetry are outside the MVP scope.

Before contributing, read the [PRD](docs/PRD/V1.0.md) and [development guide](docs/DEVELOPMENT_SETUP.md). Keep changes focused on the current milestone and document confirmed behavior. Commit source files and project configuration; leave generated `bin/` and `obj/` folders out of version control.

## Documentation

- [Product requirements](docs/PRD/V1.0.md): product direction, feature scope, architecture, and release criteria.
- [Development setup](docs/DEVELOPMENT_SETUP.md): prerequisites, commands, and troubleshooting history.
