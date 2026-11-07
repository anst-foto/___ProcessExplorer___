using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Threading;

using ProcessExplorer.Models;
using ProcessExplorer.ViewModels;

using ReactiveUI.Binding;

namespace ProcessExplorer.Views;

// Только UI interactions, работа с файлом провайдера и lifecycle; операций ОС здесь нет.
/// <summary>Размещает интерфейс, обслуживает взаимодействия и владеет назначенной моделью представления.</summary>
public partial class MainWindow : Window
{
    /// <summary>Регистрации обработчиков взаимодействий текущей модели.</summary>
    private readonly List<IDisposable> _registrations = new();

    /// <summary>Модель представления, для которой зарегистрированы обработчики.</summary>
    private MainViewModel? _registeredViewModel;

    /// <summary>Признак закрытого окна.</summary>
    private bool _closed;

    /// <summary>Загружает разметку и подключает обработчики контекста и открытия окна.</summary>
    public MainWindow()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
        Opened += OnOpened;
    }

    /// <summary>Запускает зарегистрированную модель при открытии окна.</summary>
    /// <param name="sender">Источник события.</param>
    /// <param name="e">Данные события открытия.</param>
    private void OnOpened(object? sender, EventArgs e)
    {
        _registeredViewModel?.Start();
    }

    /// <summary>Отсоединяет прежнюю модель и регистрирует взаимодействия новой.</summary>
    /// <param name="sender">Источник события.</param>
    /// <param name="e">Данные события смены контекста.</param>
    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (_closed || ReferenceEquals(DataContext, _registeredViewModel))
        {
            return;
        }

        DetachViewModel();
        if (DataContext is not MainViewModel vm)
        {
            return;
        }

        _registeredViewModel = vm;
        _registrations.Add(vm.SaveJsonInteraction.RegisterHandler(HandleSaveAsync));
        _registrations.Add(vm.KillConfirmationInteraction.RegisterHandler(HandleKillConfirmationAsync));
        if (IsVisible)
        {
            vm.Start();
        }
    }

    /// <summary>Снимает регистрации взаимодействий и освобождает прежнюю модель представления.</summary>
    private void DetachViewModel()
    {
        foreach (IDisposable registration in _registrations)
        {
            registration.Dispose();
        }

        _registrations.Clear();
        MainViewModel? previous = _registeredViewModel;
        _registeredViewModel = null;
        previous?.Dispose(); // Окно владеет назначенной ему VM.
    }

    /// <summary>Отсоединяет модель и обработчики событий перед базовой обработкой закрытия.</summary>
    /// <param name="e">Данные события закрытия.</param>
    protected override void OnClosed(EventArgs e)
    {
        _closed = true;
        DataContextChanged -= OnDataContextChanged;
        Opened -= OnOpened;
        DetachViewModel();
        base.OnClosed(e);
    }

    /// <summary>Показывает выбор файла и записывает готовый JSON в UTF-8.</summary>
    /// <param name="context">Контекст запроса и результата сохранения.</param>
    /// <returns>Задача записи и публикации результата после освобождения ресурсов.</returns>
    /// <remarks>Нативный диалог не получает токен отмены. Запись не транзакционная; после её начала ошибка или отмена могут оставить неполный файл.</remarks>
    /// <exception cref="OperationCanceledException">Запрошена отмена или окно закрыто.</exception>
    /// <exception cref="IOException">Выбор файла, запись либо освобождение ресурсов завершились ошибкой.</exception>
    private async Task HandleSaveAsync(IInteractionContext<SaveJsonRequest, SaveJsonResult> context)
    {
        SaveJsonRequest request = context.Input;
        CancellationToken token = request.CancellationToken;
        try
        {
            token.ThrowIfCancellationRequested();
            if (_closed)
            {
                throw new OperationCanceledException(token);
            }

            if (!StorageProvider.CanSave)
            {
                throw new NotSupportedException("Сохранение файлов недоступно на этой платформе.");
            }

            SaveJsonResult result;
            // Нативный picker не принимает CancellationToken.
            using (IStorageFile? file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
                   {
                       Title = "Сохранить сведения о процессах",
                       SuggestedFileName = request.SuggestedName,
                       DefaultExtension = "json",
                       FileTypeChoices = new[] { new FilePickerFileType("JSON") { Patterns = new[] { "*.json" } } }
                   }))
            {
                token.ThrowIfCancellationRequested();
                if (_closed)
                {
                    throw new OperationCanceledException(token);
                }

                if (file is null)
                {
                    result = new SaveJsonResult(false, true, null);
                }
                else
                {
                    byte[] bytes = await Task.Run(() => Encoding.UTF8.GetBytes(request.Json), token);
                    token.ThrowIfCancellationRequested();
                    // DisposeAsync завершается ДО SetOutput(success).
                    await using (Stream stream = await file.OpenWriteAsync())
                    {
                        token.ThrowIfCancellationRequested();
                        if (stream.CanSeek)
                        {
                            stream.Position = 0;
                            stream.SetLength(0);
                        }

                        await stream.WriteAsync(bytes.AsMemory(), token);
                        await stream.FlushAsync(token);
                    }

                    token.ThrowIfCancellationRequested();
                    result = new SaveJsonResult(true, false, file.Name);
                }
            }

            context.SetOutput(result); // И stream, и IStorageFile уже освобождены.
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex)
        {
            throw new IOException(
                "Не удалось сохранить JSON. После начала записи файл мог остаться неполным: " + ex.Message, ex);
        }
    }

    /// <summary>Показывает подтверждение завершения с начальным фокусом на отмене.</summary>
    /// <param name="context">Контекст запроса и логического результата подтверждения.</param>
    /// <returns>Задача показа диалога и публикации ответа.</returns>
    /// <exception cref="OperationCanceledException">Запрошена отмена или окно уже закрыто.</exception>
    private async Task HandleKillConfirmationAsync(
        IInteractionContext<KillConfirmationRequest, bool> context)
    {
        KillConfirmationRequest request = context.Input;
        CancellationToken token = request.CancellationToken;
        token.ThrowIfCancellationRequested();
        if (_closed)
        {
            throw new OperationCanceledException(token);
        }

        Window dialog = new()
        {
            Title = "Подтверждение завершения",
            Width = 460,
            SizeToContent = SizeToContent.Height,
            CanResize = false,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };
        Button cancel = new() { Content = "Отмена", IsDefault = true, IsCancel = true };
        Button terminate = new() { Content = "Завершить" };

        void Cancel(object? sender, RoutedEventArgs e)
        {
            dialog.Close(false);
        }

        void Terminate(object? sender, RoutedEventArgs e)
        {
            dialog.Close(true);
        }

        void FocusCancel(object? sender, EventArgs e)
        {
            cancel.Focus();
        }

        bool closed = false;

        void MarkClosed(object? sender, EventArgs e)
        {
            closed = true;
        }

        cancel.Click += Cancel;
        terminate.Click += Terminate;
        dialog.Opened += FocusCancel;
        dialog.Closed += MarkClosed;
        dialog.Content = new StackPanel
        {
            Margin = new Thickness(16),
            Spacing = 12,
            Children =
            {
                new TextBlock
                {
                    Text = $"Завершить процесс «{request.ProcessName}» (PID {request.Identity.Pid})? " +
                           "Несохранённые данные приложения могут быть потеряны.",
                    TextWrapping = TextWrapping.Wrap
                },
                new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Spacing = 8,
                    Children = { cancel, terminate }
                }
            }
        };
        using CancellationTokenRegistration registration = token.Register(() => Dispatcher.UIThread.Post(() =>
        {
            if (!closed)
            {
                dialog.Close(false);
            }
        }));
        try
        {
            bool confirmed = await dialog.ShowDialog<bool>(this);
            token.ThrowIfCancellationRequested();
            context.SetOutput(confirmed);
        }
        finally
        {
            closed = true;
            cancel.Click -= Cancel;
            terminate.Click -= Terminate;
            dialog.Opened -= FocusCancel;
            dialog.Closed -= MarkClosed;
        }
    }
}