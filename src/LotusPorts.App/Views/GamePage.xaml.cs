using System.Diagnostics;
using System.Globalization;
using Lotus.Design.Controls;
using LotusPorts.Catalog;
using LotusPorts.Core.Diagnostics;
using LotusPorts.Core.Guides;
using LotusPorts.Core.Install;
using LotusPorts.Core.Preferences;
using LotusPorts.Core.Verification;
using LotusPorts.Infrastructure.Diagnostics;
using LotusPorts.Infrastructure.Install;
using Microsoft.UI.Xaml.Automation;
using LotusPorts.Infrastructure.Catalog;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;

namespace LotusPorts.App.Views;

/// <summary>Página do jogo: what the game is, the recommended way to play it, and only verified facts about it.</summary>
public sealed partial class GamePage : Page
{
    private readonly CatalogService catalog = App.Services.GetRequiredService<CatalogService>();
    private readonly PlayService play = App.Services.GetRequiredService<PlayService>();
    private LibraryGame? game;
    private bool advanced;

    /// <summary>The game shown, so a file dropped on the window is checked against it.</summary>
    public LibraryGame? Game => game;
    private bool autoUpdate;

    public GamePage() => InitializeComponent();

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        play.Changed -= OnPlayChanged;
        bridge.Changed -= OnBridgeChanged;
        gameFiles.Changed -= OnGameFilesChanged;
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        var preferences = await App.Services.GetRequiredService<PreferencesService>().LoadAsync();
        advanced = preferences.AdvancedMode;
        autoUpdate = preferences.AutoUpdate;
        game = e.Parameter is string id ? catalog.Library.Find(id) : null;
        if (game is null || game.Visibility == GameVisibility.Hidden)
        {
            NotFound.Visibility = Visibility.Visible;
            return;
        }
        Details.Visibility = Visibility.Visible;
        Show(game);
        // Start at the top even after the sections above were filled in.
        DispatcherQueue.TryEnqueue(() => Scroller.ChangeView(null, 0, null, disableAnimation: true));
        play.Changed += OnPlayChanged;
        gameFiles.Changed += OnGameFilesChanged;
        bridge.Changed += OnBridgeChanged;
        _ = RefreshExperienceAsync();
        RefreshPlay();
        RefreshStyle();
        RefreshMods();
        var guide = ShowGuideAsync(game.Recommended.Port);
        _ = ShowScreenshotsAsync(game.Recommended.Port);
        await LookupAsync();
        RefreshPlay();
        await guide;
    }

    private static Style StyleOf(string key) => (Style)Application.Current.Resources[key];

    private static Brush Brush(string key) => (Brush)Application.Current.Resources[key];

    private void Show(LibraryGame g)
    {
        var port = g.Recommended.Port;
        Cover.Title = g.Game.Title;
        Cover.PlatformKey = g.Game.OriginalPlatform.ToString();
        Cover.PlatformLabel = CatalogLabels.Platform(g.Game.OriginalPlatform);
        Cover.CoverUrl = g.Game.Cover?.Url;
        TitleText.Text = g.Game.Title;
        Description.Text = g.Game.DescriptionPtBr;

        Badges.Children.Add(new StateBadge { Text = CatalogLabels.Status(port.Status), Tone = CatalogLabels.StatusTone(port.Status) });
        Badges.Children.Add(new StateBadge { Text = CatalogLabels.Platform(g.Game.OriginalPlatform), Tone = BadgeTone.Accent });
        if (CatalogLabels.Origin(g) is { } origin) Badges.Children.Add(new StateBadge { Text = origin, Tone = BadgeTone.Unknown });
        _ = ShowPerformanceBadgeAsync(g.Id);

        var favorite = catalog.StateOf(g.Id).Favorite;
        FavoriteButton.IsChecked = favorite;
        FavoriteIcon.Glyph = favorite ? "\uEB52" : "\uEB51";

        MoreMenu.Items.Add(Link(Strings.Get("GameOpenRepo"), port.Links.Repo));
        if (port.Links.Docs is { } docs && docs != port.Links.Repo) MoreMenu.Items.Add(Link(Strings.Get("GameOpenDocs"), docs));

        RecommendedTitle.Text = port.Title;
        RecommendedReason.Text = g.Game.RecommendationReasonPtBr
            ?? (g.Alternatives.Count == 0 ? Strings.Get(port.Status == PortStatus.EM_DESENVOLVIMENTO ? "GameOnlyPortBuilding" : "GameOnlyPort") : Strings.Get("GameReasonMissing"));

        // Nothing below has been checked on this PC yet: every tile says so instead of showing OK.
        Tiles.Children.Add(GameFileTile());
        Tiles.Children.Add(Tile(1, "\uE74C", Strings.Get("TileDependencies"), Strings.Get("TileNotVerified"), BadgeTone.Unknown));
        var lotusTile = Tile(2, null, Strings.Get("TileLotus"), "", BadgeTone.Unknown);
        lotusBadge = ((lotusTile.Child as StackPanel)?.Children.OfType<StackPanel>().FirstOrDefault())?.Children.OfType<StateBadge>().FirstOrDefault();
        Tiles.Children.Add(lotusTile);
        RefreshLotusTile();

        AddAbout(port, g);

        var others = g.Alternatives;
        OtherWays.Header = Strings.Format(others.Count == 1 ? "GameOtherWaysOne" : "GameOtherWaysMany", others.Count);
        OtherWays.Visibility = others.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
        foreach (var choice in others) OtherWaysList.Children.Add(Alternative(choice));
    }

    private Border Tile(int column, string? glyph, string title, string state, BadgeTone tone)
    {
        UIElement icon = glyph is null
            ? new LotusPetal { IsLit = false, Width = 26, Height = 22, VerticalAlignment = VerticalAlignment.Top }
            : new FontIcon { Glyph = glyph, FontSize = 22, Foreground = Brush("Text.Secondary"), VerticalAlignment = VerticalAlignment.Top, Margin = new Thickness(0, 2, 0, 0) };
        var tile = new Border
        {
            Style = StyleOf("Lotus.Card"),
            Padding = new Thickness(18, 14, 18, 14),
            Child = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 14,
                Children =
                {
                    icon,
                    new StackPanel { Spacing = 4, Children = { new TextBlock { Text = title, Style = StyleOf("Type.Body") }, new StateBadge { Text = state, Tone = tone } } },
                },
            },
        };
        Grid.SetColumn(tile, column);
        return tile;
    }

    private void AddAbout(Port port, LibraryGame g)
    {
        Row(Strings.Get("AboutPortType"), CatalogLabels.PortType(port.PortType));
        if (port.Evidence is { ReleaseTag: { } tag } evidence)
            Row(Strings.Get("AboutVersion"), evidence.Prerelease ? Strings.Format("AboutVersionPre", tag) : tag);
        Row(Strings.Get("AboutLicense"), port.License ?? Strings.Get("AboutLicenseUnknown"));
        Row(Strings.Get("AboutSetup"), port.Setup.NotesPtBr ?? CatalogLabels.Setup(port.Setup.Kind));

        var files = port.RequiredGameFiles;
        Row(Strings.Get("AboutGameFile"), files.Count == 0 ? Strings.Get("AboutGameFileUnknown") : string.Join("; ", files.Select(f => f.Label)));
        var versions = files.SelectMany(f => f.AcceptedVersions).ToList();
        if (versions.Count > 0)
        {
            Row(Strings.Get("AboutAcceptedVersions"), string.Join("\n", versions.Select(v =>
            {
                var label = v.Version.StartsWith("PREENCHER", StringComparison.Ordinal) ? v.Region : $"{v.Region} {v.Version}";
                if (!advanced) return label;
                var hash = v.Sha1 is { } s ? $"SHA-1 {s}" : v.Md5 is { } m ? $"MD5 {m}" : v.Sha256 is { } h ? $"SHA-256 {h}" : "";
                return $"{label}  {hash}";
            })));
        }
        if (port.KnownIssuesPtBr is { Count: > 0 } issues) Row(Strings.Get("AboutKnownIssues"), string.Join("\n", issues.Select(i => "• " + i)));
        if (port.VerifiedBy is { } verifiedBy)
        {
            var when = DateOnly.TryParseExact(port.VerifiedAt, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out var date) ? date.ToString("d", System.Globalization.CultureInfo.CurrentUICulture) : null;
            Row(Strings.Get("AboutEvaluated"), when is null ? verifiedBy : Strings.Format("AboutEvaluatedValue", verifiedBy, when));
        }
        if (g.Visibility == GameVisibility.InReview && g.Game.Review.NotesPtBr is { } review) Row(Strings.Get("AboutReview"), review);
        if (advanced) Row(Strings.Get("AboutRepository"), $"{port.Repo.Owner}/{port.Repo.Name}");

        var links = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
        links.Children.Add(LinkButton(Strings.Get("GameOpenRepo"), port.Links.Repo));
        if (port.Links.Docs is { } docs && docs != port.Links.Repo) links.Children.Add(LinkButton(Strings.Get("GameOpenDocs"), docs));
        About.Children.Add(links);
    }

    private void Row(string label, string value)
    {
        var grid = new Grid { ColumnSpacing = 16 };
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(220) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.Children.Add(new TextBlock { Text = label, Style = StyleOf("Type.Caption") });
        var text = new TextBlock { Text = value, Style = StyleOf("Type.Body"), IsTextSelectionEnabled = true };
        Grid.SetColumn(text, 1);
        grid.Children.Add(text);
        About.Children.Add(grid);
    }

    private Border Alternative(PortChoice choice)
    {
        var title = new TextBlock { Style = StyleOf("Type.Body") };
        title.Inlines.Add(new Run { Text = choice.Port.Title, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold });
        var badges = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
        badges.Children.Add(new StateBadge { Text = CatalogLabels.Status(choice.Port.Status), Tone = CatalogLabels.StatusTone(choice.Port.Status) });
        badges.Children.Add(new StateBadge { Text = CatalogLabels.Origin(choice), Tone = choice.Trusted ? BadgeTone.Accent : BadgeTone.Unknown });
        var panel = new StackPanel { Spacing = 6, Children = { title, badges } };
        if (choice.DifferencePtBr is { } difference) panel.Children.Add(new TextBlock { Text = difference, Style = StyleOf("Type.Caption") });
        panel.Children.Add(LinkButton(Strings.Get("GameOpenRepo"), choice.Port.Links.Repo));
        return new Border { Background = Brush("Bg.Elevated"), CornerRadius = new CornerRadius(10), Padding = new Thickness(16, 12, 16, 12), Child = panel };
    }

    private static HyperlinkButton LinkButton(string text, string url) => new() { Content = text, NavigateUri = new Uri(url), Padding = new Thickness(0, 4, 12, 4) };

    private static MenuFlyoutItem Link(string text, string url)
    {
        var item = new MenuFlyoutItem { Text = text, Icon = new FontIcon { Glyph = "\uE8A7" } };
        item.Click += async (_, _) => await Windows.System.Launcher.LaunchUriAsync(new Uri(url));
        return item;
    }

    private async void OnFavorite(object sender, RoutedEventArgs e)
    {
        if (game is null) return;
        var favorite = FavoriteButton.IsChecked == true;
        FavoriteIcon.Glyph = favorite ? "\uEB52" : "\uEB51";
        await catalog.SetFavoriteAsync(game.Id, favorite);
    }

    private void OnBack(object sender, RoutedEventArgs e) => App.MainWindow?.GoBack();

    // ---- Style (phase 4) ----------------------------------------------------------------------

    private void RefreshStyle()
    {
        if (game is null) return;
        var port = game.Recommended.Port;
        var installed = play.StateOf(port).Installed;
        StyleCard.Visibility = installed is not null && StyleService.CanOffer(port) ? Visibility.Visible : Visibility.Collapsed;
        if (installed is null) return;
        var current = installed.Style is { } s && s.Tag == installed.ActiveTag ? s.Name : null;
        StyleOriginal.IsChecked = current == nameof(ExperienceStyle.Original);
        StyleRemastered.IsChecked = current == nameof(ExperienceStyle.Remastered);
        StyleUndo.Visibility = current is null ? Visibility.Collapsed : Visibility.Visible;
        StyleStatus.Text = Strings.Get(current is null ? "StyleNone" : "StyleApplied");
    }

    private async Task ApplyStyleAsync(ExperienceStyle style)
    {
        var port = game!.Recommended.Port;
        var name = Strings.Get(style == ExperienceStyle.Remastered ? "StyleRemastered" : "StyleOriginal");
        if (await Confirm(Strings.Format("StyleConfirmTitle", name), Strings.Get("StyleConfirmBody"), Strings.Get("StyleApply")))
        {
            try
            {
                var applied = await App.Services.GetRequiredService<StyleService>().ApplyAsync(port, style);
                _ = App.Services.GetRequiredService<LotusBridge>().NotifyAsync(Lotus.Integration.Contracts.Events.PresetApplied, new(port.GameId, port.Id, applied.ActiveTag));
            }
            catch (Exception ex) when (ex is StyleException or IOException or UnauthorizedAccessException or System.Text.Json.JsonException or InvalidDataException)
            {
                StyleStatus.Text = Strings.Get(ex is StyleException { Problem: StyleProblem.ConfigMissing } ? "StyleOpenFirst" : "StyleFailed");
                return;
            }
        }
        RefreshStyle();
    }

    private async void OnStyleOriginal(object sender, RoutedEventArgs e) => await ApplyStyleAsync(ExperienceStyle.Original);

    private async void OnStyleRemastered(object sender, RoutedEventArgs e) => await ApplyStyleAsync(ExperienceStyle.Remastered);

    private async void OnStyleUndo(object sender, RoutedEventArgs e)
    {
        try { await App.Services.GetRequiredService<StyleService>().RevertAsync(game!.Recommended.Port); }
        catch (Exception ex) when (ex is StyleException or IOException or UnauthorizedAccessException)
        {
            StyleStatus.Text = Strings.Get("StyleFailed");
            return;
        }
        RefreshStyle();
    }

    // ---- Mods (phase 8, experimental) ----------------------------------------------------------

    private readonly ModService mods = App.Services.GetRequiredService<ModService>();

    private void RefreshMods()
    {
        if (game is null) return;
        var port = game.Recommended.Port;
        var state = play.StateOf(port);
        var installed = state.Installed;
        ModsCard.Visibility = ModService.Offers(port) && installed is not null ? Visibility.Visible : Visibility.Collapsed;
        if (installed is null || port.Mods is not { } declared) return;

        var busy = state.RunningSince is not null || state.Progress is not null;
        var accepted = string.Join(", ", AcceptedTypes(port));
        ModsHint.Text = (declared.NotesPtBr is { } notes ? notes + " " : "") + (declared.FolderMarker is { } marker
            ? Strings.Format("ModsTypesFolder", accepted, marker)
            : Strings.Format("ModsTypes", accepted));
        ModsSource.Visibility = declared.SourceUrl is null ? Visibility.Collapsed : Visibility.Visible;
        if (declared.SourceUrl is { } source) ModsSource.NavigateUri = new Uri(source);
        ModsAdd.IsEnabled = ModsAddFolder.IsEnabled = !busy;
        ModsAddFolder.Visibility = ModService.TakesFolders(port) ? Visibility.Visible : Visibility.Collapsed;

        ModsList.Children.Clear();
        var list = mods.List(port, installed);
        if (list.Count == 0) ModsList.Children.Add(new TextBlock { Text = Strings.Get("ModsEmpty"), Style = StyleOf("Type.Caption") });
        foreach (var mod in list)
        {
            var row = new Grid { ColumnSpacing = 12, Padding = new Thickness(14, 8, 14, 8), Background = Brush("Bg.Elevated"), CornerRadius = new CornerRadius(10) };
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            row.Children.Add(new StackPanel
            {
                Spacing = 2,
                Children =
                {
                    new TextBlock { Text = mod.Name, Style = StyleOf("Type.Body"), TextTrimming = TextTrimming.CharacterEllipsis },
                    new TextBlock { Text = mod.IsFolder ? Strings.Format("ModsFolderSize", Size(mod.Size)) : Size(mod.Size), Style = StyleOf("Type.Caption") },
                },
            });
            var badge = new StateBadge
            {
                Text = Strings.Get(mod.State == ModState.Enabled ? "ModsOn" : "ModsOff"),
                Tone = mod.State == ModState.Enabled ? BadgeTone.Ok : BadgeTone.Unknown,
                VerticalAlignment = VerticalAlignment.Center,
            };
            Grid.SetColumn(badge, 1);
            row.Children.Add(badge);
            var actions = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 2, VerticalAlignment = VerticalAlignment.Center };
            var name = mod.Name;
            var toggle = Action(Strings.Get(mod.State == ModState.Enabled ? "ModsDisable" : "ModsEnable"), mod.State == ModState.Enabled
                ? () => ModActionAsync(() => mods.DisableAsync(port, installed, name))
                : () => ModActionAsync(() => mods.EnableAsync(port, installed, name)));
            var remove = Action(Strings.Get("ModsRemove"), () => RemoveModAsync(port, installed, name));
            toggle.IsEnabled = remove.IsEnabled = !busy;
            actions.Children.Add(toggle);
            actions.Children.Add(remove);
            Grid.SetColumn(actions, 2);
            row.Children.Add(actions);
            ModsList.Children.Add(row);
        }
        ModsStatus.Text = busy ? Strings.Get("ModsCloseGame") : "";
    }

    private static IEnumerable<string> AcceptedTypes(Port port) =>
        ModService.UnpacksZip(port) ? port.Mods!.FileTypes.Append(".zip") : port.Mods!.FileTypes;

    private async Task<bool> ModActionAsync(Func<Task> action)
    {
        var ok = false;
        try { await action(); ModsStatus.Text = ""; ok = true; }
        catch (ModException ex) when (ex.Problem == ModProblem.NotAMod && game?.Recommended.Port.Mods?.FolderMarker is { } marker)
        {
            ModsStatus.Text = Strings.Format("ModsNotAMod", marker);
        }
        catch (Exception ex) when (ex is ModException or IOException or UnauthorizedAccessException or InvalidDataException)
        {
            ModsStatus.Text = Strings.Get("ModsFailed");
        }
        RefreshMods();
        return ok;
    }

    private async void OnAddMod(object sender, RoutedEventArgs e)
    {
        if (game is null || play.StateOf(game.Recommended.Port).Installed is not { } installed || game.Recommended.Port.Mods is null) return;
        var port = game.Recommended.Port;
        var picker = new Windows.Storage.Pickers.FileOpenPicker { SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Downloads };
        foreach (var type in AcceptedTypes(port)) picker.FileTypeFilter.Add(type);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow));
        if (await picker.PickSingleFileAsync() is not { } file) return;
        var body = Strings.Format("ModsConfirmBody", file.Name, mods.ModsFolder(port, installed) ?? "");
        if (!await Confirm(Strings.Get("ModsConfirmTitle"), body, Strings.Get("ModsAdd"))) return;
        if (await ModActionAsync(() => mods.InstallAsync(port, installed, file.Path))) ModsStatus.Text = Strings.Format("ModsAdded", file.Name);
    }

    private async void OnAddModFolder(object sender, RoutedEventArgs e)
    {
        if (game is null || play.StateOf(game.Recommended.Port).Installed is not { } installed || !ModService.TakesFolders(game.Recommended.Port)) return;
        var port = game.Recommended.Port;
        var picker = new Windows.Storage.Pickers.FolderPicker { SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Downloads };
        picker.FileTypeFilter.Add("*");
        WinRT.Interop.InitializeWithWindow.Initialize(picker, WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow));
        if (await picker.PickSingleFolderAsync() is not { } folder) return;
        var body = Strings.Format("ModsConfirmFolderBody", folder.Name, mods.ModsFolder(port, installed) ?? "");
        if (!await Confirm(Strings.Get("ModsConfirmTitle"), body, Strings.Get("ModsAdd"))) return;
        if (await ModActionAsync(() => mods.InstallFolderAsync(port, installed, folder.Path))) ModsStatus.Text = Strings.Format("ModsAdded", folder.Name);
    }

    private async Task RemoveModAsync(Port port, InstalledPort installed, string name)
    {
        if (!await Confirm(Strings.Format("ModsRemoveTitle", name), Strings.Get("ModsRemoveBody"), Strings.Get("ModsRemove"))) return;
        await ModActionAsync(() => mods.RemoveAsync(port, installed, name));
    }

    // ---- Sua experiência (phase 6) -------------------------------------------------------------

    private readonly LotusBridge bridge = App.Services.GetRequiredService<LotusBridge>();
    private Lotus.Integration.Contracts.Recommendation? suggestion;
    private bool showingExperience;

    private async Task RefreshExperienceAsync()
    {
        if (game is null || showingExperience) return;
        showingExperience = true;
        try
        {
            var g = game;
            var connected = bridge.Status.State == LotusPorts.Integration.LotusConnectionState.Connected;
            ExperienceCard.Visibility = connected ? Visibility.Visible : Visibility.Collapsed;
            if (!connected) return;

            ProfileToggle.IsOn = bridge.IsAllowed(LotusPermission.PerformanceProfile);
            ProfileToggle.IsEnabled = bridge.Status.Capabilities.Contains(Lotus.Integration.Contracts.Methods.SessionStart);
            LotusProfileLink.NavigateUri = new Uri("lotus://profile/" + Uri.EscapeDataString(g.Game.LotusGameKey));

            var drive = bridge.LastDriverChange;
            ExperienceDriver.Visibility = drive is not null && DateTimeOffset.UtcNow - drive.At < TimeSpan.FromDays(3) ? Visibility.Visible : Visibility.Collapsed;
            if (drive is not null) ExperienceDriver.Text = Strings.Format("ExperienceDriverChanged", drive.At.ToLocalTime().ToString("g", CultureInfo.CurrentCulture));

            var sessions = await bridge.SessionsAsync(g.Id);
            var report = sessions.Count == 0 ? null : await bridge.ReportAsync(sessions[0]);
            if (report is { Measured: true, AverageFps: { } fps }) await RememberPerformanceAsync(g.Id, fps, report.OnePercentLowFps);
            ExperienceSession.Text = report is null ? Strings.Get(sessions.Count == 0 ? "ExperienceNoSession" : "ExperienceNoReport") : SessionLine(report);
            LotusReportLink.Visibility = sessions.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
            if (sessions.Count > 0) LotusReportLink.NavigateUri = new Uri("lotus://session/" + Uri.EscapeDataString(sessions[0]));

            suggestion = (await bridge.RecommendationsAsync(g.Game.LotusGameKey, sessions)).FirstOrDefault();
            SuggestionBox.Visibility = suggestion is null ? Visibility.Collapsed : Visibility.Visible;
            if (suggestion is null) return;
            SuggestionText.Text = suggestion.TextPtBr;
            SuggestionEvidence.Text = Strings.Format("ExperienceEvidence", suggestion.EvidencePtBr);
            SuggestionComparison.Text = suggestion.ComparisonPtBr is { } c ? Strings.Format("ExperienceComparison", c) : "";
            SuggestionComparison.Visibility = suggestion.ComparisonPtBr is null ? Visibility.Collapsed : Visibility.Visible;
            // "Aplicar" only for a style this port really supports (verified keys) and when installed.
            var installed = play.StateOf(g.Recommended.Port).Installed;
            SuggestionApply.Visibility = SuggestedStyle() is { } style && StyleService.CanOffer(g.Recommended.Port) && installed is not null
                && installed.Style?.Name != style.ToString() ? Visibility.Visible : Visibility.Collapsed;
        }
        finally
        {
            showingExperience = false;
        }
    }

    /// <summary>Only measured numbers; when Lotus did not measure, no number at all (section 12.6).</summary>
    private static string SessionLine(Lotus.Integration.Contracts.SessionReport report)
    {
        if (!report.Measured) return Strings.Get("ExperienceNotMeasured");
        var parts = new List<string> { Strings.Format("ExperienceDuration", Duration(report.DurationSeconds)) };
        if (report.AverageFps is { } avg) parts.Add(Strings.Format("ExperienceAverage", avg.ToString("0", CultureInfo.CurrentCulture)));
        if (report.OnePercentLowFps is { } low) parts.Add(Strings.Format("ExperienceLow", low.ToString("0", CultureInfo.CurrentCulture)));
        if (report.Stutters is { } stutters) parts.Add(Strings.Format(stutters == 1 ? "ExperienceStutter" : "ExperienceStutters", stutters));
        if (report.MaxGpuTemperature is { } gpu) parts.Add(Strings.Format("ExperienceGpuTemp", gpu.ToString("0", CultureInfo.CurrentCulture)));
        return Strings.Format("ExperienceLastSession", string.Join(" · ", parts));
    }

    private static string Duration(double seconds)
    {
        var t = TimeSpan.FromSeconds(seconds);
        return t.TotalHours >= 1 ? $"{(int)t.TotalHours}h{t.Minutes:00}" : $"{Math.Max(1, (int)t.TotalMinutes)} min";
    }

    private ExperienceStyle? SuggestedStyle() => suggestion?.Style is { } s && Enum.TryParse<ExperienceStyle>(s, out var style) ? style : null;

    private async void OnApplySuggestion(object sender, RoutedEventArgs e)
    {
        if (SuggestedStyle() is { } style) await ApplyStyleAsync(style);
        await RefreshExperienceAsync();
    }

    private async void OnProfileToggled(object sender, RoutedEventArgs e)
    {
        if (ProfileToggle.IsOn != bridge.IsAllowed(LotusPermission.PerformanceProfile))
            await bridge.SetAllowedAsync(LotusPermission.PerformanceProfile, ProfileToggle.IsOn);
    }

    private void OnBridgeChanged()
    {
        RefreshLotusTile();
        _ = RefreshExperienceAsync();
    }

    private StateBadge? lotusBadge;

    /// <summary>The Lotus tile says the real connection state (the same words as the Integração com o Lotus screen).</summary>
    private void RefreshLotusTile()
    {
        if (lotusBadge is null) return;
        var status = bridge.Status;
        lotusBadge.Text = status.State == LotusPorts.Integration.LotusConnectionState.Connected
            ? Strings.Format("LotusStateConnected", status.Version ?? "?")
            : Strings.Get("LotusState" + status.State);
        lotusBadge.Tone = status.State switch
        {
            LotusPorts.Integration.LotusConnectionState.Connected => BadgeTone.Ok,
            LotusPorts.Integration.LotusConnectionState.Incompatible => BadgeTone.Warning,
            LotusPorts.Integration.LotusConnectionState.Untrusted => BadgeTone.Failure,
            _ => BadgeTone.Unknown,
        };
    }

    // ---- Game file (phase 3) ------------------------------------------------------------------

    private readonly GameFilesService gameFiles = App.Services.GetRequiredService<GameFilesService>();
    private readonly StateBadge fileBadge = new();
    private readonly HyperlinkButton fileAction = new() { Padding = new Thickness(0, 2, 0, 2) };
    private readonly TextBlock fileAccepted = new() { TextWrapping = TextWrapping.Wrap, MaxWidth = 280 };

    private Border GameFileTile()
    {
        fileAction.Click += OnAddGameFile;
        var tile = new Border
        {
            Style = StyleOf("Lotus.Card"),
            Padding = new Thickness(18, 14, 18, 14),
            Child = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 14,
                Children =
                {
                    new FontIcon { Glyph = "", FontSize = 22, Foreground = Brush("Text.Secondary"), VerticalAlignment = VerticalAlignment.Top, Margin = new Thickness(0, 2, 0, 0) },
                    new StackPanel { Spacing = 4, Children = { new TextBlock { Text = Strings.Get("TileGameFile"), Style = StyleOf("Type.Body") }, fileBadge, fileAccepted, fileAction } },
                },
            },
        };
        RefreshFileTile();
        return tile;
    }

    /// <summary>Before picking a file: which copy of the game this port takes ("Aceita: EUA 1.0"), so a wrong one is not picked.</summary>
    private void ShowAccepted(Port port)
    {
        var accepted = GameFileMatcher.AcceptedLabels(port);
        var label = port.RequiredGameFiles.FirstOrDefault()?.Label;
        fileAccepted.Style = StyleOf("Type.Caption");
        fileAccepted.Text = port.RequiredGameFiles.Count == 0 ? ""
            : accepted.Count > 0 ? Strings.Format("TileFileAccepts", string.Join(", ", accepted))
            : Strings.Format("TileFileAcceptsUnlisted", label ?? "");
        fileAccepted.Visibility = fileAccepted.Text.Length == 0 ? Visibility.Collapsed : Visibility.Visible;
    }

    private void RefreshFileTile()
    {
        if (game is null) return;
        ShowAccepted(game.Recommended.Port);
        if (gameFiles.MatchFor(game.Id) is { } match)
        {
            fileBadge.Text = Strings.Format("TileFileRecognized", match.VersionLabel);
            fileBadge.Tone = BadgeTone.Ok;
            var source = gameFiles.Files.FirstOrDefault(f => f.Matches.Contains(match))?.Info.Path;
            ToolTipService.SetToolTip(fileBadge, source);
            // Ports that read the file from their own folder: one click puts it there.
            fileAction.Content = Strings.Get(NeedsPlacing(source) ? "TileFilePlace" : PlacedHere(source) ? "TileFilePlaced" : "TileFileShow");
            return;
        }
        var searched = gameFiles.Folders.Count > 0 && !gameFiles.Scanning;
        fileBadge.Text = Strings.Get(gameFiles.Scanning ? "TileFileSearching" : searched ? "TileFileMissing" : "TileNotVerified");
        fileBadge.Tone = searched ? BadgeTone.Warning : BadgeTone.Unknown;
        fileAction.Content = Strings.Get("PlayAddGameFile");
    }

    // ---- "Funciona no seu PC": the last FPS Lotus measured for this game, kept on this PC ----------

    private sealed record MeasuredPerformance(double Fps, double? Low, DateTimeOffset At);

    private static string PerformanceKey(string gameId) => "performance." + gameId;

    private static async Task RememberPerformanceAsync(string gameId, double fps, double? low)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(new MeasuredPerformance(Math.Round(fps, 1), low, DateTimeOffset.UtcNow));
        await App.Services.GetRequiredService<ISettingsStore>().SetAsync(PerformanceKey(gameId), json);
    }

    /// <summary>Shown only for a real measurement (Lotus never sends invented numbers): "Funciona no seu PC · 58 FPS".</summary>
    private async Task ShowPerformanceBadgeAsync(string gameId)
    {
        MeasuredPerformance? measured;
        try
        {
            measured = await App.Services.GetRequiredService<ISettingsStore>().GetAsync(PerformanceKey(gameId)) is { } json
                ? System.Text.Json.JsonSerializer.Deserialize<MeasuredPerformance>(json) : null;
        }
        catch (System.Text.Json.JsonException) { measured = null; }
        if (measured is null || game?.Id != gameId) return;
        var badge = new StateBadge
        {
            Text = Strings.Format("PerfBadge", measured.Fps.ToString("0", CultureInfo.CurrentCulture)),
            Tone = measured.Fps >= 50 ? BadgeTone.Ok : measured.Fps >= 28 ? BadgeTone.Warning : BadgeTone.Failure,
        };
        ToolTipService.SetToolTip(badge, Strings.Format("PerfBadgeTip", measured.At.ToLocalTime().ToString("d", CultureInfo.CurrentCulture),
            measured.Low is { } low ? low.ToString("0", CultureInfo.CurrentCulture) : "—"));
        Badges.Children.Add(badge);
    }

    private readonly GameFilePlacer placer = App.Services.GetRequiredService<GameFilePlacer>();

    private bool PlacedHere(string? source) =>
        source is not null && !LotusPorts.Core.GameFiles.GameFileZip.IsInsideZip(source) && game is not null && GameFilePlacer.Offers(game.Recommended.Port)
        && play.StateOf(game.Recommended.Port).Installed is { } installed && placer.IsPlaced(game.Recommended.Port, installed, source);

    private bool NeedsPlacing(string? source) =>
        source is not null && !LotusPorts.Core.GameFiles.GameFileZip.IsInsideZip(source) && game is not null && GameFilePlacer.Offers(game.Recommended.Port)
        && play.StateOf(game.Recommended.Port).Installed is { } installed && !placer.IsPlaced(game.Recommended.Port, installed, source);

    private async Task PlaceGameFileAsync(KnownGameFile known)
    {
        var port = game!.Recommended.Port;
        if (play.StateOf(port).Installed is not { } installed) return;
        var target = placer.TargetPath(port, installed, known.Info.Path);
        if (!await Confirm(Strings.Get("PlaceTitle"), Strings.Format("PlaceBody", Path.GetFileName(known.Info.Path), target), Strings.Get("PlaceConfirm"))) return;
        try { await placer.PlaceAsync(port, installed, known.Info); }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidDataException or InvalidOperationException)
        {
            await new ContentDialog { XamlRoot = XamlRoot, Title = Strings.Get("PlaceFailedTitle"), Content = Strings.Get("PlaceFailedBody"), CloseButtonText = Strings.Get("Close") }.ShowAsync();
        }
        RefreshFileTile();
    }

    private void OnGameFilesChanged()
    {
        if (DispatcherQueue.HasThreadAccess) RefreshFileTile();
        else DispatcherQueue.TryEnqueue(RefreshFileTile);
    }

    private async void OnAddGameFile(object sender, RoutedEventArgs e)
    {
        if (game is null) return;
        if (gameFiles.MatchFor(game.Id) is { } match && gameFiles.Files.FirstOrDefault(f => f.Matches.Contains(match)) is { } known)
        {
            if (NeedsPlacing(known.Info.Path)) { await PlaceGameFileAsync(known); return; }
            // Already recognized: show where it is (Explorer selects it; nothing is moved).
            Process.Start(new ProcessStartInfo("explorer.exe", $"/select,\"{LotusPorts.Core.GameFiles.GameFileZip.ContainerOf(known.Info.Path)}\"") { UseShellExecute = true })?.Dispose();
            return;
        }
        var picker = new Windows.Storage.Pickers.FileOpenPicker { SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.ComputerFolder };
        picker.FileTypeFilter.Add("*");
        WinRT.Interop.InitializeWithWindow.Initialize(picker, WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow));
        if (await picker.PickSingleFileAsync() is not { } file) return;
        await IdentifyAndTellAsync(file.Path, game, XamlRoot);
    }

    /// <summary>Reads a file, then says in plain words what it is (also used when a file is dropped on the window).</summary>
    public static async Task IdentifyAndTellAsync(string path, LibraryGame? forGame, XamlRoot root)
    {
        var service = App.Services.GetRequiredService<GameFilesService>();
        KnownGameFile known;
        try { known = await service.IdentifyAsync(path); }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            await new ContentDialog { XamlRoot = root, Title = Strings.Get("GameFileUnreadableTitle"), Content = Strings.Get("GameFileUnreadableBody"), CloseButtonText = Strings.Get("Close") }.ShowAsync();
            return;
        }
        var message = GameFileMessages.Describe(known, forGame);
        var dialog = new ContentDialog
        {
            XamlRoot = root,
            Title = message.Title,
            Content = new TextBlock { Text = message.Body, TextWrapping = TextWrapping.Wrap, IsTextSelectionEnabled = true },
            CloseButtonText = Strings.Get("Close"),
            DefaultButton = ContentDialogButton.Close,
        };
        var docs = forGame?.Recommended.Port.Links.Docs;
        if (!message.Recognized && docs is not null)
        {
            dialog.SecondaryButtonText = Strings.Get("GameOpenDocs");
            dialog.SecondaryButtonClick += async (_, _) => await Windows.System.Launcher.LaunchUriAsync(new Uri(docs));
        }
        if (message.Match is { } m && (forGame is null || m.Game.Id != forGame.Id))
        {
            dialog.PrimaryButtonText = Strings.Format("GameFileOpenGame", m.Game.Game.Title);
            dialog.PrimaryButtonClick += (_, _) => App.MainWindow?.Navigate(Pages.Game, m.Game.Id);
        }
        await dialog.ShowAsync();
    }

    // ---- How to play: from the port's README on GitHub ----------------------------------------

    /// <summary>Up to six pictures from the port's README; a click opens the picture in the browser, full size.</summary>
    private async Task ShowScreenshotsAsync(Port port)
    {
        IReadOnlyList<Uri> shots;
        try { shots = await App.Services.GetRequiredService<PortGuideService>().ScreenshotsAsync(port); }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { shots = []; }
        if (game?.Recommended.Port.Id != port.Id) return;
        Shots.Children.Clear();
        foreach (var url in shots)
        {
            var image = new Image { Stretch = Stretch.UniformToFill, Source = new Microsoft.UI.Xaml.Media.Imaging.BitmapImage(url) { DecodePixelHeight = 360 } };
            var tile = new Button
            {
                Padding = new Thickness(0),
                CornerRadius = new CornerRadius(10),
                Width = 320,
                Height = 180,
                Content = new Border { CornerRadius = new CornerRadius(10), Child = image },
            };
            image.ImageFailed += (_, _) => Shots.Children.Remove(tile); // a broken or blocked picture just leaves
            AutomationProperties.SetName(tile, Strings.Get("ShotsOpen"));
            var target = url;
            tile.Click += async (_, _) => await Windows.System.Launcher.LaunchUriAsync(target);
            Lotus.Design.Motion.Attach(tile, lift: 3f, grow: 1.02f);
            Shots.Children.Add(tile);
        }
        ShotsCard.Visibility = shots.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
    }

    private async Task ShowGuideAsync(Port port)
    {
        PortGuideResult? result;
        try { result = await App.Services.GetRequiredService<PortGuideService>().GetAsync(port, Strings.Language); }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { result = null; }
        if (result is null || game?.Recommended.Port.Id != port.Id) return;

        GuideHint.Text = Strings.Get(result.Translated ? "GuideHintTranslated" : result.Guide.Portuguese || !Strings.Language.StartsWith("pt", StringComparison.Ordinal) ? "GuideHint" : "GuideHintEnglish")
            + (result.Outdated ? " " + Strings.Get("GuideHintOutdated") : "");
        GuideSource.NavigateUri = result.Source;
        GuideText.Blocks.Clear();
        foreach (var block in result.Guide.Blocks) GuideText.Blocks.Add(Render(block));
        GuideCard.Visibility = Visibility.Visible;
        GuideText.SizeChanged += (_, _) =>
            GuideMore.Visibility = GuideText.MaxHeight < double.PositiveInfinity && GuideText.ActualHeight >= GuideText.MaxHeight - 1 ? Visibility.Visible : Visibility.Collapsed;
    }

    private void OnGuideMore(object sender, RoutedEventArgs e)
    {
        GuideText.MaxHeight = double.PositiveInfinity;
        GuideMore.Visibility = Visibility.Collapsed;
    }

    private static Paragraph Render(GuideBlock block)
    {
        var paragraph = new Paragraph { Margin = new Thickness(0, 0, 0, 8), LineHeight = 22 };
        switch (block.Kind)
        {
            case GuideBlockKind.Heading:
                paragraph.Margin = new Thickness(0, 6, 0, 6);
                paragraph.FontWeight = Microsoft.UI.Text.FontWeights.SemiBold;
                paragraph.FontSize = 15;
                break;
            case GuideBlockKind.Bullet:
                paragraph.Margin = new Thickness(18, 0, 0, 6);
                paragraph.TextIndent = -14;
                paragraph.Inlines.Add(new Run { Text = "•   ", Foreground = Brush("Accent.Pink") });
                break;
            case GuideBlockKind.Numbered:
                paragraph.Margin = new Thickness(22, 0, 0, 6);
                paragraph.TextIndent = -20;
                paragraph.Inlines.Add(new Run { Text = $"{block.Number}.  ", FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, Foreground = Brush("Accent.Pink") });
                break;
            case GuideBlockKind.Note:
                paragraph.Margin = new Thickness(18, 0, 0, 8);
                paragraph.Foreground = Brush("Text.Secondary");
                paragraph.FontStyle = Windows.UI.Text.FontStyle.Italic;
                break;
            case GuideBlockKind.Code:
                paragraph.FontFamily = new FontFamily("Cascadia Mono, Consolas");
                paragraph.FontSize = 13;
                paragraph.Margin = new Thickness(18, 0, 0, 8);
                paragraph.Foreground = Brush("Text.Secondary");
                break;
        }
        foreach (var span in block.Spans)
        {
            if (span.Link is { } link)
            {
                var hyperlink = new Hyperlink { NavigateUri = link };
                hyperlink.Inlines.Add(new Run { Text = span.Text });
                paragraph.Inlines.Add(hyperlink);
                continue;
            }
            var run = new Run { Text = span.Text };
            if (span.Bold) run.FontWeight = Microsoft.UI.Text.FontWeights.SemiBold;
            if (span.Code && block.Kind != GuideBlockKind.Code) run.FontFamily = new FontFamily("Cascadia Mono, Consolas");
            paragraph.Inlines.Add(run);
        }
        return paragraph;
    }

    // ---- Main button: install, play, update, roll back, uninstall (phase 2) ---------------------

    private void OnPlayChanged(string portId)
    {
        if (game is not null && game.Recommended.Port.Id == portId) { RefreshPlay(); RefreshStyle(); RefreshMods(); _ = RefreshExperienceAsync(); }
    }

    private async Task LookupAsync(bool force = false)
    {
        if (game is null) return;
        var port = game.Recommended.Port;
        if (!ReleasePicker.CanInstall(port.WindowsAsset) || (ShelfBuilder.IsStillBeingBuilt(game) && !play.IsInstalled(port.Id))) return;
        await play.LookupAsync(port, force);
    }

    private PlayButton? button;

    private void RefreshPlay()
    {
        if (game is null) return;
        var port = game.Recommended.Port;
        var s = play.StateOf(port);
        var packageOk = ReleasePicker.CanInstall(port.WindowsAsset);
        var building = ShelfBuilder.IsStillBeingBuilt(game);
        var looking = packageOk && !building && s.Installed is null && s.Latest is null;
        button = PlayButtonRules.Decide(new PlayContext(
            building, packageOk && s.Latest?.Choice is not null, s.Installed is not null, s.Progress is not null, s.RunningSince is not null,
            s.UpdateAvailable, autoUpdate, s.Installed?.LastLaunchFailed == true, s.Installed?.CanRollback == true));

        var (glyph, text) = button.State switch
        {
            PlayButtonState.Working => ("", Strings.Get("PlayCancel")),
            PlayButtonState.Running => ("", Strings.Get("PlayRunning")),
            PlayButtonState.Follow => ("", Strings.Get("PlayFollow")),
            PlayButtonState.OfficialPage => ("", Strings.Get("PlayOfficialPage")),
            PlayButtonState.Install => ("", Strings.Get("GameInstall")),
            PlayButtonState.UpdateAndPlay => ("", Strings.Get("PlayUpdateAndPlay")),
            PlayButtonState.Resolve => ("", Strings.Get("PlayResolve")),
            PlayButtonState.AddGameFile => ("", Strings.Get("PlayAddGameFile")),
            _ => ("", Strings.Get("PlayPlay")),
        };
        if (looking) (glyph, text) = ("", Strings.Get("PlayChecking"));
        MainIcon.Glyph = glyph;
        MainText.Text = text;
        AutomationProperties.SetName(MainButton, text);
        MainButton.IsEnabled = !looking && button.State is not (PlayButtonState.Running or PlayButtonState.AddGameFile);

        MainProgress.Visibility = s.Progress is null ? Visibility.Collapsed : Visibility.Visible;
        if (s.Progress is { } p)
        {
            MainProgress.IsIndeterminate = ProgressText.Indeterminate(p);
            MainProgress.Value = p.Fraction;
        }

        PlayStatus.Text = StatusText(port, s, building, looking);
        var detail = s.Problem ?? DetailText(s);
        PlayDetail.Text = detail ?? "";
        PlayDetail.Visibility = detail is null ? Visibility.Collapsed : Visibility.Visible;
        PlayDetail.Foreground = s.Problem is null ? Brush("Text.Secondary") : Brush("State.Warn");

        PlayLinks.Children.Clear();
        foreach (var action in button.Secondary)
            if (SecondaryLink(action, s) is { } link) PlayLinks.Children.Add(link);
        if (!looking && s.Installed is null && packageOk && !building && s.Latest is { Choice: null, RateLimitedUntil: null })
            PlayLinks.Children.Add(Action(Strings.Get("PlayTryAgain"), () => LookupAsync(force: true)));
    }

    private static string StatusText(Port port, PortRuntime s, bool building, bool looking)
    {
        if (s.Progress is { } p) return ProgressText.For(p);
        if (s.RunningSince is { } since) return Strings.Format("PlayRunningSince", since.ToLocalTime().ToString("t", CultureInfo.CurrentCulture));
        if (s.Installed is { } i)
            return s.UpdateAvailable ? Strings.Format("PlayUpdateAvailable", s.Latest!.Choice!.Release.Tag, i.ActiveTag) : Strings.Format("PlayInstalled", i.ActiveTag);
        if (building) return Strings.Get("PlayStillBuilding");
        if (!ReleasePicker.CanInstall(port.WindowsAsset)) return Strings.Get("PlayUnsupported");
        if (looking) return Strings.Get("PlayLooking");
        var latest = s.Latest!;
        if (latest is { FromCatalog: true, Choice: { } checkedVersion })
            return Strings.Format("PlayAvailableFromCatalog", checkedVersion.Release.Tag, Size(checkedVersion.Asset.Size));
        if (latest.RateLimitedUntil is { } until) return Strings.Format("PlayRateLimited", until.ToLocalTime().ToString("t", CultureInfo.CurrentCulture));
        if (latest.Offline) return Strings.Get("PlayOffline");
        if (latest.Choice is { } c) return Strings.Format("PlayAvailable", c.Release.Tag, Size(c.Asset.Size));
        return Strings.Get(latest.Pick?.Problem == ReleaseProblem.Ambiguous ? "PlayAmbiguous" : "PlayNoRelease");
    }

    private static string? DetailText(PortRuntime s)
    {
        if (s.Progress is not null || s.RunningSince is not null) return null;
        if (s.Installed is { } i)
            return Strings.Get(i.Active.Integrity == CheckState.Ok ? "PlayCheckedOk" : "PlayCheckedNone") + (PlayTime.Line(i) is { } played ? "\n" + played : "");
        if (s.Latest is { FromCatalog: true, RateLimitedUntil: { } until })
            return Strings.Format("PlayCatalogVersionNote", until.ToLocalTime().ToString("t", CultureInfo.CurrentCulture));
        if (s.Latest?.Choice is { } c) return Strings.Get(c.Asset.Sha256 is null ? "PlayWillNotCheck" : "PlayWillCheck");
        return null;
    }

    private HyperlinkButton? SecondaryLink(SecondaryAction action, PortRuntime s) => action switch
    {
        SecondaryAction.Update when s.Latest?.Choice is { } c => Action(Strings.Format("PlayMenuUpdate", c.Release.Tag), () => UpdateAsync(andPlay: false)),
        SecondaryAction.PlayWithoutUpdate => Action(Strings.Get("PlayMenuPlayWithoutUpdate"), () => { Launch(); return Task.CompletedTask; }),
        SecondaryAction.Rollback when s.Installed?.Previous is { } prev => Action(Strings.Format("PlayMenuRollback", prev.Tag), RollbackAsync),
        SecondaryAction.Details when s.Installed is not null => Action(Strings.Get("PlayMenuKeepVersion"), () => { play.KeepVersion(game!.Recommended.Port); return Task.CompletedTask; }),
        SecondaryAction.CloseGame => Action(Strings.Get("PlayMenuClose"), () => { play.CloseGame(game!.Recommended.Port.Id); return Task.CompletedTask; }),
        SecondaryAction.OpenFolder => Action(Strings.Get("PlayMenuOpenFolder"), OpenFolderAsync),
        SecondaryAction.Uninstall => Action(Strings.Get("PlayMenuUninstall"), UninstallAsync),
        _ => null,
    };

    private static HyperlinkButton Action(string text, Func<Task> run)
    {
        var link = new HyperlinkButton { Content = text, Padding = new Thickness(8, 4, 8, 4) };
        link.Click += async (_, _) => await run();
        return link;
    }

    private async void OnMainClick(object sender, RoutedEventArgs e)
    {
        if (game is null || button is null) return;
        var port = game.Recommended.Port;
        switch (button.State)
        {
            case PlayButtonState.Working: play.Cancel(port.Id); break;
            case PlayButtonState.Follow: await Windows.System.Launcher.LaunchUriAsync(new Uri(port.Links.Repo)); break;
            case PlayButtonState.OfficialPage: await Windows.System.Launcher.LaunchUriAsync(new Uri(port.Links.Repo + "/releases")); break;
            case PlayButtonState.Install: await InstallAsync(); break;
            case PlayButtonState.UpdateAndPlay: await UpdateAsync(andPlay: true); break;
            case PlayButtonState.Resolve: await ResolveAsync(); break;
            case PlayButtonState.Play: Launch(); break;
        }
    }

    private async Task InstallAsync()
    {
        var port = game!.Recommended.Port;
        if (play.StateOf(port).Latest?.Choice is not { } choice) return;
        var body = Strings.Format("ConfirmInstallBody", choice.Release.Tag, Size(choice.Asset.Size), play.Folder(port));
        if (!await Confirm(Strings.Format("ConfirmInstallTitle", game.Game.Title), body, Strings.Get("GameInstall"))) return;
        await play.InstallAsync(port, choice);
    }

    private async Task UpdateAsync(bool andPlay)
    {
        var port = game!.Recommended.Port;
        var s = play.StateOf(port);
        if (s.Latest?.Choice is not { } choice || s.Installed is not { } installed) return;
        var body = Strings.Format("ConfirmUpdateBody", choice.Release.Tag, Size(choice.Asset.Size), installed.ActiveTag);
        if (!await Confirm(Strings.Format("ConfirmUpdateTitle", game.Game.Title), body, Strings.Get(andPlay ? "PlayUpdateAndPlay" : "PlayUpdate"))) return;
        if (await play.InstallAsync(port, choice) is null && andPlay && play.StateOf(port).Installed?.ActiveTag == choice.Release.Tag) Launch();
    }

    private async Task RollbackAsync()
    {
        var port = game!.Recommended.Port;
        if (play.StateOf(port).Installed is not { Previous: { } previous } installed) return;
        var body = Strings.Format("ConfirmRollbackBody", previous.Tag, installed.ActiveTag);
        if (!await Confirm(Strings.Format("ConfirmRollbackTitle", previous.Tag), body, Strings.Get("PlayRollback"))) return;
        await play.RollbackAsync(port);
    }

    /// <summary>"Resolver" after a crash right after updating: going back is the first answer offered.</summary>
    private async Task ResolveAsync()
    {
        var g = game!;
        var port = g.Recommended.Port;
        var installed = play.StateOf(port).Installed;
        var fileState = gameFiles.MatchFor(g.Id) is not null ? GameFileState.Recognized
            : port.RequiredGameFiles.Count == 0 ? GameFileState.NotRequired
            : gameFiles.Folders.Count > 0 && !gameFiles.Scanning ? GameFileState.NotFound
            : GameFileState.NotSearched;
        MainButton.IsEnabled = false;
        MainText.Text = Strings.Get("ResolveChecking");
        ResolveReport report;
        try
        {
            var diagnostics = App.Services.GetRequiredService<PortDiagnostics>();
            report = await Task.Run(() => diagnostics.Run(g, port, installed, fileState));
            // With Lotus: its system checks replace the local "graphics libraries present" line, and a recent change on
            // the PC (driver update…) appears as a clue. Nothing is duplicated.
            var program = installed is null ? null : App.Services.GetRequiredService<GameLauncher>().ProgramPath(port, installed);
            var fromLotus = await App.Services.GetRequiredService<LotusBridge>().SystemChecksAsync(port.GraphicsApis ?? [], program, installed?.LastLaunchFailed == true);
            if (fromLotus.Count > 0)
            {
                var replaceGraphics = fromLotus.Any(c => c.Kind == ResolveCheckKind.Graphics);
                report = report with { Checks = [.. report.Checks.Where(c => !(replaceGraphics && c.Kind == ResolveCheckKind.Graphics)), .. fromLotus] };
            }
        }
        finally
        {
            RefreshPlay();
        }

        var problems = report.Checks.Where(c => c.IsProblem).OrderBy(c => c.State == CheckState.Failure ? 0 : 1).ToList();
        var content = new StackPanel { Spacing = 12, MinWidth = 460 };
        if (problems.Count == 0) content.Children.Add(new TextBlock { Text = Strings.Get("ResolveNoProblem"), TextWrapping = TextWrapping.Wrap });
        foreach (var check in problems) content.Children.Add(CheckRow(check));
        var all = new StackPanel { Spacing = 10 };
        foreach (var check in report.Checks) all.Children.Add(CheckRow(check));
        content.Children.Add(new Expander
        {
            Header = Strings.Get("ResolveShowDetails"),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
            Content = new ScrollViewer { MaxHeight = 320, Content = all },
        });
        if (installed?.LastLaunchFailed == true)
            content.Children.Add(Action(Strings.Get("ResolveKeep"), () => { play.KeepVersion(port); return Task.CompletedTask; }));

        var action = report.MainProblem?.Action ?? ResolveAction.None;
        var dialog = new ContentDialog
        {
            XamlRoot = XamlRoot,
            Title = Strings.Get("ResolveTitle"),
            Content = new ScrollViewer { Content = content, MaxHeight = 520 },
            SecondaryButtonText = Strings.Get("ResolveCopyReport"),
            CloseButtonText = Strings.Get("Close"),
            DefaultButton = action == ResolveAction.None ? ContentDialogButton.Close : ContentDialogButton.Primary,
        };
        if (action != ResolveAction.None) dialog.PrimaryButtonText = ResolveText.Action(action);
        dialog.SecondaryButtonClick += (_, args) =>
        {
            args.Cancel = true; // stays open: copying is not a decision
            var package = new Windows.ApplicationModel.DataTransfer.DataPackage();
            package.SetText(ResolveText.Report(report));
            Windows.ApplicationModel.DataTransfer.Clipboard.SetContent(package);
            dialog.SecondaryButtonText = Strings.Get("ResolveCopied");
        };
        if (await dialog.ShowAsync() != ContentDialogResult.Primary) return;
        switch (action)
        {
            case ResolveAction.Rollback: await RollbackAsync(); break;
            case ResolveAction.Reinstall: await ReinstallAsync(); break;
            case ResolveAction.AddGameFile: OnAddGameFile(this, new RoutedEventArgs()); break;
            case ResolveAction.OpenFolder: await OpenFolderAsync(); break;
        }
    }

    private static Grid CheckRow(ResolveCheck check)
    {
        var row = new Grid { ColumnSpacing = 12 };
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        row.Children.Add(new StackPanel
        {
            Spacing = 2,
            Children =
            {
                new TextBlock { Text = ResolveText.Title(check.Kind), FontWeight = Microsoft.UI.Text.FontWeights.SemiBold },
                new TextBlock { Text = ResolveText.Explain(check), TextWrapping = TextWrapping.Wrap, Style = StyleOf("Type.Caption"), IsTextSelectionEnabled = true },
            },
        });
        var badge = new StateBadge
        {
            Text = ResolveText.State(check.State),
            Tone = check.State switch { CheckState.Ok => BadgeTone.Ok, CheckState.Warning => BadgeTone.Warning, CheckState.Failure => BadgeTone.Failure, _ => BadgeTone.Unknown },
            VerticalAlignment = VerticalAlignment.Top,
        };
        Grid.SetColumn(badge, 1);
        row.Children.Add(badge);
        return row;
    }

    /// <summary>"Reinstalar": uninstall keeping saves, then install the newest version again (both confirmed).</summary>
    private async Task ReinstallAsync()
    {
        var port = game!.Recommended.Port;
        var choice = (play.StateOf(port).Latest ?? await play.LookupAsync(port)).Choice;
        if (choice is null) return;
        var body = Strings.Format("ConfirmInstallBody", choice.Release.Tag, Size(choice.Asset.Size), play.Folder(port));
        if (!await Confirm(Strings.Format("ConfirmInstallTitle", game.Game.Title), body, Strings.Get("ResolveActionReinstall"))) return;
        await play.RemoveAsync(port, keepSaves: true);
        await play.InstallAsync(port, choice);
    }

    private void Launch()
    {
        if (play.Launch(game!.Recommended.Port) is null && App.MainWindow?.AppWindow.Presenter is Microsoft.UI.Windowing.OverlappedPresenter presenter)
            presenter.Minimize();
    }

    private async Task OpenFolderAsync()
    {
        var port = game!.Recommended.Port;
        if (play.StateOf(port).Installed is not { } installed) return;
        var folder = Path.GetDirectoryName(App.Services.GetRequiredService<GameLauncher>().ProgramPath(port, installed));
        if (folder is not null && Directory.Exists(folder)) await Windows.System.Launcher.LaunchFolderPathAsync(folder);
    }

    private async Task UninstallAsync()
    {
        var port = game!.Recommended.Port;
        var dialog = new ContentDialog
        {
            XamlRoot = XamlRoot,
            Title = Strings.Format("ConfirmRemoveTitle", game.Game.Title),
            Content = new TextBlock { Text = Strings.Get("ConfirmRemoveBody"), TextWrapping = TextWrapping.Wrap },
            PrimaryButtonText = Strings.Get("ConfirmRemoveKeep"),
            SecondaryButtonText = Strings.Get("ConfirmRemoveAll"),
            CloseButtonText = Strings.Get("Cancel"),
            DefaultButton = ContentDialogButton.Close,
        };
        var answer = await dialog.ShowAsync();
        if (answer == ContentDialogResult.None) return;
        if (answer == ContentDialogResult.Secondary
            && !await Confirm(Strings.Get("ConfirmRemoveAllTitle"), Strings.Get("ConfirmRemoveAllBody"), Strings.Get("ConfirmRemoveAll"))) return;
        await play.RemoveAsync(port, keepSaves: answer == ContentDialogResult.Primary);
    }

    private async Task<bool> Confirm(string title, string message, string primary)
    {
        var dialog = new ContentDialog
        {
            XamlRoot = XamlRoot,
            Title = title,
            Content = new TextBlock { Text = message, TextWrapping = TextWrapping.Wrap },
            PrimaryButtonText = primary,
            CloseButtonText = Strings.Get("Cancel"),
            DefaultButton = ContentDialogButton.Primary,
        };
        return await dialog.ShowAsync() == ContentDialogResult.Primary;
    }

    private static string Size(long bytes) => bytes >= 1L << 30
        ? Strings.Format("SizeGb", (bytes / (double)(1L << 30)).ToString("0.0", CultureInfo.CurrentCulture))
        : Strings.Format("SizeMb", Math.Max(0.1, bytes / (double)(1L << 20)).ToString("0.0", CultureInfo.CurrentCulture));
}
