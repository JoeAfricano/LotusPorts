using Lotus.Design.Themes;
using LotusPorts.Core.Preferences;
using LotusPorts.Infrastructure.Lotus;
using Microsoft.UI.Xaml;

namespace LotusPorts.App;

/// <summary>
/// Applies the chosen look with the family's theme system. "Igual ao Lotus" (default) reads Lotus's own choice — mode,
/// accent or custom theme — and follows it whenever the window comes back, so both apps always look the same.
/// </summary>
public sealed class AppearanceService(PreferencesService preferences, ISettingsStore settings)
{
    private readonly LotusProfileReader lotus = new();
    private ThemePalette? palette;
    private AppPreferences? current;

    public event Action? Changed;

    public ElementTheme Element => palette?.Element ?? ElementTheme.Dark;

    /// <summary>What is applied now: mode, accent and the custom theme (if any), after resolving "Igual ao Lotus".</summary>
    public (string Mode, string Accent, ThemeSpec? Custom) Effective { get; private set; } = ("Midnight", "rose", null);

    public async Task InitializeAsync(Microsoft.UI.Dispatching.DispatcherQueue dispatcher)
    {
        palette = new ThemePalette(Application.Current.Resources) { Dispatcher = dispatcher };
        palette.Changed += (_, _) => Changed?.Invoke();
        await ReloadAsync();
    }

    public async Task ReloadAsync()
    {
        current = await preferences.LoadAsync();
        Apply();
    }

    /// <summary>When following Lotus, its choice may have changed while this window was in the background.</summary>
    public void RefreshFromLotus()
    {
        if (current?.Theme == AppPreferences.FollowLotus) Apply();
    }

    /// <summary>The Home banners: the two scenes of the art package and its eight banners (id → app path).</summary>
    public static readonly IReadOnlyList<(string Id, string Path)> HomeBanners =
    [
        ("jardim", "Assets/Art/Scenes/05_FUNDO_JARDIM_CIBERNETICO_HERO_WIDE_1920x650.png"),
        ("santuario", "Assets/Art/Scenes/04_FUNDO_SANTUARIO_LUNAR_HERO_WIDE_1920x650.png"),
        ("sakura_temple", "Assets/Art/Banners/sakura_temple.png"),
        ("lunar_torii", "Assets/Art/Banners/lunar_torii.png"),
        ("lotus_river", "Assets/Art/Banners/lotus_river.png"),
        ("lantern_town", "Assets/Art/Banners/lantern_town.png"),
        ("aurora_peaks", "Assets/Art/Banners/aurora_peaks.png"),
        ("cosmic_orbit", "Assets/Art/Banners/cosmic_orbit.png"),
        ("purple_rift", "Assets/Art/Banners/purple_rift.png"),
        ("bamboo_path", "Assets/Art/Banners/bamboo_path.png"),
    ];

    private const string HomeBannerKey = "appearance.homeBanner";

    public async Task<string> HomeBannerAsync()
    {
        var id = await settings.GetAsync(HomeBannerKey);
        return HomeBanners.FirstOrDefault(b => b.Id == id).Path ?? HomeBanners[0].Path;
    }

    public async Task<string> HomeBannerIdAsync() => await settings.GetAsync(HomeBannerKey) is { } id && HomeBanners.Any(b => b.Id == id) ? id : HomeBanners[0].Id;

    public Task SetHomeBannerAsync(string id) => settings.SetAsync(HomeBannerKey, id);

    public async Task SetAsync(string theme, string accent, string? customCode)
    {
        await preferences.SetAppearanceAsync(theme, accent, customCode);
        await ReloadAsync();
    }

    private void Apply()
    {
        if (palette is null || current is null) return;
        var (mode, accent, code) = current.Theme == AppPreferences.FollowLotus
            ? lotus.ReadAppearance() is { } l ? (l.Theme, l.Accent, l.CustomTheme) : ("Midnight", "rose", null)
            : (current.Theme, current.Accent, current.CustomTheme);
#if DEBUG
        // --theme=Light (Debug only): preview a mode on screenshots without saving it.
        if (Environment.GetCommandLineArgs().FirstOrDefault(a => a.StartsWith("--theme=", StringComparison.Ordinal)) is { } preview)
            (mode, code) = (preview["--theme=".Length..], null);
#endif
        ThemeSpec? custom = null;
        if (mode == "Custom" && !ThemeEngine.TryFromCode(code, out custom)) (mode, custom) = ("Midnight", null);
        if (!ThemePalette.Modes.Contains(mode) && mode != "Custom") mode = "Midnight";
        var effective = (mode, accent, custom);
        if (Effective == effective && palette.Element == ElementFor(mode, custom)) return;
        Effective = effective;
        palette.Apply(mode, accent, custom);
    }

    private ElementTheme ElementFor(string mode, ThemeSpec? custom) => palette?.Element ?? (custom?.Light == true || mode == "Light" ? ElementTheme.Light : ElementTheme.Dark);
}
