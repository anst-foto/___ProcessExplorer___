using Avalonia;

using ReactiveUI.Avalonia;

using System;

namespace ProcessExplorer;

/// <summary>Содержит точку входа и конфигурацию Avalonia.</summary>
internal sealed class Program
{
    /// <summary>Запускает приложение с классическим desktop-жизненным циклом.</summary>
    /// <param name="args">Аргументы командной строки.</param>
    [STAThread]
    public static void Main(string[] args)
    {
        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    /// <summary>Настраивает приложение, платформу, шрифт Inter, трассировку и ReactiveUI.</summary>
    /// <returns>Конфигуратор приложения для запуска или инструментов Avalonia.</returns>
    public static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .UseReactiveUI(_ => { });
    }
}