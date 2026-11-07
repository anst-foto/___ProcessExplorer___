using System;
using System.Linq;

using ProcessExplorer.Models;

namespace ProcessExplorer.ViewModels;

// Только представление: строки интерфейса не используются как JSON DTO.
/// <summary>Представляет строковую карточку процесса для интерфейса, а не JSON DTO.</summary>
public sealed record ProcessDetails
{
    /// <summary>Пустая карточка со значениями-заполнителями.</summary>
    public static ProcessDetails Empty { get; } = new();

    /// <summary>Отображаемое имя процесса.</summary>
    public string Name { get; init; } = "—";

    /// <summary>Отображаемый PID.</summary>
    public string Id { get; init; } = "—";

    /// <summary>Текст поля: заголовок главного окна; при отсутствии данных — тире или сообщение об ошибке.</summary>
    public string MainWindowTitle { get; init; } = "—";

    /// <summary>Текст поля: признак ответа процесса; при отсутствии данных — тире или сообщение об ошибке.</summary>
    public string Responding { get; init; } = "—";

    /// <summary>Время запуска в локальном часовом поясе.</summary>
    public string StartTime { get; init; } = "—";

    /// <summary>Текст поля: суммарное процессорное время; при отсутствии данных — тире или сообщение об ошибке.</summary>
    public string TotalProcessorTime { get; init; } = "—";

    /// <summary>Текст поля: название класса приоритета; при отсутствии данных — тире или сообщение об ошибке.</summary>
    public string PriorityClass { get; init; } = "—";

    /// <summary>Текст поля: базовый приоритет; при отсутствии данных — тире или сообщение об ошибке.</summary>
    public string BasePriority { get; init; } = "—";

    /// <summary>Текст поля: идентификатор сеанса; при отсутствии данных — тире или сообщение об ошибке.</summary>
    public string SessionId { get; init; } = "—";

    /// <summary>Текст поля: количество дескрипторов; при отсутствии данных — тире или сообщение об ошибке.</summary>
    public string HandleCount { get; init; } = "—";

    /// <summary>Текст поля: число элементов прочитанного массива потоков; при отсутствии данных — тире или сообщение об ошибке.</summary>
    public string ThreadCount { get; init; } = "—";

    /// <summary>Рабочий набор в мегабайтах и байтах.</summary>
    public string WorkingSet { get; init; } = "—";

    /// <summary>Частная память в мегабайтах и байтах.</summary>
    public string PrivateMemory { get; init; } = "—";

    /// <summary>Виртуальная память в мегабайтах и байтах.</summary>
    public string VirtualMemory { get; init; } = "—";

    /// <summary>Страничная память в мегабайтах и байтах.</summary>
    public string PagedMemory { get; init; } = "—";

    /// <summary>Текст поля: сведения об основном модуле; при отсутствии данных — тире или сообщение об ошибке.</summary>
    public string MainModule { get; init; } = "—";

    /// <summary>Строковый список модулей.</summary>
    public string ModuleList { get; init; } = "—";

    /// <summary>Строковый список потоков.</summary>
    public string ThreadList { get; init; } = "—";

    /// <summary>Сводка успешного чтения либо перечень ошибок.</summary>
    public string AvailabilityNote { get; init; } = "";

    /// <summary>Преобразует снимок в карточку с форматированием значений и сообщениями об ошибках.</summary>
    /// <param name="snapshot">Снимок процесса.</param>
    /// <returns>Карточка для привязок интерфейса; отсутствующие значения заменены на тире.</returns>
    public static ProcessDetails FromSnapshot(ProcessSnapshot snapshot)
    {
        static string Text(string? value)
        {
            return string.IsNullOrEmpty(value) ? "—" : value;
        }

        static string Number<T>(T? value) where T : struct
        {
            return value?.ToString() ?? "—";
        }

        static string Bytes(long? value)
        {
            return value is null ? "—" : $"{value.Value / 1024d / 1024d:N1} МБ ({value.Value:N0} байт)";
        }

        string Field(string key, string value)
        {
            return snapshot.Errors.ContainsKey(key)
                ? "Недоступно: " + snapshot.Errors[key]
                : value;
        }

        ModuleSnapshot? main = snapshot.MainModule;
        return new ProcessDetails
        {
            Name = snapshot.Name,
            Id = snapshot.Pid.ToString(),
            MainWindowTitle = Field("MainWindowTitle", Text(snapshot.MainWindowTitle)),
            Responding = Field("Responding", snapshot.Responding is null ? "—" :
                snapshot.Responding.Value ? "Да" : "Нет"),
            StartTime =
                Field("StartTimeUtc", snapshot.StartTimeUtc?.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss zzz") ?? "—"),
            TotalProcessorTime = Field("TotalProcessorTime", Number(snapshot.TotalProcessorTime)),
            PriorityClass = Field("PriorityClass", Text(snapshot.PriorityClass)),
            BasePriority = Field("BasePriority", Number(snapshot.BasePriority)),
            SessionId = Field("SessionId", Number(snapshot.SessionId)),
            HandleCount = Field("HandleCount", Number(snapshot.HandleCount)),
            ThreadCount = Field("Threads", Number(snapshot.ThreadCount)),
            WorkingSet = Field("WorkingSetBytes", Bytes(snapshot.WorkingSetBytes)),
            PrivateMemory = Field("PrivateMemoryBytes", Bytes(snapshot.PrivateMemoryBytes)),
            VirtualMemory = Field("VirtualMemoryBytes", Bytes(snapshot.VirtualMemoryBytes)),
            PagedMemory = Field("PagedMemoryBytes", Bytes(snapshot.PagedMemoryBytes)),
            MainModule = Field("MainModule", main is null
                ? "—"
                : string.Join(Environment.NewLine,
                    "Имя: " + Text(main.Name), "Путь: " + Text(main.FileName),
                    "Версия файла: " + Text(main.FileVersion), "Описание: " + Text(main.Description))),
            ModuleList = Field("Modules", snapshot.Modules is null
                ? "—"
                : Text(string.Join(Environment.NewLine, snapshot.Modules.Select(m =>
                    $"{Text(m.Name)} — {Text(m.FileName)}")))),
            ThreadList = Field("Threads", snapshot.Threads is null
                ? "—"
                : Text(string.Join(Environment.NewLine, snapshot.Threads.Select(t =>
                    $"ID {Number(t.Id)}: {Text(t.State)}")))),
            AvailabilityNote = snapshot.Errors.Count == 0
                ? "Сведения прочитаны."
                : string.Join(Environment.NewLine, snapshot.Errors.Select(e => $"{e.Key}: {e.Value}"))
        };
    }
}