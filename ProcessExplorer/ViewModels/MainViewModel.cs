using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Avalonia.Threading;

using ProcessExplorer.Models;
using ProcessExplorer.Services;

using ReactiveUI;
using ReactiveUI.Binding;
using ReactiveUI.Primitives;
using ReactiveUI.Primitives.Signals;

namespace ProcessExplorer.ViewModels;

/// <summary>Управляет списком, карточкой, экспортом и завершением выбранного процесса.</summary>
/// <remarks>Изменение состояния и вызовы команд предполагают UI-поток. Чтение ОС и подготовка JSON выполняются в фоновых задачах.</remarks>
public sealed class MainViewModel : ViewModelBase, IDisposable
{
    /// <summary>Настройки форматирования и camelCase-имён JSON.</summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>Сервис доступа к процессам.</summary>
    private readonly ProcessService _service;

    /// <summary>Таймер периодического обновления списка.</summary>
    private readonly DispatcherTimer _timer;

    /// <summary>Источник отмены работы модели представления.</summary>
    private readonly CancellationTokenSource _lifetime = new();

    /// <summary>Токен времени жизни, захваченный до освобождения источника.</summary>
    private readonly CancellationToken _lifetimeToken;

    /// <summary>Сигнал доступности действий с выбранным процессом.</summary>
    private readonly BehaviorSignal<bool> _canSelected = new(false);

    /// <summary>Сигнал доступности общего экспорта.</summary>
    private readonly BehaviorSignal<bool> _canAll = new(true);

    /// <summary>Подписки резервного обработчика ошибок команд.</summary>
    private readonly List<IDisposable> _errorSubscriptions = new();

    /// <summary>Задачи карточки, учитываемые при освобождении ресурсов.</summary>
    private readonly List<Task> _detailTasks = new();

    /// <summary>Последняя запущенная задача обновления списка.</summary>
    private Task _refreshTask = Task.CompletedTask;

    /// <summary>Последняя запущенная задача пользовательского действия.</summary>
    private Task _actionTask = Task.CompletedTask;

    /// <summary>Источник отмены текущей загрузки карточки; освобождается её задачей.</summary>
    private CancellationTokenSource? _detailsCancellation;

    /// <summary>Текущая выбранная строка.</summary>
    private ProcessListItem? _selectedProcess;

    /// <summary>Текущая строковая карточка.</summary>
    private ProcessDetails _selectedDetails = ProcessDetails.Empty;

    /// <summary>Текущий текст строки состояния.</summary>
    private string _status = "Ожидание открытия окна…";

    /// <summary>Признак выполняемого пользовательского действия.</summary>
    private bool _isBusy;

    /// <summary>Признак выполняемого обновления списка.</summary>
    private bool _refreshing;

    /// <summary>Признак запуска таймера и первого обновления.</summary>
    private bool _started;

    /// <summary>Признак начала освобождения модели.</summary>
    private bool _disposed;

    /// <summary>Признак приоритета статуса действия над статусом обновления.</summary>
    private bool _actionStatus;

    /// <summary>Версия запроса карточки для отбрасывания устаревших результатов.</summary>
    private int _detailsVersion;

    /// <summary>Версия действия для отбрасывания устаревшего обновления списка.</summary>
    private int _actionVersion;

    /// <summary>Коллекция отображаемых строк; используется в UI-потоке.</summary>
    public ObservableCollection<ProcessListItem> Processes { get; } = new();

    /// <summary>Команда экспорта снимка выбранной строки в JSON.</summary>
    public ReactiveCommand<RxVoid, RxVoid> SaveSelectedProcessCommand { get; }

    /// <summary>Команда экспорта снимков строк текущего списка в JSON.</summary>
    public ReactiveCommand<RxVoid, RxVoid> SaveAllProcessesCommand { get; }

    /// <summary>Команда проверки, подтверждения и завершения выбранного процесса.</summary>
    public ReactiveCommand<RxVoid, RxVoid> KillSelectedProcessCommand { get; }

    /// <summary>Взаимодействие для выбора файла и записи готового JSON.</summary>
    public Interaction<SaveJsonRequest, SaveJsonResult> SaveJsonInteraction { get; } = new();

    /// <summary>Взаимодействие для получения подтверждения завершения процесса.</summary>
    public Interaction<KillConfirmationRequest, bool> KillConfirmationInteraction { get; } = new();

    /// <summary>Выбранная строка; смена выбора сбрасывает и перезапрашивает карточку в UI-потоке.</summary>
    public ProcessListItem? SelectedProcess
    {
        get => _selectedProcess;
        set
        {
            Dispatcher.UIThread.VerifyAccess();
            if (_disposed || ReferenceEquals(_selectedProcess, value))
            {
                return;
            }

            this.RaiseAndSetIfChanged(ref _selectedProcess, value);
            this.RaisePropertyChanged(nameof(HasSelection));
            this.RaisePropertyChanged(nameof(HasNoSelection));
            PublishAvailability();
            SelectedDetails = ProcessDetails.Empty;
            RequestDetails();
        }
    }

    /// <summary>Признак наличия выбранной строки.</summary>
    public bool HasSelection => SelectedProcess is not null;

    /// <summary>Признак отсутствия выбранной строки.</summary>
    public bool HasNoSelection => !HasSelection;

    /// <summary>Последняя применённая карточка выбранного процесса.</summary>
    public ProcessDetails SelectedDetails
    {
        get => _selectedDetails;
        private set => this.RaiseAndSetIfChanged(ref _selectedDetails, value);
    }

    /// <summary>Текст текущего состояния или результата действия.</summary>
    public string Status
    {
        get => _status;
        private set => this.RaiseAndSetIfChanged(ref _status, value);
    }

    /// <summary>Признак действия, временно ограничивающего команды и применение обновлений.</summary>
    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (_isBusy == value)
            {
                return;
            }

            this.RaiseAndSetIfChanged(ref _isBusy, value);
            PublishAvailability();
        }
    }

    // Конструктор не сканирует ОС, в том числе при работе дизайнера.
    /// <summary>Создаёт команды, подписки и таймер без сканирования ОС.</summary>
    /// <param name="service">Сервис работы с процессами.</param>
    /// <exception cref="ArgumentNullException">Сервис равен null.</exception>
    public MainViewModel(ProcessService service)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _lifetimeToken = _lifetime.Token;
        SaveSelectedProcessCommand = ReactiveCommand.CreateFromTask(
            (CancellationToken ct) => StartAction(SaveSelectedAsync, ct), _canSelected);
        SaveAllProcessesCommand = ReactiveCommand.CreateFromTask(
            (CancellationToken ct) => StartAction(SaveAllAsync, ct), _canAll);
        KillSelectedProcessCommand = ReactiveCommand.CreateFromTask(
            (CancellationToken ct) => StartAction(KillSelectedAsync, ct), _canSelected);
        CommandErrorObserver observer = new(this);
        _errorSubscriptions.Add(SaveSelectedProcessCommand.ThrownExceptions.Subscribe(observer));
        _errorSubscriptions.Add(SaveAllProcessesCommand.ThrownExceptions.Subscribe(observer));
        _errorSubscriptions.Add(KillSelectedProcessCommand.ThrownExceptions.Subscribe(observer));
        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
        _timer.Tick += OnTick;
    }

    /// <summary>Из UI-потока однократно запускает таймер и первое обновление.</summary>
    /// <remarks>После освобождения или повторного вызова ничего не делает.</remarks>
    public void Start()
    {
        Dispatcher.UIThread.VerifyAccess();
        if (_disposed || _started)
        {
            return;
        }

        _started = true;
        Status = "Загрузка процессов…";
        _timer.Start();
        RequestRefresh();
    }

    /// <summary>Публикует доступность команд с учётом выбора, занятности и освобождения.</summary>
    private void PublishAvailability()
    {
        _canSelected.OnNext(!_disposed && !IsBusy && HasSelection);
        _canAll.OnNext(!_disposed && !IsBusy);
    }

    /// <summary>Устанавливает статус действия, если модель ещё не освобождена.</summary>
    /// <param name="message">Текст состояния.</param>
    private void SetStatus(string message)
    {
        if (_disposed)
        {
            return;
        }

        _actionStatus = true;
        Status = message;
    }

    /// <summary>Передаёт резервное сообщение об ошибке команды в UI-поток.</summary>
    /// <param name="error">Ошибка команды.</param>
    private void ReportCommandError(Exception error)
    {
        // Резервная обработка ошибок ReactiveCommand, без второго диалога ошибок.
        void Apply()
        {
            if (!_disposed)
            {
                SetStatus("Ошибка команды: " + error.Message);
            }
        }

        if (Dispatcher.UIThread.CheckAccess())
        {
            Apply();
        }
        else
        {
            Dispatcher.UIThread.Post(Apply);
        }
    }

    /// <summary>Направляет ошибки реактивных команд владельцу.</summary>
    /// <param name="owner">Модель представления, принимающая ошибки.</param>
    private sealed class CommandErrorObserver(MainViewModel owner) : IObserver<Exception>
    {
        /// <summary>Передаёт полученное исключение владельцу.</summary>
        /// <param name="value">Исключение из потока ошибок команды.</param>
        public void OnNext(Exception value)
        {
            owner.ReportCommandError(value);
        }

        /// <summary>Передаёт ошибку завершения наблюдаемой последовательности владельцу.</summary>
        /// <param name="error">Ошибка последовательности.</param>
        public void OnError(Exception error)
        {
            owner.ReportCommandError(error);
        }

        /// <summary>Обрабатывает завершение последовательности без дополнительных действий.</summary>
        public void OnCompleted() { }
    }

    /// <summary>Запускает действие из UI-потока, если модель не освобождена и не занята.</summary>
    /// <param name="action">Асинхронный сценарий действия.</param>
    /// <param name="commandToken">Токен отмены команды.</param>
    /// <returns>Задача сценария либо уже завершённая задача, если запуск отклонён.</returns>
    private Task StartAction(Func<CancellationToken, Task> action, CancellationToken commandToken)
    {
        Dispatcher.UIThread.VerifyAccess();
        // Guard нужен также для прямого программного Execute, не только ICommand.CanExecute.
        if (_disposed || IsBusy)
        {
            return Task.CompletedTask;
        }

        return _actionTask = RunActionAsync(action, commandToken);
    }

    /// <summary>Выполняет действие с общей отменой, обработкой ошибок и восстановлением обновлений.</summary>
    /// <param name="action">Асинхронный сценарий действия.</param>
    /// <param name="commandToken">Токен отмены команды.</param>
    /// <returns>Задача завершения сценария и его блока очистки.</returns>
    private async Task RunActionAsync(
        Func<CancellationToken, Task> action, CancellationToken commandToken)
    {
        using CancellationTokenSource cancellation = CancellationTokenSource.CreateLinkedTokenSource(
            _lifetimeToken, commandToken);
        CancellationToken token = cancellation.Token;
        IsBusy = true;
        ++_actionVersion;
        ++_detailsVersion;
        _detailsCancellation?.Cancel();
        try
        {
            token.ThrowIfCancellationRequested();
            // Вся цепочка, включая picker/confirm/open/write/DisposeAsync, внутри try.
            await action(token);
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
            SetStatus("Действие отменено. Если запись уже началась, файл может быть неполным.");
        }
        catch (Exception ex)
        {
            SetStatus("Ошибка действия: " + ex.Message);
        }
        finally
        {
            if (!_disposed)
            {
                IsBusy = false;
                RequestDetails();
                RequestRefresh();
            }
        }
    }

    /// <summary>Собирает снимок выбранной строки и передаёт JSON обработчику сохранения.</summary>
    /// <param name="token">Токен отмены действия.</param>
    /// <returns>Задача подготовки и передачи результата сохранения в статус.</returns>
    private async Task SaveSelectedAsync(CancellationToken token)
    {
        ProcessListItem? selected = SelectedProcess; // Неизменяемая выбранная строка до await и диалогов.
        if (selected is null)
        {
            return;
        }

        string json = await Task.Run(() =>
        {
            DateTimeOffset collectedAt = DateTimeOffset.UtcNow;
            ProcessSnapshot snapshot = _service.ReadSnapshot(selected, token);
            token.ThrowIfCancellationRequested();
            return JsonSerializer.Serialize(new ProcessExport(1, collectedAt, new[] { snapshot }), JsonOptions);
        }, token);
        token.ThrowIfCancellationRequested();
        SaveJsonResult result = await SaveJsonInteraction.Handle(
            new SaveJsonRequest(json, $"process-{selected.Id}.json", token));
        token.ThrowIfCancellationRequested();
        SetSaveStatus(result);
    }

    /// <summary>Фиксирует строки текущего списка и экспортирует их снимки через взаимодействие.</summary>
    /// <param name="token">Токен отмены действия.</param>
    /// <returns>Задача сбора, сериализации и обработки результата сохранения.</returns>
    private async Task SaveAllAsync(CancellationToken token)
    {
        ProcessListItem[] selectedRows = Processes.ToArray(); // Только UI-поток, до первого await.
        string json = await Task.Run(() =>
        {
            DateTimeOffset collectedAt = DateTimeOffset.UtcNow;
            ProcessSnapshot[] snapshots = selectedRows.Select(row => _service.ReadSnapshot(row, token)).ToArray();
            token.ThrowIfCancellationRequested();
            return JsonSerializer.Serialize(new ProcessExport(1, collectedAt, snapshots), JsonOptions);
        }, token);
        token.ThrowIfCancellationRequested();
        SaveJsonResult result = await SaveJsonInteraction.Handle(new SaveJsonRequest(json, "processes.json", token));
        token.ThrowIfCancellationRequested();
        SetSaveStatus(result);
    }

    /// <summary>Отображает результат сохранения или отмены выбора файла.</summary>
    /// <param name="result">Результат взаимодействия сохранения.</param>
    private void SetSaveStatus(SaveJsonResult result)
    {
        SetStatus(result.Cancelled
            ? "Сохранение отменено."
            : result.Saved
                ? "JSON сохранён: " + result.Name
                : "Файл не сохранён.");
    }

    /// <summary>Проверяет идентичность, запрашивает подтверждение и завершение выбранного процесса.</summary>
    /// <param name="token">Токен отмены действия.</param>
    /// <returns>Задача сценария с обновлением статуса результата.</returns>
    private async Task KillSelectedAsync(CancellationToken token)
    {
        ProcessListItem? selected = SelectedProcess;
        if (selected is null)
        {
            return;
        }

        ProcessIdentity identity = await Task.Run(() => _service.CaptureKillIdentity(selected, token), token);
        token.ThrowIfCancellationRequested();
        if (!await KillConfirmationInteraction.Handle(
                new KillConfirmationRequest(identity, selected.Name, token)))
        {
            SetStatus("Завершение отменено.");
            return;
        }

        token.ThrowIfCancellationRequested();
        KillResult result = await Task.Run(() => _service.KillAsync(identity, selected.Name, token), token);
        SetStatus(result switch
        {
            KillResult.ExitConfirmed => $"Выход процесса {selected.Name} (PID {identity.Pid}) подтверждён.",
            KillResult.AlreadyExited => "Процесс уже завершился.",
            _ => "Запрос завершения отправлен; за 5 секунд выход не подтверждён."
        });
    }

    /// <summary>Запрашивает обновление списка по событию таймера.</summary>
    /// <param name="sender">Источник события.</param>
    /// <param name="e">Данные события таймера.</param>
    private void OnTick(object? sender, EventArgs e)
    {
        RequestRefresh();
    }

    /// <summary>Запускает обновление, если модель активна, не занята и другое обновление не выполняется.</summary>
    private void RequestRefresh()
    {
        if (_disposed || !_started || IsBusy || _refreshing)
        {
            return;
        }

        _refreshTask = RefreshAsync();
    }

    /// <summary>Читает список в фоне и применяет результат, если он не устарел.</summary>
    /// <returns>Задача обновления с обработкой отмены и ошибок.</returns>
    private async Task RefreshAsync()
    {
        _refreshing = true;
        int actionVersion = _actionVersion;
        CancellationToken token = _lifetimeToken;
        try
        {
            ProcessListItem[] rows = await Task.Run(() => _service.ReadList(token), token);
            // Даже если действие успело начаться И закончиться, старый refresh отбрасывается.
            if (_disposed || token.IsCancellationRequested || IsBusy || actionVersion != _actionVersion)
            {
                return;
            }

            ApplyRows(rows);
            if (!_actionStatus)
            {
                Status = $"Процессов: {Processes.Count}";
            }

            RequestDetails();
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested) { }
        catch (Exception ex)
        {
            if (!_disposed && !_actionStatus)
            {
                Status = "Ошибка обновления: " + ex.Message;
            }
        }
        finally { _refreshing = false; }
    }

    /// <summary>Обновляет коллекцию без Clear и восстанавливает выбор по подтверждённой идентичности.</summary>
    /// <param name="rows">Отсортированные строки нового списка.</param>
    private void ApplyRows(ProcessListItem[] rows)
    {
        Dictionary<int, ProcessListItem> existing = Processes.ToDictionary(x => x.Id);
        ProcessListItem? selection = SelectedProcess;
        ProcessListItem[] desired = rows.Select(row => existing.TryGetValue(row.Id, out ProcessListItem? old) &&
                                                       row.Identity is not null && row.Identity == old.Identity &&
                                                       row.Name == old.Name
            ? old
            : row).ToArray();
        // Без Clear: строки с подтверждённой неизменной identity сохраняют объект и выбор.
        HashSet<ProcessListItem> keep = desired.ToHashSet();
        for (int i = Processes.Count - 1; i >= 0; --i)
        {
            if (!keep.Contains(Processes[i]))
            {
                Processes.RemoveAt(i);
            }
        }

        for (int i = 0; i < desired.Length; ++i)
        {
            int index = Processes.IndexOf(desired[i]);
            if (index < 0)
            {
                Processes.Insert(i, desired[i]);
            }
            else if (index != i)
            {
                Processes.Move(index, i);
            }
        }

        SelectedProcess = selection?.Identity is null
            ? null
            : Processes.FirstOrDefault(x =>
                x.Identity == selection.Identity && x.Name == selection.Name);
    }

    /// <summary>Отменяет прежний запрос карточки и запускает новый при доступном выборе.</summary>
    private void RequestDetails()
    {
        if (_disposed)
        {
            return;
        }

        ++_detailsVersion;
        _detailsCancellation?.Cancel(); // Не Dispose чужого ещё работающего CTS.
        _detailsCancellation = null;
        if (IsBusy || SelectedProcess is null)
        {
            return;
        }

        _detailTasks.RemoveAll(t => t.IsCompleted);
        _detailTasks.Add(LoadDetailsAsync(SelectedProcess, _detailsVersion));
    }

    /// <summary>Загружает карточку и применяет её только для актуальных версии и выбора.</summary>
    /// <param name="selected">Строка, для которой начато чтение.</param>
    /// <param name="version">Версия запроса карточки.</param>
    /// <returns>Задача загрузки и освобождения её источника отмены.</returns>
    private async Task LoadDetailsAsync(ProcessListItem selected, int version)
    {
        using CancellationTokenSource cancellation = CancellationTokenSource.CreateLinkedTokenSource(_lifetimeToken);
        _detailsCancellation = cancellation;
        CancellationToken token = cancellation.Token; // Захват до Task.Run; не читать CTS.Token в worker.
        try
        {
            ProcessDetails details = await Task.Run(() =>
                ProcessDetails.FromSnapshot(_service.ReadSnapshot(selected, token)), token);
            if (!_disposed && !token.IsCancellationRequested && !IsBusy &&
                version == _detailsVersion && ReferenceEquals(selected, SelectedProcess))
            {
                SelectedDetails = details;
            }
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested) { }
        catch (Exception ex)
        {
            if (!_disposed && !IsBusy && version == _detailsVersion)
            {
                SelectedDetails = ProcessDetails.Empty with
                {
                    Name = selected.Name,
                    Id = selected.Id.ToString(),
                    AvailabilityNote = "Ошибка чтения: " + ex.Message
                };
            }
        }
        finally
        {
            // Продолжения и Dispose VM выполняются в одном UI-потоке.
            if (ReferenceEquals(_detailsCancellation, cancellation))
            {
                _detailsCancellation = null;
            }
        }
    }

    /// <summary>Из UI-потока останавливает таймер, отменяет работу и освобождает команды.</summary>
    /// <remarks>Ожидание текущих задач и освобождение подписок и lifetime-источника выполняются отдельной асинхронной задачей, а не блокируют этот вызов.</remarks>
    public void Dispose()
    {
        Dispatcher.UIThread.VerifyAccess();
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        ++_detailsVersion;
        _timer.Stop();
        _timer.Tick -= OnTick;
        PublishAvailability();
        _lifetime.Cancel(); // Отменяет команды, refresh, details и UI-запись.
        _detailsCancellation?.Cancel();
        SaveSelectedProcessCommand.Dispose();
        SaveAllProcessesCommand.Dispose();
        KillSelectedProcessCommand.Dispose();
        _canSelected.Dispose();
        _canAll.Dispose();
        // Lifetime CTS освобождается лишь после потребителей (включая linked CTS).
        Task[] pending = _detailTasks.Concat(new[] { _refreshTask, _actionTask }).ToArray();
        _ = ReleaseAfterWorkAsync(pending);
    }

    /// <summary>Ожидает учтённые задачи и освобождает подписки и источник отмены.</summary>
    /// <param name="pending">Задачи, существовавшие при освобождении модели.</param>
    /// <returns>Задача отложенного освобождения ресурсов.</returns>
    private async Task ReleaseAfterWorkAsync(Task[] pending)
    {
        try { await Task.WhenAll(pending); }
        catch (Exception)
        {
            /* Каждый сценарий уже обрабатывает ошибки; cleanup обязателен. */
        }
        finally
        {
            foreach (IDisposable subscription in _errorSubscriptions)
            {
                subscription.Dispose();
            }

            _errorSubscriptions.Clear();
            _detailTasks.Clear();
            _lifetime.Dispose();
        }
    }
}