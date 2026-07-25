# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build & Run

```powershell
# Build the solution
dotnet build CommandDock.slnx

# Run the WPF application
dotnet run --project src/CommandDock/CommandDock.csproj

# Build in release mode
dotnet build CommandDock.slnx -c Release
```

There are no automated tests in this repository.

## Architecture

CommandDock is a WPF/.NET 10 desktop app for managing and executing PowerShell commands. It follows Clean Architecture with four projects:

- **`CommandDock.Domain`** — Entities and value objects only; no external dependencies. Key types: `CommandDefinition` (entity), `OutputLine`, `ExecutionResult` (record structs).
- **`CommandDock.Application`** — Abstractions/contracts: `IRunner` (command execution) and `ICommandRepository` (persistence). No implementation.
- **`CommandDock.Infrastructure`** — Implements the application abstractions. `PowerShellRunner` shells out to `powershell.exe -EncodedCommand`, streaming output line-by-line via a callback. `CommandRepository` wraps EF Core with SQLite stored at `%LOCALAPPDATA%\CommandDock\commanddock.db`.
- **`CommandDock`** (WPF) — Presentation layer. `App.OnStartup()` wires up the DI container. ViewModels use CommunityToolkit.Mvvm.

Dependency direction: `CommandDock` → `CommandDock.Infrastructure` → `CommandDock.Application` → `CommandDock.Domain`.

## Key Patterns

- **DI composition root** is `App.OnStartup()` in `src/CommandDock/App.xaml.cs`.
- **ViewModels** (`MainViewModel`, `CommandEditorViewModel`) use `[ObservableProperty]` and `[RelayCommand]` from CommunityToolkit.Mvvm — avoid writing boilerplate property/command implementations manually.
- **`IRunner.ExecuteAsync`** accepts an `onOutput` callback (`Action<OutputLine>`) and a `CancellationToken`. Output is marshaled to the UI thread inside `MainViewModel`.
- **`IDbContextFactory<CommandDockDbContext>`** is used throughout Infrastructure (not `DbContext` directly) to support async/multi-operation scenarios.
- The database schema is bootstrapped with `EnsureCreated()` on startup; there is no Migrations setup — schema changes require manual column-addition handling (see existing `Icon` column migration code in `CommandDockDbContext`).
- **`IDialogService`** (implemented by `DialogService`) mediates all dialogs/confirmations from `MainViewModel` — never instantiate `CommandEditorDialog` or `MessageBox` directly in a ViewModel. Edit flow clones the selected `CommandDefinition`, edits the clone via the dialog, and only copies fields back onto the original (then persists) if the dialog returns `true` — this is what makes Cancel a no-op.

## UI Details

- Theme is defined in `src/CommandDock/Themes/DarkTheme.xaml`; all colors come from named resources there.
- Keyboard shortcuts are wired in `MainWindow.xaml` input bindings: Ctrl+N (new), Ctrl+E (edit), Ctrl+Return (run), Ctrl+. (stop), Ctrl+L (clear output), Ctrl+F (focus search).
- The output console uses a virtualized `ItemsControl` of `TextBlock`s — keep output items lightweight.
- `IconWithFallbackConverter` renders the emoji icon field; the default is 🔷.
