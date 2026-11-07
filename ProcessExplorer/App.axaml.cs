using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

using ProcessExplorer.Services;
using ProcessExplorer.ViewModels;
using ProcessExplorer.Views;

namespace ProcessExplorer;

/// <summary>Загружает ресурсы приложения и создаёт главное окно desktop-жизненного цикла.</summary>
public partial class App : Application
{
    /// <summary>Загружает XAML-ресурсы приложения.</summary>
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    /// <summary>Для desktop-жизненного цикла создаёт окно, сервис и модель представления.</summary>
    /// <remarks>После назначения главного окна вызывает базовую реализацию; чтение процессов запускается при открытии окна.</remarks>
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Ручное constructor injection; контейнер DI не нужен.
            desktop.MainWindow = new MainWindow { DataContext = new MainViewModel(new ProcessService()) };
        }

        base.OnFrameworkInitializationCompleted();
    }
}