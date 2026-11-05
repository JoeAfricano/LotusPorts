namespace LotusPorts.Core.Preferences;

public enum ExperienceStyle
{
    Original,
    Remastered,
}

/// <summary>User preferences kept in the local database (Configurações).</summary>
public sealed record AppPreferences(string Language, string? LibraryFolder, ExperienceStyle DefaultStyle, bool AutoUpdate, bool AdvancedMode)
{
    public const string DefaultLanguage = "pt-BR";
    public static readonly IReadOnlyList<string> SupportedLanguages = ["pt-BR", "en-US"];

    /// <summary>Portuguese, library in the app folder, Original style, no automatic updates, simple mode.</summary>
    public static AppPreferences Default { get; } = new(DefaultLanguage, null, ExperienceStyle.Original, false, false);
}

public static class SettingKeys
{
    public const string Language = "general.language";
    public const string LibraryFolder = "general.libraryFolder";
    public const string DefaultStyle = "general.defaultStyle";
    public const string AutoUpdate = "general.autoUpdate";
    public const string AdvancedMode = "advanced.enabled";
}

public interface ISettingsStore
{
    Task<string?> GetAsync(string key, CancellationToken cancellationToken = default);
    Task SetAsync(string key, string value, CancellationToken cancellationToken = default);
}

/// <summary>Typed access to the preferences. Unknown or damaged values fall back to the defaults.</summary>
public sealed class PreferencesService(ISettingsStore store)
{
    public async Task<AppPreferences> LoadAsync(CancellationToken cancellationToken = default)
    {
        var d = AppPreferences.Default;
        var language = await store.GetAsync(SettingKeys.Language, cancellationToken).ConfigureAwait(false);
        var folder = await store.GetAsync(SettingKeys.LibraryFolder, cancellationToken).ConfigureAwait(false);
        var style = await store.GetAsync(SettingKeys.DefaultStyle, cancellationToken).ConfigureAwait(false);
        var autoUpdate = await store.GetAsync(SettingKeys.AutoUpdate, cancellationToken).ConfigureAwait(false);
        var advanced = await store.GetAsync(SettingKeys.AdvancedMode, cancellationToken).ConfigureAwait(false);
        return new AppPreferences(
            language is not null && AppPreferences.SupportedLanguages.Contains(language) ? language : d.Language,
            string.IsNullOrWhiteSpace(folder) ? null : folder,
            Enum.TryParse<ExperienceStyle>(style, out var s) && Enum.IsDefined(s) ? s : d.DefaultStyle,
            bool.TryParse(autoUpdate, out var a) ? a : d.AutoUpdate,
            bool.TryParse(advanced, out var adv) ? adv : d.AdvancedMode);
    }

    public Task SetLanguageAsync(string language, CancellationToken cancellationToken = default) =>
        AppPreferences.SupportedLanguages.Contains(language)
            ? store.SetAsync(SettingKeys.Language, language, cancellationToken)
            : throw new ArgumentException($"Unsupported language '{language}'.", nameof(language));

    public Task SetLibraryFolderAsync(string folder, CancellationToken cancellationToken = default) => store.SetAsync(SettingKeys.LibraryFolder, folder, cancellationToken);
    public Task SetDefaultStyleAsync(ExperienceStyle style, CancellationToken cancellationToken = default) => store.SetAsync(SettingKeys.DefaultStyle, style.ToString(), cancellationToken);
    public Task SetAutoUpdateAsync(bool enabled, CancellationToken cancellationToken = default) => store.SetAsync(SettingKeys.AutoUpdate, enabled.ToString(), cancellationToken);
    public Task SetAdvancedModeAsync(bool enabled, CancellationToken cancellationToken = default) => store.SetAsync(SettingKeys.AdvancedMode, enabled.ToString(), cancellationToken);
}
