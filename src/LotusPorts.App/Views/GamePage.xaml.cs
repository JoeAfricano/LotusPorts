using System.Diagnostics;
using System.Globalization;
using Lotus.Design.Controls;
using LotusPorts.Catalog;
using LotusPorts.Core.Guides;
using LotusPorts.Core.Install;
using LotusPorts.Core.Preferences;
using LotusPorts.Core.Verification;
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
        RefreshPlay();
        var guide = ShowGuideAsync(game.Recommended.Port);
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

        var favorite = catalog.StateOf(g.Id).Favorite;
        FavoriteButton.IsChecked = favorite;
        FavoriteIcon.Glyph = favorite ? "\uEB52" : "\uEB51";

        MoreMenu.Items.Add(Link(Strings.Get("GameOpenRepo"), port.Links.Repo));
        if (port.Links.Docs is { } docs && docs != port.Links.Repo) MoreMenu.Items.Add(Link(Strings.Get("GameOpenDocs"), docs));

        RecommendedTitle.Text = port.Title;
        RecommendedReason.Text = g.Game.RecommendationReasonPtBr
            ?? (g.Alternatives.Count == 0 ? Strings.Get("GameOnlyPort") : Strings.Get("GameReasonMissing"));

        // Nothing below has been checked on this PC yet: every tile says so instead of showing OK.
        Tiles.Children.Add(GameFileTile());
        Tiles.Children.Add(Tile(1, "\uE74C", Strings.Get("TileDependencies"), Strings.Get("TileNotVerified"), BadgeTone.Unknown));
        Tiles.Children.Add(Tile(2, null, Strings.Get("TileLotus"), Strings.Get("BadgeInDevelopment"), BadgeTone.Unknown));

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

    // ---- Game file (phase 3) ------------------------------------------------------------------

    private readonly GameFilesService gameFiles = App.Services.GetRequiredService<GameFilesService>();
    private readonly StateBadge fileBadge = new();
    private readonly HyperlinkButton fileAction = new() { Padding = new Thickness(0, 2, 0, 2) };

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
                    new StackPanel { Spacing = 4, Children = { new TextBlock { Text = Strings.Get("TileGameFile"), Style = StyleOf("Type.Body") }, fileBadge, fileAction } },
                },
            },
        };
        RefreshFileTile();
        return tile;
    }

    private void RefreshFileTile()
    {
        if (game is null) return;
        if (gameFiles.MatchFor(game.Id) is { } match)
        {
            fileBadge.Text = Strings.Format("TileFileRecognized", match.VersionLabel);
            fileBadge.Tone = BadgeTone.Ok;
            fileAction.Content = Strings.Get("TileFileShow");
            ToolTipService.SetToolTip(fileBadge, gameFiles.Files.FirstOrDefault(f => f.Matches.Contains(match))?.Info.Path);
            return;
        }
        var searched = gameFiles.Folders.Count > 0 && !gameFiles.Scanning;
        fileBadge.Text = Strings.Get(gameFiles.Scanning ? "TileFileSearching" : searched ? "TileFileMissing" : "TileNotVerified");
        fileBadge.Tone = searched ? BadgeTone.Warning : BadgeTone.Unknown;
        fileAction.Content = Strings.Get("PlayAddGameFile");
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
            // Already recognized: show where it is (Explorer selects it; nothing is moved).
            Process.Start(new ProcessStartInfo("explorer.exe", $"/select,\"{known.Info.Path}\"") { UseShellExecute = true })?.Dispose();
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
        if (game is not null && game.Recommended.Port.Id == portId) RefreshPlay();
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
            MainProgress.IsIndeterminate = p.Stage == InstallStage.Finishing;
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
        if (s.Progress is { } p)
            return p.Stage switch
            {
                InstallStage.Downloading => Strings.Format("PlayDownloading", (int)Math.Round(p.Fraction * 100)),
                InstallStage.Extracting => Strings.Format("PlayExtracting", (int)Math.Round(p.Fraction * 100)),
                _ => Strings.Get("PlayFinishing"),
            };
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
        if (s.Installed is { } i) return Strings.Get(i.Active.Integrity == CheckState.Ok ? "PlayCheckedOk" : "PlayCheckedNone");
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
        if (play.StateOf(game!.Recommended.Port).Installed?.CanRollback == true) await RollbackAsync();
        else play.KeepVersion(game.Recommended.Port);
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
