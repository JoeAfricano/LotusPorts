using Lotus.Design.Controls;
using LotusPorts.Catalog;
using LotusPorts.Core.Preferences;
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
    private LibraryGame? game;
    private bool advanced;

    public GamePage() => InitializeComponent();

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        advanced = (await App.Services.GetRequiredService<PreferencesService>().LoadAsync()).AdvancedMode;
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
        Tiles.Children.Add(Tile(0, "\uE8B7", Strings.Get("TileGameFile"), Strings.Get("TileNotVerified"), BadgeTone.Unknown));
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
            ? new LotusPetal { IsLit = false, Width = 26, Height = 22 }
            : new FontIcon { Glyph = glyph, FontSize = 22, Foreground = Brush("Text.Secondary") };
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
}
