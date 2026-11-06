using System.Diagnostics;
using LotusPorts.App.ViewModels;
using LotusPorts.Core.Preferences;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Ellipse = Microsoft.UI.Xaml.Shapes.Ellipse;
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
            TvModeToggle.IsOn = App.MainWindow?.TvMode == true;
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

    private void OnTvModeToggled(object sender, RoutedEventArgs e)
    {
        if (App.MainWindow is { } window && window.TvMode != TvModeToggle.IsOn) window.SetTvMode(TvModeToggle.IsOn);
    }

    // ---- Aparência (as in Lotus: cards that preview themselves) ---------------------------------

    private readonly AppearanceService appearance = App.Services.GetRequiredService<AppearanceService>();
    private AppPreferences? look;

    /// <summary>Preview colors per mode: page, card, text. Same as Lotus's picker.</summary>
    private static readonly Dictionary<string, (string Bg, string Card, string Text)> Previews = new()
    {
        ["Midnight"] = ("#0D0A0F", "#211A29", "#F8F5FA"),
        ["Light"] = ("#F7ECF1", "#FFFAFC", "#2B1020"),
        ["Graphite"] = ("#131316", "#222228", "#F5F5F7"),
        ["System"] = ("#2D2336", "#211A29", "#F8F5FA"),
    };

    private async Task ShowAppearanceAsync()
    {
        look = await App.Services.GetRequiredService<PreferencesService>().LoadAsync();
        var (fill, _, _) = Lotus.Design.Themes.ThemePalette.AccentColors(look.Accent);

        ThemeCards.Children.Clear();
        foreach (var theme in new[] { AppPreferences.FollowLotus, "Midnight", "Light", "Graphite", "System" })
            ThemeCards.Children.Add(ModeCard(theme, theme == look.Theme, fill));

        AccentDots.Children.Clear();
        foreach (var accent in Lotus.Design.Themes.ThemePalette.Accents)
        {
            var selected = accent == look.Accent;
            var (dotFill, _, _) = Lotus.Design.Themes.ThemePalette.AccentColors(accent);
            var dot = new Grid { Width = 44, Height = 44 };
            dot.Children.Add(new Ellipse { Fill = new SolidColorBrush(dotFill) });
            if (selected)
            {
                dot.Children.Add(new Ellipse { Stroke = (Brush)Application.Current.Resources["Text.Primary"], StrokeThickness = 2, Margin = new Thickness(-4) });
                dot.Children.Add(new FontIcon { Glyph = "", FontSize = 16, Foreground = new SolidColorBrush(Microsoft.UI.Colors.White) });
            }
            var button = new Button { Content = dot, Width = 52, Height = 52, Padding = new Thickness(0), CornerRadius = new CornerRadius(26), Background = new SolidColorBrush(Microsoft.UI.Colors.Transparent), BorderThickness = new Thickness(0) };
            var label = Strings.Get("Accent" + accent);
            ToolTipService.SetToolTip(button, label);
            AutomationProperties.SetName(button, label + (selected ? " ✓" : ""));
            var id = accent;
            button.Click += async (_, _) => await SetLookAsync(look.Theme is AppPreferences.FollowLotus or "Custom" ? "Midnight" : look.Theme, id, null);
            AccentDots.Children.Add(button);
            if (selected) AccentName.Text = label;
        }
        // Following Lotus or a custom theme brings its own colors: the dots apply to the modes.
        AccentArea.Opacity = look.Theme is AppPreferences.FollowLotus or "Custom" ? 0.55 : 1;
        if (look.Theme == AppPreferences.FollowLotus) AccentName.Text = Strings.Get("AppearanceAccentFromLotus");
        else if (look.Theme == "Custom") AccentName.Text = Strings.Get("AppearanceAccentCustom");

        GalleryCards.Children.Clear();
        var activeCode = look.Theme == "Custom" ? look.CustomTheme : null;
        foreach (var preset in Lotus.Design.Themes.ThemeGallery.All)
            GalleryCards.Children.Add(GalleryCard(preset.Spec, preset.Spec.Name, activeCode));
        // A theme pasted or created in Lotus that is not in the gallery also shows, so it can be picked again.
        if (activeCode is not null && Lotus.Design.Themes.ThemeEngine.TryFromCode(activeCode, out var mine)
            && !Lotus.Design.Themes.ThemeGallery.All.Any(p => Lotus.Design.Themes.ThemeEngine.ToCode(p.Spec) == activeCode))
            GalleryCards.Children.Insert(0, GalleryCard(mine, mine.Name, activeCode));

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
                Child = new Image { Stretch = Stretch.UniformToFill, Source = new Microsoft.UI.Xaml.Media.Imaging.BitmapImage(new Uri("ms-appx:///" + path)) { DecodePixelWidth = 336 } },
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

    /// <summary>A mode card painted in that mode's own colors, with a sample accent bar and a check when chosen.</summary>
    private Button ModeCard(string theme, bool selected, Windows.UI.Color accentFill)
    {
        var follow = theme == AppPreferences.FollowLotus;
        var (bg, card, text) = follow ? ("#2A0F24", "#3A1530", "#F8F5FA") : Previews[theme];
        var content = new Grid { Padding = new Thickness(18, 16, 18, 16), RowSpacing = 10 };
        content.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        content.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        content.RowDefinitions.Add(new RowDefinition());
        var name = new TextBlock { Text = Strings.Get("Theme" + theme), FontSize = 17, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, Foreground = Hex(text) };
        var head = new Grid();
        head.Children.Add(name);
        if (selected) head.Children.Add(new FontIcon { Glyph = "", FontSize = 14, Foreground = Hex(text), HorizontalAlignment = HorizontalAlignment.Right });
        if (follow)
        {
            var petal = new Lotus.Design.Controls.LotusPetal { IsLit = true, Width = 26, Height = 22, HorizontalAlignment = HorizontalAlignment.Right, VerticalAlignment = VerticalAlignment.Bottom };
            Grid.SetRow(petal, 2);
            content.Children.Add(petal);
        }
        content.Children.Add(head);
        var bar = new Border { Height = 7, Width = 120, CornerRadius = new CornerRadius(3.5), HorizontalAlignment = HorizontalAlignment.Left, Background = new SolidColorBrush(theme == "Light" ? accentFill : Lighter(accentFill)) };
        Grid.SetRow(bar, 1);
        content.Children.Add(bar);
        var hint = new TextBlock { Text = Strings.Get("Theme" + theme + "Hint"), FontSize = 11.5, Foreground = Hex(text), Opacity = 0.78, TextWrapping = TextWrapping.Wrap, VerticalAlignment = VerticalAlignment.Bottom };
        if (follow) hint.Margin = new Thickness(0, 0, 32, 0);
        Grid.SetRow(hint, 2);
        content.Children.Add(hint);

        var surface = new Border
        {
            Child = content,
            CornerRadius = new CornerRadius(14),
            Margin = new Thickness(0, 0, 12, 12),
            BorderThickness = new Thickness(selected ? 2 : 1),
            BorderBrush = selected ? (Brush)Application.Current.Resources["Accent.Rose"] : (Brush)Application.Current.Resources["Stroke.Subtle"],
            Background = theme switch
            {
                // Light above, dark below: like Windows' own choice.
                "System" => new LinearGradientBrush(new GradientStopCollection { new() { Color = Color("#FFFFFF"), Offset = 0.56 }, new() { Color = Color("#211A29"), Offset = 0.56 } }, 90),
                // "Igual ao Lotus": the Lotus rose.
                AppPreferences.FollowLotus => new LinearGradientBrush(new GradientStopCollection { new() { Color = Color("#A8124F"), Offset = 0 }, new() { Color = Color("#2A0F24"), Offset = 1 } }, 35),
                _ => Hex(card),
            },
        };
        if (theme == "System")
        {
            name.Foreground = Hex("#281C30");
            hint.Foreground = Hex("#F8F5FA");
            hint.Opacity = 0.9;
            foreach (var icon in head.Children.OfType<FontIcon>()) icon.Foreground = Hex("#281C30");
        }
        _ = bg;
        var button = CardButton(surface, Strings.Get("Theme" + theme) + (selected ? " ✓" : ""));
        button.Click += async (_, _) => await SetLookAsync(theme, look?.Accent ?? "rose", null);
        return button;
    }

    /// <summary>A gallery theme: its background colors, a mini card in its surface color with text, accent and detail.</summary>
    private Button GalleryCard(Lotus.Design.Themes.ThemeSpec spec, string name, string? activeCode)
    {
        var k = Lotus.Design.Themes.ThemeEngine.Derive(spec);
        static SolidColorBrush B(Lotus.Design.Themes.ThemeColor c, byte a = 255) => new(Windows.UI.Color.FromArgb(a, c.R, c.G, c.B));
        var code = Lotus.Design.Themes.ThemeEngine.ToCode(spec);
        var selected = activeCode == code;

        var stops = new GradientStopCollection();
        var colors = spec.Backdrop.Colors;
        for (var i = 0; i < colors.Count; i++)
        {
            var c = Lotus.Design.Themes.ThemeColor.Parse(colors[i]);
            stops.Add(new GradientStop { Color = Windows.UI.Color.FromArgb(255, c.R, c.G, c.B), Offset = (double)i / Math.Max(1, colors.Count - 1) });
        }
        var art = new Grid { Height = 74, CornerRadius = new CornerRadius(10), Background = new LinearGradientBrush(stops, 30) { Opacity = spec.Light ? 0.75 : 0.9 } };
        var mini = new Border
        {
            Width = 116, Height = 46, CornerRadius = new CornerRadius(8), HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Bottom,
            Margin = new Thickness(10, 0, 0, 10), Padding = new Thickness(9, 7, 9, 7), BorderThickness = new Thickness(1),
            Background = B(k.Card, (byte)Math.Round(k.CardOpacity * 255)), BorderBrush = B(k.Border),
        };
        var lines = new StackPanel { Spacing = 5 };
        lines.Children.Add(new Border { Height = 6, Width = 70, CornerRadius = new CornerRadius(3), HorizontalAlignment = HorizontalAlignment.Left, Background = B(k.Text) });
        var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 6 };
        row.Children.Add(new Border { Height = 12, Width = 40, CornerRadius = new CornerRadius(6), Background = B(k.AccentFill) });
        row.Children.Add(new Ellipse { Width = 12, Height = 12, Fill = B(k.Detail) });
        lines.Children.Add(row);
        mini.Child = lines;
        art.Children.Add(mini);

        var head = new Grid { ColumnSpacing = 6 };
        head.ColumnDefinitions.Add(new ColumnDefinition());
        head.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        head.Children.Add(new TextBlock { Text = name, FontSize = 14, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, Foreground = B(k.Text), TextTrimming = TextTrimming.CharacterEllipsis, VerticalAlignment = VerticalAlignment.Center });
        if (selected)
        {
            var check = new FontIcon { Glyph = "", FontSize = 14, Foreground = B(k.AccentText) };
            Grid.SetColumn(check, 1);
            head.Children.Add(check);
        }
        var kind = new TextBlock { Text = Strings.Get(spec.Light ? "AppearanceLight" : "AppearanceDark"), FontSize = 11.5, Foreground = B(k.TextSecondary) };
        var body = new StackPanel { Spacing = 6, Padding = new Thickness(10), Children = { art, head, kind } };
        var surface = new Border
        {
            Child = body,
            CornerRadius = new CornerRadius(14),
            Margin = new Thickness(0, 0, 12, 12),
            Background = B(k.Canvas),
            BorderThickness = new Thickness(selected ? 2 : 1),
            BorderBrush = selected ? (Brush)Application.Current.Resources["Accent.Rose"] : (Brush)Application.Current.Resources["Stroke.Subtle"],
        };
        var button = CardButton(surface, name + (selected ? " ✓" : ""));
        button.Click += async (_, _) => await SetLookAsync("Custom", look?.Accent ?? "rose", code);

        // Right click: use, or copy the code to share (the same code works in Lotus).
        var menu = new MenuFlyout();
        var use = new MenuFlyoutItem { Text = Strings.Get("AppearanceUse"), Icon = new FontIcon { Glyph = "" } };
        use.Click += async (_, _) => await SetLookAsync("Custom", look?.Accent ?? "rose", code);
        var copy = new MenuFlyoutItem { Text = Strings.Get("AppearanceCopyCode"), Icon = new FontIcon { Glyph = "" } };
        copy.Click += (_, _) =>
        {
            var data = new Windows.ApplicationModel.DataTransfer.DataPackage();
            data.SetText(code);
            Windows.ApplicationModel.DataTransfer.Clipboard.SetContent(data);
            AppearanceStatus.Text = Strings.Get("AppearanceCopied");
        };
        menu.Items.Add(use);
        menu.Items.Add(copy);
        button.ContextFlyout = menu;
        return button;
    }

    private static Button CardButton(Border surface, string automationName)
    {
        var button = new Button
        {
            Content = surface,
            Padding = new Thickness(0),
            Background = new SolidColorBrush(Microsoft.UI.Colors.Transparent),
            BorderThickness = new Thickness(0),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
            VerticalContentAlignment = VerticalAlignment.Stretch,
        };
        Lotus.Design.Motion.SetLift(button, true);
        AutomationProperties.SetName(button, automationName);
        return button;
    }

    private static Windows.UI.Color Color(string hex) => Windows.UI.Color.FromArgb(255, Convert.ToByte(hex[1..3], 16), Convert.ToByte(hex[3..5], 16), Convert.ToByte(hex[5..7], 16));

    private static SolidColorBrush Hex(string hex) => new(Color(hex));

    private static Windows.UI.Color Lighter(Windows.UI.Color c) =>
        Windows.UI.Color.FromArgb(255, (byte)(c.R + (255 - c.R) * 0.45), (byte)(c.G + (255 - c.G) * 0.45), (byte)(c.B + (255 - c.B) * 0.45));

    private async Task SetLookAsync(string theme, string accent, string? code)
    {
        await appearance.SetAsync(theme, accent, code);
        await ShowAppearanceAsync();
    }

    private async void OnBannerClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is Border { Tag: string id }) await appearance.SetHomeBannerAsync(id);
    }

    /// <summary>Like Lotus: any text with a Lotus theme code in it (a whole chat message works too) is used.</summary>
    private async void OnPasteTheme(object sender, RoutedEventArgs e)
    {
        string? clip = null;
        try
        {
            var content = Windows.ApplicationModel.DataTransfer.Clipboard.GetContent();
            if (content.Contains(Windows.ApplicationModel.DataTransfer.StandardDataFormats.Text)) clip = await content.GetTextAsync();
        }
        catch (Exception ex) when (ex is System.Runtime.InteropServices.COMException or UnauthorizedAccessException)
        {
        }
        var code = clip is null ? null : System.Text.RegularExpressions.Regex.Match(clip, @"LOTUS-THEME:[A-Za-z0-9_\-]+").Value;
        if (string.IsNullOrEmpty(code) || !Lotus.Design.Themes.ThemeEngine.TryFromCode(code, out var spec))
        {
            AppearanceStatus.Text = Strings.Get("AppearanceBadCode");
            return;
        }
        await SetLookAsync("Custom", look?.Accent ?? "rose", code);
        AppearanceStatus.Text = Strings.Format("AppearancePasted", spec.Name);
    }

    /// <summary>The Theme Studio lives in Lotus: open it there; "Igual ao Lotus" brings the result here.</summary>
    private async void OnCreateInLotus(object sender, RoutedEventArgs e)
    {
        try
        {
            if (LotusPorts.Infrastructure.Lotus.LotusProfileReader.OpenLotus()) return;
        }
        catch (System.ComponentModel.Win32Exception)
        {
        }
        await Windows.System.Launcher.LaunchUriAsync(LotusPorts.Infrastructure.Lotus.LotusProfileReader.DownloadPage);
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
