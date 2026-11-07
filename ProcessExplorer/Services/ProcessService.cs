using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using ProcessExplorer.Models;

namespace ProcessExplorer.Services;

// Все обращения к System.Diagnostics.Process находятся здесь.
// Синхронные методы вызываются VM только внутри Task.Run.
/// <summary>Читает сведения о процессах и выполняет запрос завершения.</summary>
/// <remarks>Синхронные операции вызываются моделью представления в фоне. Доступность свойств зависит от ОС, прав и состояния процесса.</remarks>
public sealed class ProcessService
{
    /// <summary>Определяет ошибки, которые код чтения преобразует в сведения о недоступности.</summary>
    /// <param name="ex">Проверяемое исключение.</param>
    /// <returns>true для обрабатываемых ошибок чтения; иначе false.</returns>
    private static bool IsReadError(Exception ex)
    {
        return ex is Win32Exception or InvalidOperationException or ArgumentException
            or NotSupportedException or System.IO.IOException
            or UnauthorizedAccessException or System.Security.SecurityException;
    }

    /// <summary>Читает время запуска и переводит его в UTC.</summary>
    /// <param name="process">Процесс для чтения.</param>
    /// <returns>Время запуска в UTC.</returns>
    private static DateTimeOffset StartUtc(Process process)
    {
        return new DateTimeOffset(process.StartTime.ToUniversalTime());
    }

    /// <summary>Читает и сортирует доступные строки процессов по имени и PID.</summary>
    /// <param name="token">Токен отмены, проверяемый между процессами.</param>
    /// <returns>Массив строк; процессы с недоступными PID или именем пропущены.</returns>
    /// <exception cref="OperationCanceledException">Запрошена отмена.</exception>
    public ProcessListItem[] ReadList(CancellationToken token)
    {
        Process[] processes = Process.GetProcesses();
        try
        {
            List<ProcessListItem> result = new();
            foreach (Process process in processes)
            {
                token.ThrowIfCancellationRequested();
                try
                {
                    int id = process.Id;
                    string name = process.ProcessName;
                    ProcessIdentity? identity = null;
                    string? error = null;
                    try { identity = new ProcessIdentity(id, StartUtc(process)); }
                    catch (Exception ex) when (IsReadError(ex)) { error = ex.Message; }

                    result.Add(new ProcessListItem(id, name, identity, error));
                }
                catch (Exception ex) when (IsReadError(ex))
                {
                    // Даже PID/имя уже недоступны: строку нельзя надёжно показать.
                }
            }

            return result.OrderBy(x => x.Name, StringComparer.CurrentCultureIgnoreCase)
                .ThenBy(x => x.Id).ToArray();
        }
        finally
        {
            foreach (Process process in processes)
            {
                process.Dispose();
            }
        }
    }

    /// <summary>Сверяет имя и, при наличии исходной идентичности, время запуска процесса.</summary>
    /// <param name="process">Текущий объект процесса.</param>
    /// <param name="item">Исходная строка списка.</param>
    /// <exception cref="InvalidOperationException">Имя или идентичность изменились.</exception>
    private static void Validate(Process process, ProcessListItem item)
    {
        if (!string.Equals(process.ProcessName, item.Name, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Процесс с выбранным PID изменился.");
        }

        if (item.Identity is not null &&
            new ProcessIdentity(process.Id, StartUtc(process)) != item.Identity)
        {
            throw new InvalidOperationException("PID уже принадлежит другому процессу.");
        }
    }

    /// <summary>Проверяет выбранную строку перед показом подтверждения завершения.</summary>
    /// <param name="item">Выбранная строка списка.</param>
    /// <param name="token">Токен отмены.</param>
    /// <returns>Исходная идентичность после проверки.</returns>
    /// <exception cref="InvalidOperationException">Выбран собственный процесс, нет идентичности или процесс изменился.</exception>
    /// <exception cref="OperationCanceledException">Запрошена отмена.</exception>
    public ProcessIdentity CaptureKillIdentity(ProcessListItem item, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if (item.Id == Environment.ProcessId)
        {
            throw new InvalidOperationException("Завершение самого Process Explorer запрещено.");
        }

        if (item.Identity is null)
        {
            throw new InvalidOperationException(
                "Нельзя безопасно завершить процесс: время запуска выбранной строки недоступно. " +
                "Обновите список или проверьте права доступа.");
        }

        using Process process = Process.GetProcessById(item.Id);
        Validate(process, item);
        token.ThrowIfCancellationRequested();
        return item.Identity;
    }

    /// <summary>Собирает сведения о процессе, сохраняя обрабатываемые ошибки по полям.</summary>
    /// <param name="item">Исходная строка списка.</param>
    /// <param name="token">Токен отмены, проверяемый между чтениями.</param>
    /// <returns>Снимок с доступными значениями и причинами ошибок.</returns>
    /// <remarks>При общей обрабатываемой ошибке возвращает PID, имя и ошибку Process. Отсутствие исходного времени запуска отмечается ошибкой Identity.</remarks>
    /// <exception cref="OperationCanceledException">Запрошена отмена.</exception>
    public ProcessSnapshot ReadSnapshot(ProcessListItem item, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        Dictionary<string, string> errors = new();

        T? Number<T>(string key, Func<T> read) where T : struct
        {
            token.ThrowIfCancellationRequested();
            try { return read(); }
            catch (Exception ex) when (IsReadError(ex))
            {
                errors[key] = ex.Message;
                return null;
            }
        }

        T? Reference<T>(string key, Func<T?> read) where T : class
        {
            token.ThrowIfCancellationRequested();
            try { return read(); }
            catch (Exception ex) when (IsReadError(ex))
            {
                errors[key] = ex.Message;
                return null;
            }
        }

        ModuleSnapshot Module(ProcessModule module, string prefix)
        {
            return new ModuleSnapshot(
                Reference(prefix + ".Name", () => module.ModuleName),
                Reference(prefix + ".FileName", () => module.FileName),
                Reference(prefix + ".FileVersion", () => module.FileVersionInfo.FileVersion),
                Reference(prefix + ".Description", () => module.FileVersionInfo.FileDescription));
        }

        try
        {
            using Process process = Process.GetProcessById(item.Id);
            Validate(process, item);
            DateTimeOffset? start = Number("StartTimeUtc", () => StartUtc(process));
            if (item.Identity is null)
            {
                errors["Identity"] = "В исходной строке нет времени запуска; идентичность снимка не гарантируется. " +
                                     item.IdentityError;
            }

            ModuleSnapshot? main = Reference("MainModule", () =>
            {
                using ProcessModule? module = process.MainModule;
                return module is null ? null : Module(module, "MainModule");
            });
            ModuleSnapshot[]? modules = Reference("Modules", () =>
            {
                ProcessModuleCollection collection = process.Modules;
                try
                {
                    return collection.Cast<ProcessModule>().Select((m, i) =>
                        Module(m, $"Modules[{i}]")).ToArray();
                }
                finally
                {
                    foreach (ProcessModule module in collection)
                    {
                        module.Dispose();
                    }
                }
            });
            ThreadSnapshot[]? threads = Reference("Threads", () =>
            {
                ProcessThreadCollection collection = process.Threads;
                try
                {
                    return collection.Cast<ProcessThread>().Select((t, i) => new ThreadSnapshot(
                        Number($"Threads[{i}].Id", () => t.Id),
                        Reference($"Threads[{i}].State", () => t.ThreadState.ToString()))).ToArray();
                }
                finally
                {
                    foreach (ProcessThread thread in collection)
                    {
                        thread.Dispose();
                    }
                }
            });

            return new ProcessSnapshot
            {
                Pid = item.Id,
                Name = item.Name,
                Identity = start is null ? null : new ProcessIdentity(item.Id, start.Value),
                StartTimeUtc = start,
                MainWindowTitle = Reference("MainWindowTitle", () => process.MainWindowTitle),
                Responding = Number("Responding", () => process.Responding),
                TotalProcessorTime = Number("TotalProcessorTime", () => process.TotalProcessorTime),
                PriorityClass = Reference("PriorityClass", () => process.PriorityClass.ToString()),
                BasePriority = Number("BasePriority", () => process.BasePriority),
                SessionId = Number("SessionId", () => process.SessionId),
                HandleCount = Number("HandleCount", () => process.HandleCount),
                ThreadCount = threads?.Length,
                WorkingSetBytes = Number("WorkingSetBytes", () => process.WorkingSet64),
                PrivateMemoryBytes = Number("PrivateMemoryBytes", () => process.PrivateMemorySize64),
                VirtualMemoryBytes = Number("VirtualMemoryBytes", () => process.VirtualMemorySize64),
                PagedMemoryBytes = Number("PagedMemoryBytes", () => process.PagedMemorySize64),
                MainModule = main,
                Modules = modules,
                Threads = threads,
                Errors = errors
            };
        }
        catch (Exception ex) when (IsReadError(ex))
        {
            errors["Process"] = "Процесс завершился, изменился или недоступен: " + ex.Message;
            return new ProcessSnapshot { Pid = item.Id, Name = item.Name, Errors = errors };
        }
    }

    /// <summary>Запрашивает завершение выбранного процесса и ожидает подтверждения выхода.</summary>
    /// <param name="identity">PID и время запуска для повторной проверки.</param>
    /// <param name="expectedName">Ожидаемое имя процесса.</param>
    /// <param name="token">Токен отмены операции и ожидания.</param>
    /// <returns>Результат проверки выхода, включая отсутствие процесса или истечение ожидания.</returns>
    /// <remarks>Завершается только процесс, не дерево. Ожидание ограничено пятью секундами; проверка идентичности и системный вызов не атомарны.</remarks>
    /// <exception cref="InvalidOperationException">Выбран собственный процесс, обнаружена смена процесса или операция недопустима.</exception>
    /// <exception cref="OperationCanceledException">Запрошена отмена вызывающей стороной.</exception>
    public async Task<KillResult> KillAsync(
        ProcessIdentity identity, string expectedName, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if (identity.Pid == Environment.ProcessId)
        {
            throw new InvalidOperationException("Завершение самого Process Explorer запрещено.");
        }

        Process process;
        try { process = Process.GetProcessById(identity.Pid); }
        catch (ArgumentException) { return KillResult.AlreadyExited; }

        using (process)
        {
            if (process.HasExited)
            {
                return KillResult.AlreadyExited;
            }

            Validate(process, new ProcessListItem(identity.Pid, expectedName, identity, null));
            token.ThrowIfCancellationRequested();
            try { process.Kill(); }
            catch (InvalidOperationException) when (process.HasExited)
            {
                return KillResult.AlreadyExited;
            }

            using CancellationTokenSource timeout = CancellationTokenSource.CreateLinkedTokenSource(token);
            timeout.CancelAfter(TimeSpan.FromSeconds(5));
            try
            {
                await process.WaitForExitAsync(timeout.Token).ConfigureAwait(false);
                return KillResult.ExitConfirmed;
            }
            catch (OperationCanceledException) when (!token.IsCancellationRequested)
            {
                return process.HasExited ? KillResult.ExitConfirmed : KillResult.RequestSentTimeout;
            }
        }
    }
}