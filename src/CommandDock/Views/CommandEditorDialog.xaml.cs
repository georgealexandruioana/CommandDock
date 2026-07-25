using System.Windows;
using CommandDock.ViewModels;

namespace CommandDock.Views;

public partial class CommandEditorDialog : Window
{
    private readonly CommandEditorViewModel _viewModel;

    public CommandEditorDialog(CommandEditorViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
        Loaded += (_, _) => NameBox.Focus();
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (!_viewModel.IsValid)
        {
            MessageBox.Show(this, "Name and command are required.", "Validation",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        DialogResult = true;
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
