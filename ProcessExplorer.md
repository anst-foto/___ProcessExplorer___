<!-- Solution2MD generated file -->
# Содержимое решения

- Решение: `ProcessExplorer.slnx`
- Проектов: 1
- Файлов в структуре: 21

## Проекты

- `ProcessExplorer/ProcessExplorer.csproj`

## Структура решения

```
ProcessExplorer/
.idea/
  .idea.ProcessExplorer/
    .idea/
      .gitignore
      avalonia.xml
      indexLayout.xml
      misc.xml
Directory.Packages.props
ProcessExplorer/
ProcessExplorer.slnx
  App.axaml
  App.axaml.cs
  app.manifest
  Assets/
    avalonia-logo.ico
  ProcessExplorer.csproj
  Program.cs
  ViewLocator.cs
  ViewModels/
    MainViewModel.cs
    ViewModelBase.cs
  Views/
    Controls/
      InfoField.axaml
      InfoField.axaml.cs
    MainWindow.axaml
    MainWindow.axaml.cs
    Styles/
      InfoFieldStyles.axaml
      MainWindowStyles.axaml
```

## Содержимое файлов

### `.idea/.idea.ProcessExplorer/.idea/.gitignore`

```text
# Default ignored files
/shelf/
/workspace.xml
# Editor-based HTTP Client requests
/httpRequests/
# Rider ignored files
/modules.xml
/contentModel.xml
/projectSettingsUpdater.xml
/.idea.ProcessExplorer.iml
# Ignored default folder with query files
/queries/
# Datasource local storage ignored files
/dataSources/
/dataSources.local.xml
```

### `.idea/.idea.ProcessExplorer/.idea/avalonia.xml`

```xml
<?xml version="1.0" encoding="UTF-8"?>
<project version="4">
  <component name="AvaloniaProject">
    <option name="projectPerEditor">
      <map>
        <entry key="ProcessExplorer/Views/Controls/InfoField.axaml" value="ProcessExplorer/ProcessExplorer.csproj" />
        <entry key="ProcessExplorer/Views/MainWindow.axaml" value="ProcessExplorer/ProcessExplorer.csproj" />
        <entry key="ProcessExplorer/Views/Styles/InfoFieldStyles.axaml" value="ProcessExplorer/ProcessExplorer.csproj" />
        <entry key="ProcessExplorer/Views/Styles/MainWindowStyles.axaml" value="ProcessExplorer/ProcessExplorer.csproj" />
      </map>
    </option>
  </component>
</project>
```

### `.idea/.idea.ProcessExplorer/.idea/indexLayout.xml`

```xml
<?xml version="1.0" encoding="UTF-8"?>
<project version="4">
  <component name="UserContentModel">
    <attachedFolders />
    <explicitIncludes />
    <explicitExcludes />
  </component>
</project>
```

### `.idea/.idea.ProcessExplorer/.idea/misc.xml`

```xml
<?xml version="1.0" encoding="UTF-8"?>
<project version="4">
  <component name="DebugServers">
    <config name="Config" />
  </component>
</project>
```

### `Directory.Packages.props`

```xml
<Project>
  <PropertyGroup>
    <!-- Enable central package management, https://learn.microsoft.com/en-us/nuget/consume-packages/Central-Package-Management -->
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>

    <AvaloniaVersion>12.1.5</AvaloniaVersion>
  </PropertyGroup>

  <ItemGroup>
    <PackageVersion Include="Avalonia" Version="$(AvaloniaVersion)" />
    <PackageVersion Include="Avalonia.Desktop" Version="$(AvaloniaVersion)" />
    <PackageVersion Include="Avalonia.Themes.Fluent" Version="$(AvaloniaVersion)" />
    <PackageVersion Include="Avalonia.Fonts.Inter" Version="$(AvaloniaVersion)" />

    <PackageVersion Include="ReactiveUI.Avalonia" Version="12.1.6" />
    <PackageVersion Include="ReactiveUI.SourceGenerators" Version="2.0.1" />

    <PackageVersion Include="MessageBox.Avalonia" Version="3.0.0" />
  </ItemGroup>
</Project>
```

### `ProcessExplorer.slnx`

```xml
<Solution>
  <Folder Name="/items/">
    <File Path="Directory.Packages.props" />
  </Folder>
  <Project Path="ProcessExplorer/ProcessExplorer.csproj" />
</Solution>
```

### `ProcessExplorer/App.axaml`

```text
<Application xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             x:Class="ProcessExplorer.App"
             xmlns:local="using:ProcessExplorer"
             RequestedThemeVariant="Default">
             <!-- "Default" ThemeVariant follows system theme variant. "Dark" or "Light" are other available options. -->

    <Application.DataTemplates>
        <local:ViewLocator />
    </Application.DataTemplates>

    <Application.Styles>
        <FluentTheme />
    </Application.Styles>
</Application>
```

### `ProcessExplorer/App.axaml.cs`

```csharp
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using ProcessExplorer.ViewModels;
using ProcessExplorer.Views;

namespace ProcessExplorer;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainViewModel(),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
```

### `ProcessExplorer/app.manifest`

```text
﻿<?xml version="1.0" encoding="utf-8"?>
<assembly manifestVersion="1.0" xmlns="urn:schemas-microsoft-com:asm.v1">
  <!-- This manifest is used on Windows only.
       Don't remove it as it might cause problems with window transparency and embedded controls.
       For more details visit https://learn.microsoft.com/en-us/windows/win32/sbscs/application-manifests -->
  <assemblyIdentity version="1.0.0.0" name="ProcessExplorer.Desktop"/>

  <compatibility xmlns="urn:schemas-microsoft-com:compatibility.v1">
    <application>
      <!-- A list of the Windows versions that this application has been tested on
           and is designed to work with. Uncomment the appropriate elements
           and Windows will automatically select the most compatible environment. -->

      <!-- Windows 10 -->
      <supportedOS Id="{8e0f7a12-bfb3-4fe8-b9a5-48fd50a15a9a}" />
    </application>
  </compatibility>
</assembly>
```

### `ProcessExplorer/Assets/avalonia-logo.ico`

> Пропущен бинарный файл (175 875 байт).

### `ProcessExplorer/ProcessExplorer.csproj`

```xml
﻿<Project Sdk="Microsoft.NET.Sdk">
    <PropertyGroup>
        <OutputType>WinExe</OutputType>
        <TargetFramework>net10.0</TargetFramework>
        <Nullable>enable</Nullable>
        <ImplicitUsings>disable</ImplicitUsings>
        <ApplicationManifest>app.manifest</ApplicationManifest>
    </PropertyGroup>

    <ItemGroup>
        <Folder Include="Models\"/>
        <AvaloniaResource Include="Assets\**"/>
    </ItemGroup>

    <ItemGroup>
        <PackageReference Include="Avalonia" />
        <PackageReference Include="Avalonia.Desktop" />
        <PackageReference Include="Avalonia.Themes.Fluent" />
        <PackageReference Include="Avalonia.Fonts.Inter" />

        <PackageReference Include="ReactiveUI.Avalonia" />
        <PackageReference Include="ReactiveUI.SourceGenerators" PrivateAssets="all" />

        <PackageReference Include="MessageBox.Avalonia" />
    </ItemGroup>
</Project>
```

### `ProcessExplorer/Program.cs`

```csharp
﻿using Avalonia;
using ReactiveUI.Avalonia;
using System;

namespace ProcessExplorer;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .UseReactiveUI(_ => { });
}
```

### `ProcessExplorer/ViewLocator.cs`

```csharp
using System;
using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using ProcessExplorer.ViewModels;

namespace ProcessExplorer;

/// <summary>
/// Given a view model, returns the corresponding view if possible.
/// </summary>
[RequiresUnreferencedCode(
    "Default implementation of ViewLocator involves reflection which may be trimmed away.",
    Url = "https://docs.avaloniaui.net/docs/concepts/view-locator")]
public class ViewLocator : IDataTemplate
{
    public Control? Build(object? param)
    {
        if (param is null)
            return null;

        var name = param.GetType().FullName!.Replace("ViewModel", "View", StringComparison.Ordinal);
        var type = Type.GetType(name);

        if (type != null)
        {
            return (Control)Activator.CreateInstance(type)!;
        }

        return new TextBlock { Text = "Not Found: " + name };
    }

    public bool Match(object? data)
    {
        return data is ViewModelBase;
    }
}
```

### `ProcessExplorer/ViewModels/MainViewModel.cs`

```csharp
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace ProcessExplorer.ViewModels;

public sealed partial class MainViewModel : ViewModelBase
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    private readonly DispatcherTimer _refreshTimer;
    private ProcessListItem? _selectedProcess;

    // ---- Взаимодействия с UI ----
    public Interaction<string, IStorageFile?> SaveFileInteraction { get; } = new();
    public Interaction<KillConfirmationRequest, bool> KillConfirmationInteraction { get; } = new();
    public Interaction<MessageBoxRequest, Unit> ShowMessageInteraction { get; } = new();

    // ---- Генерируемые свойства ----
    [Reactive]
    public partial string Status { get; private set; } = "";

    [Reactive]
    public partial ProcessDetails SelectedDetails { get; private set; } = ProcessDetails.Empty;

    // SelectedProcess оставлен ручным: при установке нужна доп. логика
    public ProcessListItem? SelectedProcess
    {
        get => _selectedProcess;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedProcess, value);
            this.RaisePropertyChanged(nameof(HasSelection));
            this.RaisePropertyChanged(nameof(HasNoSelection));
            LoadSelectedDetails();
        }
    }

    public ObservableCollection<ProcessListItem> Processes { get; } = new();

    public bool HasSelection => SelectedProcess is not null;
    public bool HasNoSelection => SelectedProcess is null;

    public MainViewModel()
    {
        RefreshProcesses();
        _refreshTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
        _refreshTimer.Tick += (_, _) => RefreshProcesses();
        _refreshTimer.Start();
    }

    public void SetStatus(string message) => Status = message;

    public void RefreshProcesses()
    {
        var selectedId = SelectedProcess?.Id;
        var items = Process.GetProcesses()
            .Select(process =>
            {
                try
                {
                    return new ProcessListItem(process.Id, process.ProcessName);
                }
                catch
                {
                    return null;
                }
                finally
                {
                    process.Dispose();
                }
            })
            .Where(item => item is not null)
            .Select(item => item!)
            .OrderBy(item => item.Name, StringComparer.CurrentCultureIgnoreCase)
            .ThenBy(item => item.Id)
            .ToArray();

        Processes.Clear();
        foreach (var item in items)
            Processes.Add(item);

        SelectedProcess = selectedId is int id
            ? Processes.FirstOrDefault(item => item.Id == id)
            : null;

        Status = $"Процессов: {Processes.Count}";
    }

    private void LoadSelectedDetails()
    {
        if (SelectedProcess is null)
        {
            SelectedDetails = ProcessDetails.Empty;
            return;
        }

        try
        {
            using var process = Process.GetProcessById(SelectedProcess.Id);
            SelectedDetails = ProcessDetails.FromProcess(process);
        }
        catch (Exception ex)
        {
            SelectedDetails = ProcessDetails.Empty with
            {
                Name = SelectedProcess.Name,
                Id = SelectedProcess.Id.ToString(),
                AvailabilityNote = $"Не удалось получить подробности: {ex.Message}"
            };
        }
    }

    private Task ShowMessageAsync(string title, string message) =>
        ShowMessageInteraction.Handle(new MessageBoxRequest(title, message));

    // ---- Генерируемые команды ----
    [ReactiveCommand(CanExecute = nameof(HasSelection))]
    private async Task SaveSelectedProcess()
    {
        var selected = SelectedProcess;
        if (selected is null) return;
        var details = SelectedDetails;

        var file = await SaveFileInteraction.Handle($"process-{selected.Id}.json");
        if (file is null) return;

        try
        {
            await using var stream = await file.OpenWriteAsync();
            await JsonSerializer.SerializeAsync(stream, details, JsonOptions);
            SetStatus($"Информация о процессе сохранена: {file.Name}");
            await ShowMessageAsync(
                "Сохранение выполнено",
                $"Сведения о процессе «{details.Name}» сохранены в файл:\n{file.Name}");
        }
        catch (Exception ex)
        {
            SetStatus($"Ошибка сохранения JSON: {ex.Message}");
            await ShowMessageAsync(
                "Ошибка сохранения",
                $"Не удалось сохранить JSON:\n{ex.Message}");
        }
    }

    [ReactiveCommand]
    private async Task SaveAllProcesses()
    {
        var file = await SaveFileInteraction.Handle("processes.json");
        if (file is null) return;

        try
        {
            var snapshot = await Task.Run(() =>
                Processes.Select(item =>
                {
                    try
                    {
                        using var process = Process.GetProcessById(item.Id);
                        return ProcessDetails.FromProcess(process);
                    }
                    catch (Exception ex)
                    {
                        return ProcessDetails.Empty with
                        {
                            Name = item.Name,
                            Id = item.Id.ToString(),
                            AvailabilityNote = $"Процесс завершился или сведения недоступны: {ex.Message}"
                        };
                    }
                }).ToArray());

            await using var stream = await file.OpenWriteAsync();
            await JsonSerializer.SerializeAsync(stream, snapshot, JsonOptions);
            SetStatus($"Информация о процессах сохранена: {file.Name}");
            await ShowMessageAsync(
                "Сохранение выполнено",
                $"Сведения обо всех процессах ({snapshot.Length}) сохранены в файл:\n{file.Name}");
        }
        catch (Exception ex)
        {
            SetStatus($"Ошибка сохранения JSON: {ex.Message}");
            await ShowMessageAsync(
                "Ошибка сохранения",
                $"Не удалось сохранить JSON:\n{ex.Message}");
        }
    }

    [ReactiveCommand(CanExecute = nameof(HasSelection))]
    private async Task KillSelectedProcess()
    {
        var selected = SelectedProcess;
        if (selected is null) return;

        var confirmed = await KillConfirmationInteraction.Handle(
            new KillConfirmationRequest(selected.Name, selected.Id));
        if (!confirmed) return;

        try
        {
            using var process = Process.GetProcessById(selected.Id);
            if (!string.Equals(process.ProcessName, selected.Name, StringComparison.OrdinalIgnoreCase))
            {
                SetStatus("Процесс с выбранным ID изменился; завершение отменено.");
                await ShowMessageAsync(
                    "Завершение отменено",
                    "Процесс с выбранным ID изменился. Список процессов будет обновлён.");
                RefreshProcesses();
                return;
            }

            process.Kill();
            await process.WaitForExitAsync();
            SetStatus($"Процесс {selected.Name} (ID {selected.Id}) завершён.");
            RefreshProcesses();
        }
        catch (Exception ex)
        {
            SetStatus($"Не удалось завершить процесс: {ex.Message}");
            await ShowMessageAsync(
                "Ошибка завершения",
                $"Не удалось завершить процесс «{selected.Name}» (ID {selected.Id}):\n{ex.Message}");
        }
    }
}

public sealed record KillConfirmationRequest(string ProcessName, int ProcessId);

public sealed record MessageBoxRequest(string Title, string Message);

public sealed record ProcessListItem(int Id, string Name)
{
    public string DisplayName => $"{Name}  ({Id})";
}

public sealed record ProcessDetails
{
    public static ProcessDetails Empty { get; } = new();

    public string Name { get; init; } = "—";
    public string Id { get; init; } = "—";
    public string MainWindowTitle { get; init; } = "—";
    public string Responding { get; init; } = "—";
    public string StartTime { get; init; } = "—";
    public string TotalProcessorTime { get; init; } = "—";
    public string PriorityClass { get; init; } = "—";
    public string BasePriority { get; init; } = "—";
    public string SessionId { get; init; } = "—";
    public string HandleCount { get; init; } = "—";
    public string ThreadCount { get; init; } = "—";
    public string WorkingSet { get; init; } = "—";
    public string PrivateMemory { get; init; } = "—";
    public string VirtualMemory { get; init; } = "—";
    public string PagedMemory { get; init; } = "—";
    public string MainModule { get; init; } = "—";
    public string ModuleList { get; init; } = "—";
    public string ThreadList { get; init; } = "—";
    public string AvailabilityNote { get; init; } = "";

    public static ProcessDetails FromProcess(Process process)
    {
        static string Read<T>(Func<T> read, Func<T, string>? format = null)
        {
            try
            {
                var value = read();
                if (value is null) return "—";
                return format is null ? value.ToString() ?? "—" : format(value);
            }
            catch
            {
                return "Нет доступа или данные недоступны";
            }
        }

        static string Bytes(long value) => $"{value / 1024d / 1024d:N1} МБ ({value:N0} байт)";

        var modules = Read(() => string.Join(Environment.NewLine,
            process.Modules.Cast<ProcessModule>()
                .Select(module => $"{module.ModuleName} — {module.FileName}")));
        var threads = Read(() => string.Join(Environment.NewLine,
            process.Threads.Cast<ProcessThread>()
                .Select(thread => $"ID {thread.Id}: {thread.ThreadState}")));

        var mainModule = Read(() =>
        {
            var module = process.MainModule;
            if (module is null) return "—";
            return string.Join(Environment.NewLine, new[]
            {
                $"Имя: {module.ModuleName}",
                $"Путь: {module.FileName}",
                $"Версия файла: {module.FileVersionInfo.FileVersion ?? "—"}",
                $"Описание: {module.FileVersionInfo.FileDescription ?? "—"}"
            });
        });

        return new ProcessDetails
        {
            Name = Read(() => process.ProcessName),
            Id = Read(() => process.Id),
            MainWindowTitle = Read(() => process.MainWindowTitle),
            Responding = Read(() => process.Responding, value => value ? "Да" : "Нет"),
            StartTime = Read(() => process.StartTime, value => value.ToString("G")),
            TotalProcessorTime = Read(() => process.TotalProcessorTime),
            PriorityClass = Read(() => process.PriorityClass),
            BasePriority = Read(() => process.BasePriority),
            SessionId = Read(() => process.SessionId),
            HandleCount = Read(() => process.HandleCount),
            ThreadCount = Read(() => process.Threads.Count),
            WorkingSet = Read(() => process.WorkingSet64, Bytes),
            PrivateMemory = Read(() => process.PrivateMemorySize64, Bytes),
            VirtualMemory = Read(() => process.VirtualMemorySize64, Bytes),
            PagedMemory = Read(() => process.PagedMemorySize64, Bytes),
            MainModule = mainModule,
            ModuleList = modules,
            ThreadList = threads
        };
    }
}
```

### `ProcessExplorer/ViewModels/ViewModelBase.cs`

```csharp
﻿using ReactiveUI;

namespace ProcessExplorer.ViewModels;

public abstract class ViewModelBase : ReactiveObject
{
}
```

### `ProcessExplorer/Views/Controls/InfoField.axaml`

```text
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             x:Class="ProcessExplorer.Views.Controls.InfoField"
             x:Name="Root">
    <UserControl.Styles>
        <StyleInclude Source="avares://ProcessExplorer/Views/Styles/InfoFieldStyles.axaml" />
    </UserControl.Styles>

    <Grid ColumnDefinitions="180,*" ColumnSpacing="8">
        <Label Grid.Column="0"
               Content="{Binding LabelText, ElementName=Root}"
               Target="{Binding #ValueTextBox}"
               Classes="info-field-label" />
        <TextBox x:Name="ValueTextBox"
                 Grid.Column="1"
                 Text="{Binding Value, ElementName=Root}"
                 IsEnabled="False"
                 IsReadOnly="True"
                 TextWrapping="Wrap"
                 AcceptsReturn="True"
                 Classes="info-field-value" />
    </Grid>
</UserControl>
```

### `ProcessExplorer/Views/Controls/InfoField.axaml.cs`

```csharp
using Avalonia;
using Avalonia.Controls;

namespace ProcessExplorer.Views.Controls;

public partial class InfoField : UserControl
{
    public static readonly StyledProperty<string?> LabelTextProperty =
        AvaloniaProperty.Register<InfoField, string?>(nameof(LabelText));

    public static readonly StyledProperty<string?> ValueProperty =
        AvaloniaProperty.Register<InfoField, string?>(nameof(Value));

    public string? LabelText
    {
        get => GetValue(LabelTextProperty);
        set => SetValue(LabelTextProperty, value);
    }

    public string? Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public InfoField()
    {
        InitializeComponent();
    }
}
```

### `ProcessExplorer/Views/MainWindow.axaml`

```text
<Window xmlns="https://github.com/avaloniaui"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:vm="using:ProcessExplorer.ViewModels"
        xmlns:controls="using:ProcessExplorer.Views.Controls"
        xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
        xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
        mc:Ignorable="d"
        d:DesignWidth="1100" d:DesignHeight="700"
        x:Class="ProcessExplorer.Views.MainWindow"
        x:DataType="vm:MainViewModel"
        Icon="/Assets/avalonia-logo.ico"
        Title="Process Explorer"
        Width="1100" Height="700"
        MinWidth="800" MinHeight="500">

    <Window.Styles>
        <StyleInclude Source="avares://ProcessExplorer/Views/Styles/MainWindowStyles.axaml" />
    </Window.Styles>

    <Design.DataContext>
        <vm:MainViewModel />
    </Design.DataContext>

    <Grid ColumnDefinitions="300,*" RowDefinitions="Auto,*,Auto" Margin="12">
        <Menu Grid.Row="0" Grid.Column="0" Grid.ColumnSpan="2">
            <MenuItem Header="Процесс">
                <MenuItem Header="Завершить выбранный процесс…"
                          Command="{Binding KillSelectedProcessCommand}" />
                <Separator />
                <MenuItem Header="Сохранить сведения выбранного процесса в JSON…"
                          Command="{Binding SaveSelectedProcessCommand}" />
                <MenuItem Header="Сохранить сведения обо всех процессах в JSON…"
                          Command="{Binding SaveAllProcessesCommand}" />
            </MenuItem>
        </Menu>

        <Border Grid.Row="1" Grid.Column="0" Classes="process-list-panel">
            <DockPanel>
                <TextBlock DockPanel.Dock="Top" Text="Запущенные процессы"
                           Classes="window-section-title" />
                <ListBox ItemsSource="{Binding Processes}"
                         SelectedItem="{Binding SelectedProcess, Mode=TwoWay}">
                    <ListBox.ItemTemplate>
                        <DataTemplate x:DataType="vm:ProcessListItem">
                            <Grid ColumnDefinitions="*,Auto" Classes="process-list-item">
                                <TextBlock Grid.Column="0" Text="{Binding Name}"
                                           TextTrimming="CharacterEllipsis" />
                                <TextBlock Grid.Column="1" Text="{Binding Id}"
                                           Classes="process-id" />
                            </Grid>
                        </DataTemplate>
                    </ListBox.ItemTemplate>
                </ListBox>
            </DockPanel>
        </Border>

        <Border Grid.Row="1" Grid.Column="1" Classes="process-details-panel">
            <ScrollViewer VerticalScrollBarVisibility="Auto">
                <StackPanel Spacing="8">
                    <TextBlock Text="Информация о процессе" Classes="details-title" />
                    <TextBlock Text="Выберите процесс в списке слева."
                               IsVisible="{Binding HasNoSelection}" />
                    <StackPanel Spacing="6" IsVisible="{Binding HasSelection}">
                        <controls:InfoField LabelText="Имя" Value="{Binding SelectedDetails.Name}" />
                        <controls:InfoField LabelText="ID" Value="{Binding SelectedDetails.Id}" />
                        <controls:InfoField LabelText="Заголовок окна" Value="{Binding SelectedDetails.MainWindowTitle}" />
                        <controls:InfoField LabelText="Отвечает" Value="{Binding SelectedDetails.Responding}" />
                        <controls:InfoField LabelText="Время запуска" Value="{Binding SelectedDetails.StartTime}" />
                        <controls:InfoField LabelText="CPU-время" Value="{Binding SelectedDetails.TotalProcessorTime}" />
                        <controls:InfoField LabelText="Класс приоритета" Value="{Binding SelectedDetails.PriorityClass}" />
                        <controls:InfoField LabelText="Базовый приоритет" Value="{Binding SelectedDetails.BasePriority}" />
                        <controls:InfoField LabelText="Сеанс" Value="{Binding SelectedDetails.SessionId}" />
                        <controls:InfoField LabelText="Дескрипторы" Value="{Binding SelectedDetails.HandleCount}" />
                        <controls:InfoField LabelText="Потоки" Value="{Binding SelectedDetails.ThreadCount}" />
                        <controls:InfoField LabelText="Рабочий набор" Value="{Binding SelectedDetails.WorkingSet}" />
                        <controls:InfoField LabelText="Частная память" Value="{Binding SelectedDetails.PrivateMemory}" />
                        <controls:InfoField LabelText="Виртуальная память" Value="{Binding SelectedDetails.VirtualMemory}" />
                        <controls:InfoField LabelText="Paged-память" Value="{Binding SelectedDetails.PagedMemory}" />
                        <controls:InfoField LabelText="Основной модуль" Value="{Binding SelectedDetails.MainModule}" />
                        <controls:InfoField LabelText="Модули" Value="{Binding SelectedDetails.ModuleList}" />
                        <controls:InfoField LabelText="Сведения о потоках" Value="{Binding SelectedDetails.ThreadList}" />
                        <controls:InfoField LabelText="Состояние доступа к информации"
                                            Value="{Binding SelectedDetails.AvailabilityNote}" />
                    </StackPanel>
                </StackPanel>
            </ScrollViewer>
        </Border>

        <TextBlock Grid.Row="2" Grid.Column="0" Grid.ColumnSpan="2"
                   Text="{Binding Status}" Classes="status-line" />
    </Grid>
</Window>
```

### `ProcessExplorer/Views/MainWindow.axaml.cs`

```csharp
using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using ProcessExplorer.ViewModels;

namespace ProcessExplorer.Views;

public partial class MainWindow : Window
{
    private MainViewModel? _registeredViewModel;

    public MainWindow()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is MainViewModel vm && vm != _registeredViewModel)
        {
            _registeredViewModel = vm;

            vm.SaveFileInteraction.RegisterHandler(async ctx =>
            {
                ctx.SetOutput(await PickJsonFileAsync(ctx.Input));
            });

            vm.ShowMessageInteraction.RegisterHandler(async ctx =>
            {
                var box = MessageBoxManager.GetMessageBoxStandard(
                    ctx.Input.Title,
                    ctx.Input.Message,
                    ButtonEnum.Ok);
                await box.ShowAsync(this);
                ctx.SetOutput(System.Reactive.Unit.Default);
            });

            vm.KillConfirmationInteraction.RegisterHandler(async ctx =>
            {
                var box = MessageBoxManager.GetMessageBoxStandard(
                    "Подтверждение завершения",
                    $"Завершить процесс «{ctx.Input.ProcessName}» (ID {ctx.Input.ProcessId})? " +
                    "Несохранённые данные приложения могут быть потеряны.",
                    ButtonEnum.YesNo);
                var result = await box.ShowAsync(this);
                ctx.SetOutput(result == ButtonResult.Yes);
            });
        }
    }

    private async Task<IStorageFile?> PickJsonFileAsync(string suggestedName)
    {
        return await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Сохранить сведения о процессах",
            SuggestedFileName = suggestedName,
            DefaultExtension = "json",
            FileTypeChoices = new[]
            {
                new FilePickerFileType("JSON") { Patterns = new[] { "*.json" } }
            }
        });
    }
}
```

### `ProcessExplorer/Views/Styles/InfoFieldStyles.axaml`

```text
<Styles xmlns="https://github.com/avaloniaui">
    <Style Selector="Label.info-field-label">
        <Setter Property="VerticalAlignment" Value="Center" />
        <Setter Property="HorizontalContentAlignment" Value="Left" />
        <Setter Property="Margin" Value="0,2,4,2" />
    </Style>

    <Style Selector="TextBox.info-field-value">
        <Setter Property="MinHeight" Value="32" />
        <Setter Property="TextWrapping" Value="Wrap" />
        <Setter Property="AcceptsReturn" Value="True" />
        <Setter Property="IsEnabled" Value="False" />
        <Setter Property="IsReadOnly" Value="True" />
    </Style>
</Styles>
```

### `ProcessExplorer/Views/Styles/MainWindowStyles.axaml`

```text
<Styles xmlns="https://github.com/avaloniaui">
    <Style Selector="Border.process-list-panel">
        <Setter Property="BorderBrush" Value="#808080" />
        <Setter Property="BorderThickness" Value="1" />
        <Setter Property="CornerRadius" Value="4" />
    </Style>

    <Style Selector="Border.process-details-panel">
        <Setter Property="BorderBrush" Value="#808080" />
        <Setter Property="BorderThickness" Value="1" />
        <Setter Property="CornerRadius" Value="4" />
        <Setter Property="Margin" Value="12,0,0,0" />
        <Setter Property="Padding" Value="14" />
    </Style>

    <Style Selector="TextBlock.window-section-title">
        <Setter Property="FontSize" Value="18" />
        <Setter Property="FontWeight" Value="SemiBold" />
        <Setter Property="Margin" Value="10" />
    </Style>

    <Style Selector="TextBlock.details-title">
        <Setter Property="FontSize" Value="20" />
        <Setter Property="FontWeight" Value="SemiBold" />
    </Style>

    <Style Selector="Grid.process-list-item">
        <Setter Property="Margin" Value="6,3" />
    </Style>

    <Style Selector="TextBlock.process-id">
        <Setter Property="Margin" Value="8,0,0,0" />
        <Setter Property="Opacity" Value="0.7" />
    </Style>

    <Style Selector="TextBlock.status-line">
        <Setter Property="Margin" Value="4,10,0,0" />
        <Setter Property="Opacity" Value="0.7" />
    </Style>
</Styles>
```
