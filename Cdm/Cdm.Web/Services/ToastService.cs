using Cdm.Common.Errors;
using Microsoft.Extensions.Localization;

namespace Cdm.Web.Services;

/// <summary>
/// Types visuels d'un toast.
/// </summary>
public enum ToastType
{
    Success,
    Info,
    Warning,
    Error
}

/// <summary>
/// Représente un message toast actif avec son identifiant unique et ses métadonnées.
/// </summary>
public sealed record ToastMessage(
    Guid Id,
    ToastType Type,
    string Message,
    string? Title,
    int DurationMs)
{
    public DateTime CreatedAt { get; } = DateTime.UtcNow;
}

/// <summary>
/// Service centralisé de gestion des notifications toast.
/// S'enregistre en Scoped — un état par circuit SignalR.
/// </summary>
public class ToastService
{
    private readonly List<ToastMessage> _toasts = [];

    /// <summary>Déclenché à chaque changement de la liste (ajout ou retrait).</summary>
    public event Action? OnChange;

    /// <summary>Liste en lecture seule des toasts actuellement visibles.</summary>
    public IReadOnlyList<ToastMessage> Toasts => _toasts.AsReadOnly();

    // ── Méthodes génériques ──────────────────────────────────────────────────

    /// <summary>Affiche un toast succès.</summary>
    public void ShowSuccess(string message, string? title = null, int durationMs = 4000)
        => Add(ToastType.Success, message, title, durationMs);

    /// <summary>Affiche un toast erreur.</summary>
    public void ShowError(string message, string? title = null, int durationMs = 6000)
        => Add(ToastType.Error, message, title, durationMs);

    /// <summary>Affiche un toast avertissement.</summary>
    public void ShowWarning(string message, string? title = null, int durationMs = 5000)
        => Add(ToastType.Warning, message, title, durationMs);

    /// <summary>Affiche un toast information.</summary>
    public void ShowInfo(string message, string? title = null, int durationMs = 4000)
        => Add(ToastType.Info, message, title, durationMs);

    // ── Méthode centralisée depuis un CdmErrorCode ───────────────────────────

    /// <summary>
    /// Affiche un toast d'erreur à partir d'un <see cref="CdmErrorCode"/> typé.
    /// Le message est localisé via <paramref name="localizer"/> si fourni,
    /// sinon on utilise le nom de l'enum comme fallback.
    /// </summary>
    public void ShowFromError(
        CdmErrorCode code,
        IStringLocalizer? localizer = null,
        int durationMs = 6000)
    {
        var error = ErrorCatalog.Get(code);
        var message = localizer != null
            ? localizer[error.ResourceKey].Value
            : code.ToString().Replace("_", " ");

        Add(ToastType.Error, message, durationMs: durationMs);
    }

    /// <summary>
    /// Affiche un toast d'erreur à partir d'une chaîne de code (ex: valeur JSON de l'API).
    /// Utilise <see cref="ErrorCatalog.Parse"/> pour convertir la chaîne en <see cref="CdmErrorCode"/>.
    /// </summary>
    public void ShowFromErrorCode(
        string? rawCode,
        IStringLocalizer? localizer = null,
        int durationMs = 6000)
        => ShowFromError(ErrorCatalog.Parse(rawCode), localizer, durationMs);

    // ── Retrait ───────────────────────────────────────────────────────────────

    /// <summary>Retire un toast de la liste par son identifiant.</summary>
    public void Dismiss(Guid id)
    {
        var toast = _toasts.FirstOrDefault(t => t.Id == id);
        if (toast is not null)
        {
            _toasts.Remove(toast);
            NotifyChanged();
        }
    }

    // ── Interne ───────────────────────────────────────────────────────────────

    private void Add(ToastType type, string message, string? title = null, int durationMs = 4000)
    {
        _toasts.Add(new ToastMessage(Guid.NewGuid(), type, message, title, durationMs));
        NotifyChanged();
    }

    private void NotifyChanged() => OnChange?.Invoke();
}
