using CommandDock.Domain.Entities;

namespace CommandDock.Services;

public interface IDialogService
{
    bool ShowCommandEditor(CommandDefinition command);
    bool Confirm(string message, string title);
    void ShowError(string message, string title);
}
