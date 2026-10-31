# Dikit — Development Setup & Troubleshooting

> **Document type:** Developer onboarding and troubleshooting guide  
> **Project:** Dikit — offline Windows sticky-note application  
> **Platform:** Windows 11  
> **Stack:** C#, WPF/XAML, .NET 10, Visual Studio Code, Git  
> **Initial setup recorded:** October 31, 2026  
> **Status:** First WPF application successfully launched

## 1. Purpose

This document helps contributors prepare a local Dikit development environment, run the project, and resolve build issues already encountered. It records **observed symptoms and attempted commands**. Where the underlying cause was not confirmed, it is marked as unconfirmed rather than presented as fact.

For product requirements and planned functionality, see the project's PRD.

## 2. Prerequisites

| Tool                         | Purpose                                  | Notes                                                  |
| ---------------------------- | ---------------------------------------- | ------------------------------------------------------ |
| Windows 10/11                | WPF development and runtime              | WPF targets Windows. Windows 11 is our initial target. |
| Visual Studio Code           | Source editor                            | Full Visual Studio IDE is optional.                    |
| Git                          | Version control                          | Install and configure separately.                      |
| .NET **10 SDK**              | C# compiler, WPF templates, `dotnet` CLI | An SDK is required; a runtime alone is insufficient.   |
| **C#** extension (Microsoft) | Language assistance in VS Code           | Extension ID: `ms-dotnettools.csharp`.                 |

**Important:** Microsoft's _C# Dev Kit_ is not necessary for this WPF workflow. We use the base C# extension and .NET CLI to build and launch the app. Some Visual Studio graphical designers are not available in VS Code; WPF windows can be edited in XAML.

### Useful official links

- [.NET 10 SDK downloads](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)
- [Visual Studio Code](https://code.visualstudio.com/)
- [Microsoft C# extension for VS Code](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csharp)
- [Git for Windows](https://git-scm.com/downloads/win)
- [.NET CLI reference](https://learn.microsoft.com/en-us/dotnet/core/tools/)

## 3. Verify the development environment

Open **VS Code → Terminal → New Terminal** (PowerShell):

```powershell
dotnet --list-sdks
dotnet --version
dotnet --info
dotnet new list wpf
git --version
```

Expected outcomes:

- `.NET 10` appears in the installed SDKs. During initial Dikit setup, SDK **10.0.403** was observed in the build error path.
- `dotnet new list wpf` lists the **WPF Application** template.
- `git --version` prints an installed version.

If `dotnet` is not recognized after installation, restart the terminal or VS Code and try again. If only runtimes are installed, install the **SDK**.

> **Note on C# extension startup logs:** Messages such as `Razor extension startup finished` and `Finished discovery in 'c:\GitHub'` indicate that language services started. They do **not** by themselves prove that the WPF application compiles.

## 4. Create or open the Dikit project

The existing project directory used during setup is:

```text
C:\GitHub\dikit
```

If you already have the project, **do not generate a second project on top of it**. Open it directly:

```powershell
cd C:\GitHub\dikit
code .
```

For a **new project only**, the WPF template can be generated using:

```powershell
dotnet new wpf -n Dikit -f net10.0
```

This creates a WPF project for the Windows-targeted .NET framework. The project file should include properties equivalent to:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net10.0-windows</TargetFramework>
    <UseWPF>true</UseWPF>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
</Project>
```

The initial template normally includes `App.xaml`, `App.xaml.cs`, `MainWindow.xaml`, `MainWindow.xaml.cs`, and a `.csproj` file.

## 5. Build and run

From the project directory:

```powershell
cd C:\GitHub\dikit
dotnet restore
dotnet build
dotnet run
```

A successful build prints `Build succeeded` (with no build errors). A successful run opens the Dikit WPF window.

**Use `dotnet run --no-build` only when the executable has already been built successfully.** That flag skips the compile step entirely.

## 6. Troubleshooting history — October 31, 2026

### Issue A — `BG1002: MainWindow.baml cannot be found`

**Observed error:**

```text
C:\Program Files\dotnet\sdk\10.0.403\Sdks\Microsoft.NET.Sdk.WindowsDesktop\targets\Microsoft.WinFX.targets(707,5): error BG1002:
File 'C:\GitHub\dikit\obj\Debug\net10.0-windows\MainWindow.baml' cannot be found.

The build failed. Fix the build errors and run again.
```

**What this means:** WPF expected a compiled XAML resource (`MainWindow.baml`) during its build process, but that file was unavailable. Potential contributors include generated-file/build-state inconsistencies or XAML/project configuration problems. **The specific root cause in Dikit was not confirmed.**

**Troubleshooting commands attempted/recommended:**

```powershell
# Run in C:\GitHub\dikit
dotnet build-server shutdown
dotnet clean

# CAUTION: delete generated project folders only, not source files.
# Check the path before executing.
if (Test-Path .\bin) { Remove-Item -Recurse -Force .\bin }
if (Test-Path .\obj) { Remove-Item -Recurse -Force .\obj }

dotnet restore
dotnet build --disable-build-servers
```

If the problem persists, request a full non-incremental build:

```powershell
dotnet build --no-incremental --disable-build-servers
```

**Outcome:** The user subsequently confirmed Dikit now runs. A successful rebuild is established, but the conversation does not conclusively establish which cleanup/build command resolved `BG1002`.

### Issue B — `dotnet run --no-build` could not find `dikit.exe`

**Observed error:**

```text
Unhandled exception: An error occurred trying to start process
'C:\GitHub\dikit\bin\Debug\net10.0-windows\dikit.exe'
with working directory 'C:\GitHub\dikit'.
The system cannot find the file specified.
```

**Explanation:** `--no-build` assumes a runnable executable already exists. Because the earlier build had failed or build output had been removed, the executable was not present at the expected location.

**Resolution procedure:**

```powershell
cd C:\GitHub\dikit
dotnet build --no-incremental --disable-build-servers

# Only after the build succeeds:
dotnet run
```

**Lesson learned:** When a build fails, investigate the **first compilation error**. Do not use `--no-build` as a workaround for a missing executable.

## 7. Command reference

| Command                                                 | Why we use it                                                               |
| ------------------------------------------------------- | --------------------------------------------------------------------------- |
| `dotnet --list-sdks`                                    | Show installed .NET SDKs.                                                   |
| `dotnet --version`                                      | Show the active SDK version.                                                |
| `dotnet --info`                                         | Show SDK/runtime and OS information.                                        |
| `dotnet new list wpf`                                   | Verify that the WPF application template is available.                      |
| `dotnet new wpf -n Dikit -f net10.0`                    | Create a **new** WPF application (do not rerun inside an existing project). |
| `dotnet restore`                                        | Restore NuGet dependencies.                                                 |
| `dotnet build`                                          | Compile the project.                                                        |
| `dotnet run`                                            | Build (if needed) and start the app.                                        |
| `dotnet run --no-build`                                 | Run without compiling; requires existing successful build output.           |
| `dotnet clean`                                          | Remove project build outputs recorded by the build system.                  |
| `dotnet build-server shutdown`                          | Shut down persistent .NET/MSBuild build servers.                            |
| `dotnet build --no-incremental --disable-build-servers` | Perform a full build without using persistent build servers.                |

## 8. Git hygiene

Don't commit generated build output or local editor settings unnecessarily. A minimal `.gitignore` for the WPF project can include:

```gitignore
bin/
obj/
.vs/
.vscode/*.log
```

Keep the source code, `.csproj`, `.xaml`, `.cs`, documentation, and any intentionally shared editor configurations in version control. Be mindful not to commit personal notes, screenshots, credentials, or other private local app data.

## 9. Quick checklist before reporting a build problem

1. Confirm the terminal is in `C:\GitHub\dikit` (or the correct project directory).
2. Check `dotnet --version` and `dotnet new list wpf`.
3. Run `dotnet build` and copy the **first error**, plus a few lines around it.
4. If the issue is related to stale generated output, try the clean/rebuild sequence above.
5. Verify `Dikit.csproj`, `App.xaml`, and `MainWindow.xaml` if XAML compilation still fails.
6. Do not delete source files or reinstall the SDK until the error is understood.

## 10. Current status and next milestone

**Completed:** .NET 10 SDK recognized; C# tooling initialized; Dikit WPF project successfully launched.

**Next:** Build a single editable sticky note, save it locally, close Dikit, reopen it, and verify that its content remains intact.

---

**Documentation maintenance:** When a new issue occurs, add the date, exact error, affected environment, attempted commands, confirmed solution, and any remaining uncertainty. Update this document whenever the setup workflow changes.
