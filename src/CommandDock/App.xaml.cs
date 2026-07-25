using System;
using System.Windows;
using CommandDock.Application.Abstractions;
using CommandDock.Infrastructure.Persistence;
using CommandDock.Infrastructure.PowerShell;
using CommandDock.Services;
using CommandDock.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace CommandDock;

public partial class App : System.Windows.Application
{
    public IServiceProvider Services { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        DispatcherUnhandledException += OnDispatcherUnhandledException;

        var services = new ServiceCollection();
        services.AddDbContextFactory<CommandDockDbContext>();
        services.AddSingleton<ICommandRepository, CommandRepository>();
        services.AddSingleton<IDatabaseInitializer, SqliteDatabaseInitializer>();
        services.AddSingleton<IRunner, PowerShellRunner>();
        services.AddSingleton<IDialogService, DialogService>();
        services.AddSingleton<MainViewModel>();
        services.AddSingleton<MainWindow>();
        Services = services.BuildServiceProvider();

        try
        {
            Services.GetRequiredService<IDatabaseInitializer>().EnsureCreated();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"CommandDock could not open its database:\n{ex.Message}\n\nThe app will now close.",
                "Startup failed", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown(-1);
            return;
        }

        var main = Services.GetRequiredService<MainWindow>();
        main.Show();
    }

    private void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
        MessageBox.Show(
            $"An unexpected error occurred:\n{e.Exception.Message}",
            "CommandDock", MessageBoxButton.OK, MessageBoxImage.Error);
        e.Handled = true;
    }
}
