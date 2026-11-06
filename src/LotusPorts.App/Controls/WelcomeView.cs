using Lotus.Design.Controls;
using LotusPorts.Core.Preferences;
using LotusPorts.Infrastructure.Lotus;
using LotusPorts.Integration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Storage.Pickers;

namespace LotusPorts.App.Controls;

/// <summary>
/// The first-run welcome: four short steps (who you are in Lotus, where your game files are, the default style,
/// Lotus and the controller). Every step can be skipped; nothing here installs or copies anything.
/// Shown once: closing it in any way records <see cref="DoneKey"/>.
/// </summary>
public sealed partial class WelcomeView : UserControl
{
    public const string DoneKey = "general.welcomeDone";
    private const int StepCount = 4;

    private readonly Grid stepHost = new();
    private readonly StackPanel dots = new() { Orientation = Orientation.Horizontal, Spacing = 8, VerticalAlignment = VerticalAlignment.Center };
    private readonly Button back = new();
    private readonly Button next = new();
    private readonly Button skip = new();
    private int step;

    public event Action? Closed;

    /// <summary>Development screenshots only (--welcome=N).</summary>
    public int StartStep { get; init; }

    public WelcomeView()
    {
        var res = Application.Current.Resources;
        back.Content = Strings.Get("WelcomeBack");
        next.Style = (Style)res["AccentButtonStyle"];
        skip.Content = Strings.Get("WelcomeSkip");
        back.Click += (_, _) => Go(step - 1);
        next.Click += (_, _) => { if (step == StepCount - 1) Finish(); else Go(step + 1); };
        skip.Click += (_, _) => Finish();

        var footer = new Grid { ColumnSpacing = 10, Margin = new Thickness(0, 26, 0, 0) };
        footer.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        footer.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        footer.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        footer.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        footer.Children.Add(dots);
        Grid.SetColumn(skip, 1); footer.Children.Add(skip);
        Grid.SetColumn(back, 2); footer.Children.Add(back);
        Grid.SetColumn(next, 3); footer.Children.Add(next);

        var card = new Border
        {
            Style = (Style)res["Lotus.Card"],
            MaxWidth = 720,
            Padding = new Thickness(40, 36, 40, 30),
            Margin = new Thickness(32),
            HorizontalAlignment = HorizontalAlignment.Stretch, // with MaxWidth: same width on every step, centered
            VerticalAlignment = VerticalAlignment.Center,
            Child = new StackPanel { Children = { stepHost, footer } },
        };
        stepHost.ChildrenTransitions = new Microsoft.UI.Xaml.Media.Animation.TransitionCollection { new Microsoft.UI.Xaml.Media.Animation.EntranceThemeTransition { FromHorizontalOffset = 40, FromVerticalOffset = 0 } };

        Content = new Grid
        {
            Children =
            {
                new SceneHero { Source = "Assets/Art/Banners/lotus_river.png" },
                new Border { Background = new SolidColorBrush(Windows.UI.Color.FromArgb(0xB8, 0x08, 0x05, 0x0B)) },
                new ScrollViewer { Content = card, VerticalScrollBarVisibility = ScrollBarVisibility.Auto },
            },
        };
        Loaded += (_, _) => { Go(StartStep); next.Focus(FocusState.Programmatic); };
    }

    private static T Service<T>() where T : notnull => App.Services.GetRequiredService<T>();

    private void Go(int index)
    {
        step = Math.Clamp(index, 0, StepCount - 1);
        stepHost.Children.Clear();
        stepHost.Children.Add(step switch
        {
            0 => ProfileStep(),
            1 => FilesStep(),
            2 => StyleStep(),
            _ => LotusStep(),
        });
        back.Visibility = step == 0 ? Visibility.Collapsed : Visibility.Visible;
        next.Content = Strings.Get(step == StepCount - 1 ? "WelcomeStart" : "WelcomeNext");
        skip.Visibility = step == StepCount - 1 ? Visibility.Collapsed : Visibility.Visible;
        dots.Children.Clear();
        for (var i = 0; i < StepCount; i++)
            dots.Children.Add(new Border
            {
                Width = i == step ? 22 : 8,
                Height = 8,
                CornerRadius = new CornerRadius(4),
                Background = (Brush)Application.Current.Resources[i == step ? "Accent.Rose" : "Stroke.Subtle"],
            });
        Microsoft.UI.Xaml.Automation.AutomationProperties.SetName(dots, Strings.Format("WelcomeStepOf", step + 1, StepCount));
    }

    private async void Finish()
    {
        try
        {
            await Service<ISettingsStore>().SetAsync(DoneKey, "true");
        }
        catch (Exception ex)
        {
            Serilog.Log.Warning(ex, "Could not record the welcome as seen");
        }
        Closed?.Invoke();
    }

    // ---- Steps ---------------------------------------------------------------------------------

    private static StackPanel Page(string eyebrow, string title, string body, params UIElement[] extra)
    {
        var res = Application.Current.Resources;
        var panel = new StackPanel { Spacing = 10 };
        panel.Children.Add(new TextBlock { Text = eyebrow, CharacterSpacing = 160, FontSize = 12, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, Foreground = (Brush)res["Accent.Rose"] });
        panel.Children.Add(new TextBlock { Text = title, Style = (Style)res["Type.Title"], TextWrapping = TextWrapping.Wrap });
        panel.Children.Add(new TextBlock { Text = body, Style = (Style)res["Type.Body"], Foreground = (Brush)res["Text.Secondary"], TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 0, 0, 8) });
        foreach (var e in extra) panel.Children.Add(e);
        return panel;
    }

    private static TextBlock Caption(string text) => new()
    {
        Text = text,
        Style = (Style)Application.Current.Resources["Type.Caption"],
        TextWrapping = TextWrapping.Wrap,
    };

    private static Border Row(UIElement icon, string title, string hint, UIElement? action = null)
    {
        var res = Application.Current.Resources;
        var grid = new Grid { ColumnSpacing = 16 };
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        grid.Children.Add(icon);
        var text = new StackPanel { VerticalAlignment = VerticalAlignment.Center, Spacing = 2 };
        text.Children.Add(new TextBlock { Text = title, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, TextWrapping = TextWrapping.Wrap });
        text.Children.Add(Caption(hint));
        Grid.SetColumn(text, 1);
        grid.Children.Add(text);
        if (action is FrameworkElement a)
        {
            a.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetColumn(a, 2);
            grid.Children.Add(a);
        }
        return new Border
        {
            Padding = new Thickness(16, 14, 16, 14),
            CornerRadius = new CornerRadius(12),
            Background = (Brush)res["Bg.Sidebar"],
            BorderBrush = (Brush)res["Stroke.Subtle"],
            BorderThickness = new Thickness(1),
            Child = grid,
        };
    }

    private static FontIcon Glyph(string glyph) => new()
    {
        Glyph = glyph,
        FontSize = 22,
        Width = 44,
        Foreground = (Brush)Application.Current.Resources["Accent.Rose"],
        VerticalAlignment = VerticalAlignment.Center,
    };

    private static UIElement ProfileStep()
    {
        var logo = new Image { Source = new BitmapImage(new Uri("ms-appx:///Assets/Art/Brand/app-icon-128.png")), Width = 72, Height = 72, HorizontalAlignment = HorizontalAlignment.Left, Margin = new Thickness(0, 0, 0, 6) };
        UIElement profileRow;
        if (new LotusProfileReader().Read() is { } p)
        {
            var picture = new PersonPicture { Width = 44, Height = 44, DisplayName = p.DisplayName };
            if (p.AvatarPath is { } path) picture.ProfilePicture = new BitmapImage(new Uri(path)) { DecodePixelWidth = 88 };
            profileRow = Row(picture, Strings.Format("WelcomeHello", p.DisplayName), Strings.Get("WelcomeProfileFound"));
        }
        else
        {
            var create = new Button { Content = Strings.Get(LotusProfileReader.FindLotusExecutable() is null ? "WelcomeGetLotus" : "WelcomeOpenLotus") };
            create.Click += async (_, _) =>
            {
                try { if (LotusProfileReader.OpenLotus()) return; }
                catch (System.ComponentModel.Win32Exception) { }
                await Windows.System.Launcher.LaunchUriAsync(LotusProfileReader.DownloadPage);
            };
            profileRow = Row(Glyph(""), Strings.Get("WelcomeNoProfile"), Strings.Get("WelcomeNoProfileHint"), create);
        }
        var panel = Page(Strings.Get("WelcomeEyebrow"), Strings.Get("WelcomeTitle"), Strings.Get("WelcomeBody"), profileRow);
        panel.Children.Insert(0, logo);
        return panel;
    }

    private UIElement FilesStep()
    {
        var files = Service<GameFilesService>();
        var status = Caption("");
        void Show() => status.Text = files.Folders.Count == 0
            ? Strings.Get("WelcomeFilesNone")
            : Strings.Format("WelcomeFilesChosen", string.Join(" · ", files.Folders));
        Show();
        var choose = new Button { Content = Strings.Get("WelcomeFilesChoose") };
        choose.Click += async (_, _) =>
        {
            var picker = new FolderPicker { SuggestedStartLocation = PickerLocationId.ComputerFolder };
            picker.FileTypeFilter.Add("*");
            WinRT.Interop.InitializeWithWindow.Initialize(picker, WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow));
            if (await picker.PickSingleFolderAsync() is not { } folder) return;
            await files.AddFolderAsync(folder.Path);
            Show();
        };
        return Page(Strings.Get("WelcomeFilesEyebrow"), Strings.Get("WelcomeFilesTitle"), Strings.Get("WelcomeFilesBody"),
            Row(Glyph(""), Strings.Get("WelcomeFilesRow"), Strings.Get("WelcomeFilesRowHint"), choose),
            status,
            Caption(Strings.Get("WelcomeFilesLegal")));
    }

    private UIElement StyleStep()
    {
        var preferences = Service<PreferencesService>();
        var choices = new StackPanel { Spacing = 10 };
        RadioButton Option(ExperienceStyle style)
        {
            var radio = new RadioButton
            {
                GroupName = "WelcomeStyle",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Content = new StackPanel
                {
                    Spacing = 2,
                    Children =
                    {
                        new TextBlock { Text = Strings.Get("Style" + style), FontWeight = Microsoft.UI.Text.FontWeights.SemiBold },
                        Caption(Strings.Get("Style" + style + "Hint")),
                    },
                },
            };
            radio.Checked += async (_, _) =>
            {
                try { await preferences.SetDefaultStyleAsync(style); }
                catch (Exception ex) { Serilog.Log.Warning(ex, "Could not save the default style"); }
            };
            return radio;
        }
        var original = Option(ExperienceStyle.Original);
        var remastered = Option(ExperienceStyle.Remastered);
        choices.Children.Add(original);
        choices.Children.Add(remastered);
        _ = Load();
        async Task Load()
        {
            var current = (await preferences.LoadAsync()).DefaultStyle;
            (current == ExperienceStyle.Original ? original : remastered).IsChecked = true;
        }
        var themes = new HyperlinkButton { Content = Strings.Get("WelcomeThemesLink"), Padding = new Thickness(0, 4, 0, 4) };
        themes.Click += (_, _) => { Finish(); App.MainWindow?.Navigate(Pages.Settings); };
        return Page(Strings.Get("WelcomeStyleEyebrow"), Strings.Get("WelcomeStyleTitle"), Strings.Get("WelcomeStyleBody"),
            choices, Caption(Strings.Get("WelcomeThemes")), themes);
    }

    private static UIElement LotusStep()
    {
        var status = Service<LotusBridge>().Status;
        var state = status.State == LotusConnectionState.Connected
            ? Strings.Format("LotusStateConnected", status.Version ?? "?")
            : Strings.Get("LotusState" + status.State);
        return Page(Strings.Get("WelcomeLotusEyebrow"), Strings.Get("WelcomeLotusTitle"), Strings.Get("WelcomeLotusBody"),
            Row(new LotusPetal { Width = 44, Height = 38, IsLit = status.State == LotusConnectionState.Connected }, state, Strings.Get("LotusStateHint" + status.State)),
            Row(Glyph(""), Strings.Get("WelcomeGamepad"), Strings.Get("WelcomeGamepadHint")),
            Row(Glyph(""), Strings.Get("WelcomeTv"), Strings.Get("WelcomeTvHint")));
    }
}
