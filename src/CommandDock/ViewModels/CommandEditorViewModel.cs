using CommandDock.Domain.Entities;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CommandDock.ViewModels;

public partial class CommandEditorViewModel : ObservableObject
{
    private readonly CommandDefinition _model;

    public CommandEditorViewModel(CommandDefinition model)
    {
        _model = model;
        _name = model.Name;
        _icon = model.Icon;
        _description = model.Description;
        _commandText = model.CommandText;
    }

    [ObservableProperty]
    private string _name;

    [ObservableProperty]
    private string? _icon;

    [ObservableProperty]
    private string? _description;

    [ObservableProperty]
    private string _commandText;

    public string Title => _model.Id == default || string.IsNullOrEmpty(_model.Name)
        ? "New command"
        : "Edit command";

    public bool IsValid =>
        !string.IsNullOrWhiteSpace(Name) &&
        !string.IsNullOrWhiteSpace(CommandText);

    public void ApplyTo(CommandDefinition target)
    {
        target.Name = Name.Trim();
        target.Icon = string.IsNullOrWhiteSpace(Icon) ? null : Icon.Trim();
        target.Description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim();
        target.CommandText = CommandText;
    }
}
