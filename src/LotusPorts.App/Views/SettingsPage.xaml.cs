using System.Diagnostics;
using LotusPorts.App.ViewModels;
using LotusPorts.Core.Preferences;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
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
            await ShowAppearanceAsync();
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

    // ---- Aparência ------------------------------------------------------------------------------

    private readonly AppearanceService appearance = App.Services.GetRequiredService<AppearanceService>();
    private AppPreferences? look;

    private async Task ShowAppearanceAsync()
    {
        look = await App.Services.GetRequiredService<PreferencesService>().LoadAsync();
        ThemeChoices.Items.Clear();
        foreach (var theme in AppPreferences.Themes.Where(t => t != "Custom" || look.CustomTheme is not null))
        {
            var card = new StackPanel
            {
                Width = 150,
                Spacing = 4,
                Padding = new Thickness(12, 10, 12, 10),
                Tag = theme,
                Children =
                {
                    new TextBlock { Text = Strings.Get("Theme" + theme), FontWeight = Microsoft.UI.Text.FontWeights.SemiBold },
                    new TextBlock { Text = Strings.Get("Theme" + theme + "Hint"), Style = (Style)Application.Current.Resources["Type.Caption"], TextWrapping = TextWrapping.Wrap },
                },
            };
            ThemeChoices.Items.Add(card);
            if (theme == look.Theme) ThemeChoices.SelectedItem = card;
        }

        AccentChoices.Children.Clear();
        foreach (var accent in Lotus.Design.Themes.ThemePalette.Accents)
        {
            var (fill, _, _) = Lotus.Design.Themes.ThemePalette.AccentColors(accent);
            var swatch = new Button
            {
                Width = 36,
                Height = 36,
                CornerRadius = new CornerRadius(18),
                Padding = new Thickness(0),
                Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(fill),
                BorderThickness = new Thickness(accent == look.Accent ? 3 : 0),
                BorderBrush = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["Text.Primary"],
            };
            AutomationProperties.SetName(swatch, Strings.Get("Accent" + accent));
            ToolTipService.SetToolTip(swatch, Strings.Get("Accent" + accent));
            swatch.Click += async (_, _) => await SetLookAsync(look.Theme is AppPreferences.FollowLotus or "Custom" ? "Midnight" : look.Theme, accent, null);
            AccentChoices.Children.Add(swatch);
        }
        AccentRow.Opacity = look.Theme is AppPreferences.FollowLotus or "Custom" ? 0.55 : 1;

        GalleryChoices.Children.Clear();
        foreach (var preset in Lotus.Design.Themes.ThemeGallery.All.Take(8))
        {
            var button = new Button { Content = preset.Spec.Name, Padding = new Thickness(12, 6, 12, 6) };
            button.Click += async (_, _) => await SetLookAsync("Custom", look.Accent, Lotus.Design.Themes.ThemeEngine.ToCode(preset.Spec));
            GalleryChoices.Children.Add(button);
        }

        BannerChoices.Items.Clear();
        var chosenBanner = await appearance.HomeBannerIdAsync();
        foreach (var (id, path) in AppearanceService.HomeBanners)
        {
            var thumb = new Border
            {
                Width = 168,
                Height = 57,
                CornerRadius = new CornerRadius(8),
                Tag = id,
                Child = new Image
                {
                    Stretch = Microsoft.UI.Xaml.Media.Stretch.UniformToFill,
                    Source = new Microsoft.UI.Xaml.Media.Imaging.BitmapImage(new Uri("ms-appx:///" + path)) { DecodePixelWidth = 336 },
                },
            };
            AutomationProperties.SetName(thumb, Strings.Get("Banner" + id));
            ToolTipService.SetToolTip(thumb, Strings.Get("Banner" + id));
            BannerChoices.Items.Add(thumb);
            if (id == chosenBanner) BannerChoices.SelectedItem = thumb;
        }

        var (mode, _, custom) = appearance.Effective;
        AppearanceStatus.Text = look.Theme == AppPreferences.FollowLotus
            ? Strings.Format("AppearanceFollowing", custom?.Name ?? Strings.Get("Theme" + mode))
            : custom is not null ? Strings.Format("AppearanceCustom", custom.Name) : "";
    }

    private async Task SetLookAsync(string theme, string accent, string? code)
    {
        await appearance.SetAsync(theme, accent, code);
        await ShowAppearanceAsync();
    }

    private async void OnThemeClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is StackPanel { Tag: string theme } && look is not null)
            await SetLookAsync(theme, look.Accent, theme == "Custom" ? look.CustomTheme : null);
    }

    private async void OnBannerClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is Border { Tag: string id }) await appearance.SetHomeBannerAsync(id);
    }

    private async void OnUseThemeCode(object sender, RoutedEventArgs e)
    {
        var code = ThemeCode.Text.Trim();
        if (!Lotus.Design.Themes.ThemeEngine.TryFromCode(code, out _))
        {
            AppearanceStatus.Text = Strings.Get("AppearanceBadCode");
            return;
        }
        ThemeCode.Text = "";
        await SetLookAsync("Custom", look?.Accent ?? "rose", code);
    }

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
