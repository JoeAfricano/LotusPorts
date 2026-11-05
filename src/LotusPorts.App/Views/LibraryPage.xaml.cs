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
        Loaded += async (_, _) => await ViewModel.LoadAsync();
    }

    public LibraryViewModel ViewModel { get; }

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
