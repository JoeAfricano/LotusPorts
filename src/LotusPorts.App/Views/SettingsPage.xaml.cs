using System.Diagnostics;
using LotusPorts.App.ViewModels;
using LotusPorts.Core.Preferences;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage.Pickers;

namespace LotusPorts.App.Views;

public sealed partial class SettingsPage : Page
{
    public SettingsPage()
    {
        ViewModel = App.Services.GetRequiredService<SettingsViewModel>();
        InitializeComponent();
        Loaded += async (_, _) =>
        {
            await ViewModel.LoadAsync();
            (ViewModel.DefaultStyle == ExperienceStyle.Remastered ? StyleRemastered : StyleOriginal).IsChecked = true;
            gameFiles.Changed += ShowGameFiles;
            ShowGameFiles();
        };
        Unloaded += (_, _) => gameFiles.Changed -= ShowGameFiles;
    }

    public SettingsViewModel ViewModel { get; }

    private void OnStyleChecked(object sender, RoutedEventArgs e)
    {
        if (sender is RadioButton { Tag: string tag } && Enum.TryParse<ExperienceStyle>(tag, out var style)) ViewModel.ChooseStyle(style);
    }

    private async void OnChooseLibraryFolder(object sender, RoutedEventArgs e)
    {
        var picker = new FolderPicker { SuggestedStartLocation = PickerLocationId.ComputerFolder };
        picker.FileTypeFilter.Add("*");
        WinRT.Interop.InitializeWithWindow.Initialize(picker, WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow));
        var folder = await picker.PickSingleFolderAsync();
        if (folder is not null) ViewModel.ChooseLibraryFolder(folder.Path);
    }

    private void OnOpenCatalogSources(object sender, RoutedEventArgs e) => App.MainWindow?.Navigate(Pages.Catalog);

    // ---- Arquivos de jogo ----------------------------------------------------------------------

    private readonly GameFilesService gameFiles = App.Services.GetRequiredService<GameFilesService>();

    private void ShowGameFiles()
    {
        var known = gameFiles.Files;
        var recognized = known.Where(f => f.Matches.Count > 0).ToList();
        ScanButton.Content = Strings.Get(gameFiles.Scanning ? "GameFilesStop" : "GameFilesScan");
        ScanButton.IsEnabled = gameFiles.Folders.Count > 0;
        GameFilesStatus.Text = gameFiles.Folders.Count == 0 ? Strings.Get("GameFilesNoFolders")
            : gameFiles.Progress is { Current: { } current } ? Strings.Format("GameFilesReading", current)
            : gameFiles.Scanning ? Strings.Get("GameFilesSearching")
            : Strings.Format("GameFilesSummary", known.Count, recognized.Count);

        GameFolderList.Children.Clear();
        foreach (var folder in gameFiles.Folders)
        {
            var row = new Grid { ColumnSpacing = 12 };
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            row.Children.Add(new FontIcon { Glyph = "", FontSize = 16, VerticalAlignment = VerticalAlignment.Center });
            var path = new TextBlock { Text = folder, Style = (Style)Application.Current.Resources["Type.Body"], TextTrimming = TextTrimming.CharacterEllipsis, VerticalAlignment = VerticalAlignment.Center };
            Grid.SetColumn(path, 1);
            row.Children.Add(path);
            var remove = new HyperlinkButton { Content = Strings.Get("GameFilesRemoveFolder") };
            remove.Click += async (_, _) => await gameFiles.RemoveFolderAsync(folder);
            Grid.SetColumn(remove, 2);
            row.Children.Add(remove);
            GameFolderList.Children.Add(row);
        }

        RecognizedList.Children.Clear();
        foreach (var file in recognized.OrderBy(f => f.Matches[0].Game.Game.Title, StringComparer.CurrentCulture))
        {
            var match = file.Matches[0];
            var text = new TextBlock { Style = (Style)Application.Current.Resources["Type.Body"], TextWrapping = TextWrapping.Wrap };
            text.Inlines.Add(new Microsoft.UI.Xaml.Documents.Run { Text = "✓  ", Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["State.Ok"] });
            text.Inlines.Add(new Microsoft.UI.Xaml.Documents.Run { Text = match.Game.Game.Title, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold });
            text.Inlines.Add(new Microsoft.UI.Xaml.Documents.Run { Text = $"  ·  {match.VersionLabel}  ·  {Path.GetFileName(file.Info.Path)}" });
            ToolTipService.SetToolTip(text, file.Info.Path);
            RecognizedList.Children.Add(text);
        }
    }

    private async void OnAddGameFolder(object sender, RoutedEventArgs e)
    {
        var picker = new FolderPicker { SuggestedStartLocation = PickerLocationId.ComputerFolder };
        picker.FileTypeFilter.Add("*");
        WinRT.Interop.InitializeWithWindow.Initialize(picker, WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow));
        if (await picker.PickSingleFolderAsync() is { } folder) await gameFiles.AddFolderAsync(folder.Path);
    }

    private async void OnScanGameFiles(object sender, RoutedEventArgs e)
    {
        if (gameFiles.Scanning) gameFiles.CancelScan();
        else await gameFiles.ScanAsync();
    }

    private void OnOpenDataFolder(object sender, RoutedEventArgs e) =>
        Process.Start(new ProcessStartInfo("explorer.exe", $"\"{ViewModel.DataFolder}\"") { UseShellExecute = true });
}
