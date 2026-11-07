using System.Threading;

namespace ProcessExplorer.Models;

/// <summary>Передаёт готовый JSON обработчику сохранения.</summary>
/// <param name="Json">Сериализованный документ.</param>
/// <param name="SuggestedName">Предлагаемое имя файла.</param>
/// <param name="CancellationToken">Токен отмены сценария сохранения.</param>
public sealed record SaveJsonRequest(
    string Json,
    string SuggestedName,
    CancellationToken CancellationToken);

/// <summary>Описывает результат диалога и записи JSON.</summary>
/// <param name="Saved">Признак успешного завершения записи и освобождения ресурсов.</param>
/// <param name="Cancelled">Признак отмены выбора файла.</param>
/// <param name="Name">Имя сохранённого файла либо null.</param>
public sealed record SaveJsonResult(bool Saved, bool Cancelled, string? Name);

/// <summary>Передаёт данные для подтверждения завершения процесса.</summary>
/// <param name="Identity">Предварительно проверенная идентичность процесса.</param>
/// <param name="ProcessName">Имя процесса для текста подтверждения.</param>
/// <param name="CancellationToken">Токен отмены сценария.</param>
public sealed record KillConfirmationRequest(
    ProcessIdentity Identity,
    string ProcessName,
    CancellationToken CancellationToken);