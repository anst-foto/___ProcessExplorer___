using System;
using System.Collections.Generic;

namespace ProcessExplorer.Models;

/// <summary>Идентифицирует процесс сочетанием PID и времени запуска.</summary>
/// <param name="Pid">Идентификатор процесса.</param>
/// <param name="StartTimeUtc">Время запуска процесса в UTC.</param>
public sealed record ProcessIdentity(int Pid, DateTimeOffset StartTimeUtc);

/// <summary>Представляет строку списка процессов.</summary>
/// <param name="Id">Идентификатор процесса.</param>
/// <param name="Name">Имя процесса.</param>
/// <param name="Identity">PID и время запуска либо null, если время недоступно.</param>
/// <param name="IdentityError">Причина недоступности идентичности либо null.</param>
public sealed record ProcessListItem(
    int Id,
    string Name,
    ProcessIdentity? Identity,
    string? IdentityError);

/// <summary>Содержит прочитанные сведения о модуле процесса.</summary>
/// <param name="Name">Имя модуля либо null.</param>
/// <param name="FileName">Путь к файлу модуля либо null.</param>
/// <param name="FileVersion">Версия файла либо null.</param>
/// <param name="Description">Описание файла либо null.</param>
public sealed record ModuleSnapshot(
    string? Name,
    string? FileName,
    string? FileVersion,
    string? Description);

/// <summary>Содержит прочитанные сведения о потоке.</summary>
/// <param name="Id">Идентификатор потока либо null при ошибке чтения.</param>
/// <param name="State">Строковое состояние потока либо null при ошибке чтения.</param>
public sealed record ThreadSnapshot(int? Id, string? State);

/// <summary>Хранит последовательно прочитанные сведения о процессе.</summary>
/// <remarks>Недоступные значения допускают null; причины ошибок хранятся отдельно. Сбор не является атомарным снимком ОС.</remarks>
public sealed record ProcessSnapshot
{
    /// <summary>Идентификатор процесса из выбранной строки.</summary>
    public required int Pid { get; init; }

    /// <summary>Имя процесса из выбранной строки.</summary>
    public required string Name { get; init; }

    /// <summary>Идентичность по прочитанному времени запуска либо null.</summary>
    public ProcessIdentity? Identity { get; init; }

    /// <summary>Время создания объекта снимка в UTC.</summary>
    public DateTimeOffset CollectedAtUtc { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>Заголовок главного окна либо null.</summary>
    public string? MainWindowTitle { get; init; }

    /// <summary>Признак ответа процесса либо null при недоступности.</summary>
    public bool? Responding { get; init; }

    /// <summary>Время запуска в UTC либо null.</summary>
    public DateTimeOffset? StartTimeUtc { get; init; }

    /// <summary>Суммарное процессорное время либо null.</summary>
    public TimeSpan? TotalProcessorTime { get; init; }

    /// <summary>Название класса приоритета либо null.</summary>
    public string? PriorityClass { get; init; }

    /// <summary>Базовый приоритет либо null.</summary>
    public int? BasePriority { get; init; }

    /// <summary>Идентификатор сеанса либо null.</summary>
    public int? SessionId { get; init; }

    /// <summary>Количество дескрипторов либо null.</summary>
    public int? HandleCount { get; init; }

    /// <summary>Число элементов прочитанного массива потоков либо null.</summary>
    public int? ThreadCount { get; init; }

    /// <summary>Размер рабочего набора в байтах либо null.</summary>
    public long? WorkingSetBytes { get; init; }

    /// <summary>Объём частной памяти в байтах либо null.</summary>
    public long? PrivateMemoryBytes { get; init; }

    /// <summary>Объём виртуальной памяти в байтах либо null.</summary>
    public long? VirtualMemoryBytes { get; init; }

    /// <summary>Объём страничной памяти в байтах либо null.</summary>
    public long? PagedMemoryBytes { get; init; }

    /// <summary>Сведения об основном модуле либо null.</summary>
    public ModuleSnapshot? MainModule { get; init; }

    /// <summary>Модули; null означает недоступность, пустой массив — отсутствие элементов.</summary>
    public ModuleSnapshot[]? Modules { get; init; }

    /// <summary>Потоки; null означает недоступность, пустой массив — отсутствие элементов.</summary>
    public ThreadSnapshot[]? Threads { get; init; }

    /// <summary>Причины ошибок по именам полей и элементов; ключ Process описывает общую ошибку.</summary>
    public required IReadOnlyDictionary<string, string> Errors { get; init; }
}

/// <summary>Задаёт корневую структуру JSON-экспорта.</summary>
/// <param name="SchemaVersion">Версия схемы экспорта.</param>
/// <param name="CollectedAtUtc">Время начала сбора экспорта в UTC.</param>
/// <param name="Processes">Материализованные снимки процессов.</param>
public sealed record ProcessExport(
    int SchemaVersion,
    DateTimeOffset CollectedAtUtc,
    ProcessSnapshot[] Processes);

/// <summary>Различает результаты запроса завершения процесса.</summary>
public enum KillResult
{
    /// <summary>Выход процесса подтверждён после запроса завершения.</summary>
    ExitConfirmed,

    /// <summary>Запрос отправлен, но выход не подтверждён за время ожидания.</summary>
    RequestSentTimeout,

    /// <summary>Процесс уже отсутствовал или завершился до отправки запроса.</summary>
    AlreadyExited
}