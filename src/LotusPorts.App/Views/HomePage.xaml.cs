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
            await ViewModel.LoadAsync();
            var showInReview = (await App.Services.GetRequiredService<PreferencesService>().LoadAsync()).ShowInReview;
            Search.Load(App.Services.GetRequiredService<CatalogService>().Library.Visible(showInReview));
        };
    }

    public HomeViewModel ViewModel { get; }

    public SearchViewModel Search { get; } = new();

    public void FocusSearch() => SearchBox.Focus(FocusState.Keyboard);

    private void OnFeatured(object sender, RoutedEventArgs e) => App.MainWindow?.Navigate(Pages.Game, ViewModel.FeaturedId);

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
public sealed record CatalogFilterArgument(Platform? Platform, string? Genre)
{
    public static CatalogFilterArgument ForPlatform(Platform platform) => new(platform, null);
    public static CatalogFilterArgument ForGenre(string genre) => new(null, genre);
}
