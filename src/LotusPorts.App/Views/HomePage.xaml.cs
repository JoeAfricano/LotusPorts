using LotusPorts.App.ViewModels;
using LotusPorts.Catalog;
using LotusPorts.Core.Preferences;
using LotusPorts.Core.Search;
using LotusPorts.Infrastructure.Catalog;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace LotusPorts.App.Views;

/// <summary>Início: the storefront (RecompLauncher's Discover, the Lotus way).</summary>
public sealed partial class HomePage : Page
{
    public HomePage()
    {
        ViewModel = App.Services.GetRequiredService<HomeViewModel>();
        InitializeComponent();
        Loaded += async (_, _) =>
        {
            HomeHero.Source = await App.Services.GetRequiredService<AppearanceService>().HomeBannerAsync();
            await ViewModel.LoadAsync();
            var showInReview = (await App.Services.GetRequiredService<PreferencesService>().LoadAsync()).ShowInReview;
            Search.Load(App.Services.GetRequiredService<CatalogService>().Library.Visible(showInReview));
            news.Changed += ShowFavoriteNews;
            ShowFavoriteNews();
        };
        Unloaded += (_, _) => news.Changed -= ShowFavoriteNews;
    }

    private readonly FavoriteNews news = App.Services.GetRequiredService<FavoriteNews>();

    /// <summary>Favorite games (not installed) whose port published a new version since the player last looked.</summary>
    private void ShowFavoriteNews()
    {
        FavoriteNewsList.Children.Clear();
        foreach (var item in news.Items)
        {
            var row = new Grid { ColumnSpacing = 12 };
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            row.Children.Add(new StackPanel
            {
                Spacing = 1,
                VerticalAlignment = VerticalAlignment.Center,
                Children =
                {
                    new TextBlock { Text = item.Game.Game.Title, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, TextTrimming = TextTrimming.CharacterEllipsis },
                    new TextBlock { Text = Strings.Format("FavNewsVersion", item.Tag, item.Game.Recommended.Port.Title), Style = (Style)Application.Current.Resources["Type.Caption"] },
                },
            });
            var open = new Button { Content = Strings.Get("FavNewsOpen") };
            var gameId = item.Game.Id;
            open.Click += async (_, _) =>
            {
                await news.MarkSeenAsync(gameId);
                App.MainWindow?.Navigate(Pages.Game, gameId);
            };
            Grid.SetColumn(open, 1);
            row.Children.Add(open);
            FavoriteNewsList.Children.Add(row);
        }
        FavoriteNewsCard.Visibility = news.Items.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
    }

    public HomeViewModel ViewModel { get; }

    public SearchViewModel Search { get; } = new();

    public void FocusSearch() => SearchBox.Focus(FocusState.Keyboard);

    private void OnFeatured(object sender, RoutedEventArgs e) => App.MainWindow?.Navigate(Pages.Game, ViewModel.FeaturedId);

    // Card width 180 + spacing 16, the same numbers as the grid in the XAML.
    private void OnPopularSizeChanged(object sender, SizeChangedEventArgs e) =>
        ViewModel.SetCoverColumns((int)((e.NewSize.Width + 16) / (180 + 16)));

    // Tile minimum width 290 + spacing 14, at most 4 columns, as in the XAML.
    private void OnTilesSizeChanged(object sender, SizeChangedEventArgs e) =>
        ViewModel.SetTileColumns((int)((e.NewSize.Width + 14) / (290 + 14)));

    private void OnSeeAllPopular(object sender, RoutedEventArgs e) => App.MainWindow?.Navigate(Pages.Catalog, new CatalogFilterArgument(null, null, CatalogSort.Popular));

    private void OnSeeAllUpdated(object sender, RoutedEventArgs e) => App.MainWindow?.Navigate(Pages.Catalog, new CatalogFilterArgument(null, null, CatalogSort.Updated));

    private void OnConsole(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: Platform platform }) App.MainWindow?.Navigate(Pages.Catalog, CatalogFilterArgument.ForPlatform(platform));
    }

    private void OnGenre(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: string genre }) App.MainWindow?.Navigate(Pages.Catalog, CatalogFilterArgument.ForGenre(genre));
    }

    private void OnSearchTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput) Search.Update(sender.Text);
    }

    private void OnQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        if (Search.Open(args.ChosenSuggestion as SearchEntry)) sender.Text = "";
    }
}

/// <summary>How Início opens the Catálogo already filtered.</summary>
public sealed record CatalogFilterArgument(Platform? Platform, string? Genre, CatalogSort Sort = CatalogSort.Name)
{
    public static CatalogFilterArgument ForPlatform(Platform platform) => new(platform, null);
    public static CatalogFilterArgument ForGenre(string genre) => new(null, genre);
}
