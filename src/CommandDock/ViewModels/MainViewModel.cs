using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using CommandDock.Application.Abstractions;
using CommandDock.Domain.Entities;
using CommandDock.Domain.ValueObjects;
using CommandDock.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CommandDock.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly ICommandRepository _repository;
    private readonly IRunner _runner;
    private readonly IDialogService _dialogs;
    private CancellationTokenSource? _runCts;

    public MainViewModel(
        ICommandRepository repository,
        IRunner runner,
        IDialogService dialogs)
    {
        _repository = repository;
        _runner = runner;
        _dialogs = dialogs;

        CommandsView = CollectionViewSource.GetDefaultView(Commands);
        CommandsView.Filter = MatchesSearch;
        CommandsView.SortDescriptions.Add(new SortDescription(nameof(CommandDefinition.Name), ListSortDirection.Ascending));
    }

    public ObservableCollection<CommandDefinition> Commands { get; } = new();
    public ObservableCollection<OutputLine> Output { get; } = new();
    public ICollectionView CommandsView { get; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(EditCommandCommand))]
    [NotifyCanExecuteChangedFor(nameof(DeleteCommandCommand))]
    [NotifyCanExecuteChangedFor(nameof(RunCommandCommand))]
    private CommandDefinition? _selectedCommand;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RunCommandCommand))]
    [NotifyCanExecuteChangedFor(nameof(StopCommand))]
    private bool _isRunning;

    [ObservableProperty]
    private string _statusText = "Ready.";

    [ObservableProperty]
    private string _searchText = string.Empty;

    partial void OnSearchTextChanged(string value) => CommandsView.Refresh();

    private bool MatchesSearch(object item)
    {
        if (item is not CommandDefinition cmd) return false;
        if (string.IsNullOrWhiteSpace(SearchText)) return true;

        var q = SearchText.Trim();
        return Contains(cmd.Name, q) ||
               Contains(cmd.Description, q) ||
               Contains(cmd.CommandText, q);
    }

    private static bool Contains(string? source, string query) =>
        source is not null && source.Contains(query, StringComparison.OrdinalIgnoreCase);

    public async Task LoadAsync()
    {
        Commands.Clear();
        foreach (var c in await _repository.GetAllAsync())
            Commands.Add(c);
    }

    [RelayCommand]
    private async Task NewCommand()
    {
        var draft = new CommandDefinition { Name = string.Empty, CommandText = string.Empty };
        if (!_dialogs.ShowCommandEditor(draft)) return;

        try
        {
            await _repository.AddAsync(draft);
        }
        catch (Exception ex)
        {
            _dialogs.ShowError($"Could not save the new command:\n{ex.Message}", "Save failed");
            return;
        }

        Commands.Add(draft);
        SelectedCommand = draft;
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private async Task EditCommand()
    {
        if (SelectedCommand is null) return;

        var clone = new CommandDefinition
        {
            Id = SelectedCommand.Id,
            Name = SelectedCommand.Name,
            Icon = SelectedCommand.Icon,
            Description = SelectedCommand.Description,
            CommandText = SelectedCommand.CommandText,
            CreatedUtc = SelectedCommand.CreatedUtc,
        };
        if (!_dialogs.ShowCommandEditor(clone)) return;

        try
        {
            await _repository.UpdateAsync(clone);
        }
        catch (Exception ex)
        {
            _dialogs.ShowError($"Could not save the changes:\n{ex.Message}", "Save failed");
            return;
        }

        var index = Commands.IndexOf(SelectedCommand);
        if (index >= 0) Commands[index] = clone;
        SelectedCommand = clone;
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private async Task DeleteCommand()
    {
        if (SelectedCommand is null) return;
        if (!_dialogs.Confirm($"Delete \"{SelectedCommand.Name}\"?", "Confirm delete")) return;

        var id = SelectedCommand.Id;
        var toRemove = SelectedCommand;

        try
        {
            await _repository.DeleteAsync(id);
        }
        catch (Exception ex)
        {
            _dialogs.ShowError($"Could not delete the command:\n{ex.Message}", "Delete failed");
            return;
        }

        Commands.Remove(toRemove);
        SelectedCommand = null;
    }

    [RelayCommand(CanExecute = nameof(CanRun))]
    private async Task RunCommand()
    {
        if (SelectedCommand is null) return;

        Output.Clear();
        IsRunning = true;
        StatusText = $"Running \"{SelectedCommand.Name}\"...";
        _runCts = new CancellationTokenSource();

        var ui = SynchronizationContext.Current;
        void Append(OutputLine line)
        {
            if (ui is not null) ui.Post(_ => Output.Add(line), null);
            else Output.Add(line);
        }

        try
        {
            var result = await _runner.ExecuteAsync(SelectedCommand.CommandText, Append, _runCts.Token);
            StatusText = result.Cancelled
                ? $"Cancelled after {Format(result.Duration)}."
                : $"Exit code {result.ExitCode} in {Format(result.Duration)}.";
        }
        catch (Exception ex)
        {
            Append(new OutputLine(OutputStream.Stderr, ex.Message));
            StatusText = "Failed.";
        }
        finally
        {
            IsRunning = false;
            _runCts?.Dispose();
            _runCts = null;
        }
    }

    [RelayCommand(CanExecute = nameof(IsRunning))]
    private void Stop()
    {
        _runCts?.Cancel();
    }

    [RelayCommand]
    private void ClearOutput() => Output.Clear();

    [RelayCommand]
    private void ClearSearch() => SearchText = string.Empty;

    private bool HasSelection() => SelectedCommand is not null && !IsRunning;
    private bool CanRun() => SelectedCommand is not null && !IsRunning;

    private static string Format(TimeSpan ts) =>
        ts.TotalSeconds < 1
            ? $"{ts.TotalMilliseconds:F0} ms"
            : $"{ts.TotalSeconds:F2} s";
}
