using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CommandDock.Domain.Entities;
using CommandDock.ViewModels;

namespace CommandDock;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;

    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;
        _viewModel.Output.CollectionChanged += Output_CollectionChanged;
        Loaded += async (_, _) => await _viewModel.LoadAsync();
    }

    private void Output_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add)
            OutputScroll.ScrollToEnd();
    }

    private void CommandsList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (_viewModel.RunCommandCommand.CanExecute(null))
            _viewModel.RunCommandCommand.Execute(null);
    }

    private void ItemMoreButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn) return;

        if (btn.DataContext is CommandDefinition cmd)
            _viewModel.SelectedCommand = cmd;

        if (btn.ContextMenu is { } menu)
        {
            menu.PlacementTarget = btn;
            menu.IsOpen = true;
        }
        e.Handled = true;
    }

    private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.F && Keyboard.Modifiers == ModifierKeys.Control)
        {
            SearchBox.Focus();
            SearchBox.SelectAll();
            e.Handled = true;
        }
    }

    private void SearchBox_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            _viewModel.SearchText = string.Empty;
            CommandsList.Focus();
            e.Handled = true;
        }
    }
}
