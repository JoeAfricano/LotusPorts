using LotusPorts.App.ViewModels;
using LotusPorts.Catalog;
using LotusPorts.Core.Search;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace LotusPorts.App.Views;

public sealed partial class LibraryPage : Page
{
    public LibraryPage()
    {
        ViewModel = App.Services.GetRequiredService<LibraryViewModel>();
        InitializeComponent();
        Loaded += async (_, _) =>
        {
            play.Changed += OnPlayChanged;
            ShowUpdates();
            await ViewModel.LoadAsync();
            // The icon rail only helps when the player's games span more than one platform.
            Rail.SetPlatforms(ViewModel.Platforms, ViewModel.Platform);
            Rail.Visibility = ViewModel.Platforms.Count > 2 ? Visibility.Visible : Visibility.Collapsed;
        };
        Rail.SelectionChanged += (_, platform) => ViewModel.SetPlatform(platform);
        Unloaded += (_, _) => play.Changed -= OnPlayChanged;
    }

    public LibraryViewModel ViewModel { get; }

    private readonly PlayService play = App.Services.GetRequiredService<PlayService>();
    private readonly LotusPorts.Infrastructure.Catalog.CatalogService catalog = App.Services.GetRequiredService<LotusPorts.Infrastructure.Catalog.CatalogService>();
    private bool updating;

    private void OnPlayChanged(string _) => ShowUpdates();

    private string TitleOf(Port port) => catalog.Library.Find(port.GameId)?.Game.Title ?? port.Title;

    /// <summary>"N atualizações disponíveis" with the games' names; hidden when there are none.</summary>
    private void ShowUpdates()
    {
        var updates = play.UpdatesAvailable;
        UpdatesCard.Visibility = updates.Count > 0 || updating ? Visibility.Visible : Visibility.Collapsed;
        if (updating) return;
        UpdatesTitle.Text = updates.Count == 1 ? Strings.Get("UpdatesOne") : Strings.Format("UpdatesMany", updates.Count);
        UpdatesNames.Text = string.Join(" · ", updates.Select(TitleOf));
        UpdateAllButton.IsEnabled = updates.Count > 0;
    }

    private async void OnUpdateAll(object sender, RoutedEventArgs e)
    {
        var updates = play.UpdatesAvailable;
        if (updates.Count == 0 || updating) return;
        var list = new StackPanel { Spacing = 6 };
        list.Children.Add(new TextBlock { Text = Strings.Get("UpdateAllBody"), TextWrapping = TextWrapping.Wrap });
        foreach (var port in updates)
        {
            var s = play.StateOf(port);
            list.Children.Add(new TextBlock
            {
                Text = $"• {TitleOf(port)}: {s.Installed?.ActiveTag} → {s.Latest?.Choice?.Release.Tag}",
                Style = (Style)Application.Current.Resources["Type.Caption"],
                TextWrapping = TextWrapping.Wrap,
            });
        }
        var dialog = new ContentDialog
        {
            XamlRoot = XamlRoot,
            Title = Strings.Get("UpdateAllTitle"),
            Content = new ScrollViewer { Content = list, MaxHeight = 420 },
            PrimaryButtonText = Strings.Get("UpdateAll"),
            CloseButtonText = Strings.Get("Cancel"),
            DefaultButton = ContentDialogButton.Primary,
        };
        if (await dialog.ShowAsync() != ContentDialogResult.Primary) return;
        updating = true;
        UpdateAllButton.IsEnabled = false;
        UpdatesTitle.Text = Strings.Get("UpdatingAll");
        try
        {
            var failed = await play.UpdateAllAsync(updates);
            updating = false;
            ShowUpdates();
            if (failed > 0)
            {
                UpdatesCard.Visibility = Visibility.Visible;
                UpdatesTitle.Text = Strings.Format("UpdateAllSomeFailed", failed);
            }
        }
        finally
        {
            updating = false;
        }
    }

    public void FocusSearch() => SearchBox.Focus(FocusState.Keyboard);

    private void OnTabChecked(object sender, RoutedEventArgs e)
    {
        if (sender is RadioButton { Tag: string tag } && Enum.TryParse<LibraryTab>(tag, out var tab) && IsLoaded) ViewModel.SetTab(tab);
    }

    private void OnGoHome(object sender, RoutedEventArgs e) => App.MainWindow?.Navigate(Pages.Home);

    private void OnGoCatalog(object sender, RoutedEventArgs e) => App.MainWindow?.Navigate(Pages.Catalog);

    private void OnSearchTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput) ViewModel.Search.Update(sender.Text);
    }

    private void OnQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        if (ViewModel.Search.Open(args.ChosenSuggestion as SearchEntry)) sender.Text = "";
    }
}
