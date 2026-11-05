using LotusPorts.App.ViewModels;
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
        Loaded += (_, _) => ViewModel.Load();
    }

    public LibraryViewModel ViewModel { get; }

    public void FocusSearch() => SearchBox.Focus(FocusState.Keyboard);

    private void OnSearchTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput) ViewModel.UpdateSuggestions(sender.Text);
    }

    private void OnQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        var entry = args.ChosenSuggestion as SearchEntry ?? ViewModel.Suggestions.FirstOrDefault(s => s.Id.Length > 0);
        if (entry is not { Kind: SearchEntryKind.Action, Id.Length: > 0 }) return;
        sender.Text = "";
        App.MainWindow?.Navigate(entry.Id);
    }
}
