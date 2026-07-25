using System.Linq;
using System.Windows;
using CommandDock.Domain.Entities;
using CommandDock.ViewModels;
using CommandDock.Views;

namespace CommandDock.Services;

public sealed class DialogService : IDialogService
{
    public bool ShowCommandEditor(CommandDefinition command)
    {
        var vm = new CommandEditorViewModel(command);
        var dlg = new CommandEditorDialog(vm)
        {
            Owner = System.Windows.Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive),
        };
        var result = dlg.ShowDialog() == true;
        if (result) vm.ApplyTo(command);
        return result;
    }

    public bool Confirm(string message, string title) =>
        MessageBox.Show(System.Windows.Application.Current.MainWindow, message, title,
            MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK;

    public void ShowError(string message, string title) =>
        MessageBox.Show(System.Windows.Application.Current.MainWindow, message, title,
            MessageBoxButton.OK, MessageBoxImage.Error);
}
