using LotusPorts.Core.Preferences;
using LotusPorts.Core.Privacy;
using LotusPorts.Core.Search;

namespace LotusPorts.Core.Tests;

public class PreferencesTests
{
    private sealed class MemoryStore : ISettingsStore
    {
        public Dictionary<string, string> Values { get; } = [];
        public Task<string?> GetAsync(string key, CancellationToken cancellationToken = default) => Task.FromResult(Values.GetValueOrDefault(key));
        public Task SetAsync(string key, string value, CancellationToken cancellationToken = default) { Values[key] = value; return Task.CompletedTask; }
    }

    [Fact]
    public async Task Empty_store_gives_defaults()
    {
        var prefs = await new PreferencesService(new MemoryStore()).LoadAsync();
        prefs.Should().Be(AppPreferences.Default);
        prefs.Language.Should().Be("pt-BR");
    }

    [Fact]
    public async Task Values_round_trip()
    {
        var store = new MemoryStore();
        var service = new PreferencesService(store);
        await service.SetLanguageAsync("en-US");
        await service.SetLibraryFolderAsync(@"D:\Jogos\Ports");
        await service.SetDefaultStyleAsync(ExperienceStyle.Remastered);
        await service.SetAutoUpdateAsync(true);
        await service.SetAdvancedModeAsync(true);
        await service.SetShowInReviewAsync(true);

        (await service.LoadAsync()).Should().Be(new AppPreferences("en-US", @"D:\Jogos\Ports", ExperienceStyle.Remastered, true, true, true));
    }

    [Fact]
    public async Task Damaged_values_fall_back_to_defaults()
    {
        var store = new MemoryStore();
        store.Values[SettingKeys.Language] = "xx-YY";
        store.Values[SettingKeys.DefaultStyle] = "99";
        store.Values[SettingKeys.AutoUpdate] = "maybe";
        (await new PreferencesService(store).LoadAsync()).Should().Be(AppPreferences.Default);
    }

    [Fact]
    public async Task Appearance_defaults_to_lotus_and_round_trips()
    {
        var store = new MemoryStore();
        var service = new PreferencesService(store);
        (await service.LoadAsync()).Theme.Should().Be(AppPreferences.FollowLotus);

        await service.SetAppearanceAsync("Graphite", "violet", null);
        (await service.LoadAsync()).Should().Match<AppPreferences>(p => p.Theme == "Graphite" && p.Accent == "violet" && p.CustomTheme == null);

        await service.SetAppearanceAsync("Custom", "rose", "LOTUS-THEME:abc");
        (await service.LoadAsync()).CustomTheme.Should().Be("LOTUS-THEME:abc");

        // "Custom" without a code is not a theme: back to following Lotus.
        store.Values[SettingKeys.CustomTheme] = "";
        (await service.LoadAsync()).Theme.Should().Be(AppPreferences.FollowLotus);
        var act = () => service.SetAppearanceAsync("Neon", "rose", null);
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task Unsupported_language_is_rejected()
    {
        var act = () => new PreferencesService(new MemoryStore()).SetLanguageAsync("fr-FR");
        await act.Should().ThrowAsync<ArgumentException>();
    }
}

public class LibraryFolderRulesTests
{
    private static readonly SystemFolders Folders = new(
        [@"C:\Program Files", @"C:\Program Files (x86)", @"C:\Windows"],
        [@"C:\Users\ana\OneDrive"]);

    [Theory]
    [InlineData(@"D:\Jogos\Ports", LibraryFolderProblem.None)]
    [InlineData(@"C:\Users\ana\AppData\Local\LotusPorts\library", LibraryFolderProblem.None)]
    [InlineData(@"C:\Program Files Extra\Ports", LibraryFolderProblem.None)]
    [InlineData(@"relative\path", LibraryFolderProblem.NotAbsolute)]
    [InlineData("", LibraryFolderProblem.NotAbsolute)]
    [InlineData(@"D:\", LibraryFolderProblem.DriveRoot)]
    [InlineData(@"C:\Program Files\Ports", LibraryFolderProblem.ProtectedSystemFolder)]
    [InlineData(@"c:\windows", LibraryFolderProblem.ProtectedSystemFolder)]
    [InlineData(@"C:\Users\ana\OneDrive\Jogos", LibraryFolderProblem.SyncedByOneDrive)]
    public void Checks_folder(string path, LibraryFolderProblem expected) =>
        LibraryFolderRules.Check(path, Folders).Should().Be(expected);
}

public class QuickSearchTests
{
    private static readonly SearchEntry[] Entries =
    [
        new("settings", SearchEntryKind.Action, "Configurações", ["engrenagem", "opções"]),
        new("library", SearchEntryKind.Action, "Biblioteca", ["estante", "jogos"]),
        new("majoras-mask", SearchEntryKind.Game, "Majora's Mask", ["zelda"]),
    ];

    [Fact]
    public void Ignores_accents_and_case() =>
        QuickSearch.Filter(Entries, "CONFIGURACOES").Select(e => e.Id).Should().Equal("settings");

    [Fact]
    public void Matches_keywords() =>
        QuickSearch.Filter(Entries, "zelda").Select(e => e.Id).Should().Equal("majoras-mask");

    [Fact]
    public void Prefix_matches_come_first() =>
        QuickSearch.Filter(
            [new("sm64", SearchEntryKind.Game, "Super Mario 64", []), new("mk", SearchEntryKind.Game, "Mario Kart 64", [])],
            "mario").Select(e => e.Id).Should().Equal("mk", "sm64");

    [Fact]
    public void Empty_query_returns_nothing() =>
        QuickSearch.Filter(Entries, "  ").Should().BeEmpty();
}

public class PathAnonymizerTests
{
    [Fact]
    public void Replaces_profile_and_user_name() =>
        PathAnonymizer.Anonymize(@"Failed at C:\Users\ana\AppData\x and D:\Data\ana\y", "ana", @"C:\Users\ana")
            .Should().Be(@"Failed at %USERPROFILE%\AppData\x and D:\Data\<user>\y");

    [Fact]
    public void Leaves_text_without_paths_alone() =>
        PathAnonymizer.Anonymize("banana split", "ana", @"C:\Users\ana").Should().Be("banana split");
}
