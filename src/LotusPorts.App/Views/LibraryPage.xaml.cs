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
            await ViewModel.LoadAsync();
            BuildChips();
        };
    }

    public LibraryViewModel ViewModel { get; }

    public void FocusSearch() => SearchBox.Focus(FocusState.Keyboard);

    private void BuildChips()
    {
        PlatformChips.Children.Clear();
        foreach (var chip in ViewModel.Platforms)
        {
            var button = new RadioButton
            {
                Content = chip.Label,
                GroupName = "Platform",
                Style = (Style)Application.Current.Resources["Lotus.Pill"],
                Padding = new Thickness(12, 5, 12, 6),
                IsChecked = chip.Platform == ViewModel.PlatformFilter,
                Tag = chip,
            };
            button.Checked += (s, _) => ViewModel.SetPlatform(((PlatformChip)((RadioButton)s).Tag).Platform);
            PlatformChips.Children.Add(button);
        }
        GenreBox.ItemsSource = ViewModel.Genres;
        GenreBox.SelectedIndex = 0;
    }

    private void OnTabChecked(object sender, RoutedEventArgs e)
    {
        if (sender is RadioButton { Tag: string tag } && Enum.TryParse<LibraryTab>(tag, out var tab) && IsLoaded) ViewModel.SetTab(tab);
    }

    private void OnGenreChanged(object sender, SelectionChangedEventArgs e) =>
        ViewModel.SetGenre(GenreBox.SelectedIndex <= 0 ? null : GenreBox.SelectedItem as string);

    private async void OnDismissNew(object sender, RoutedEventArgs e) => await ViewModel.DismissNewAsync();

    private void OnSearchTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput) ViewModel.UpdateSuggestions(sender.Text);
    }

    private void OnQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        var entry = args.ChosenSuggestion as SearchEntry ?? ViewModel.Suggestions.FirstOrDefault(s => s.Id.Length > 0);
        if (entry is not { Id.Length: > 0 }) return;
        sender.Text = "";
        if (entry.Kind == SearchEntryKind.Game) App.MainWindow?.Navigate(Pages.Game, entry.Id);
        else App.MainWindow?.Navigate(entry.Id);
    }
}
