using System.Runtime.InteropServices;
using LotusPorts.App.Views;
using LotusPorts.Infrastructure.Lotus;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;
using Windows.Graphics;

namespace LotusPorts.App;

public sealed partial class MainWindow : Window
{
    private readonly Dictionary<string, RadioButton> nav;
    private bool navigating;

    public MainWindow(string startPage)
    {
        InitializeComponent();
        Title = "Lotus Ports";
        AppWindow.SetIcon(System.IO.Path.Combine(AppContext.BaseDirectory, "Assets", "Art", "Brand", "app.ico"));
        ExtendsContentIntoTitleBar = true;
        SetTitleBar(DragRegion);
        var titleBar = AppWindow.TitleBar;
        titleBar.PreferredHeightOption = TitleBarHeightOption.Tall;
        titleBar.ButtonBackgroundColor = Colors.Transparent;
        titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
        titleBar.ButtonForegroundColor = Colors.White;
        titleBar.ButtonInactiveForegroundColor = Colors.Gray;
        titleBar.ButtonHoverBackgroundColor = Windows.UI.Color.FromArgb(0x26, 0xFF, 0xFF, 0xFF);

        var scale = GetDpiForWindow(WinRT.Interop.WindowNative.GetWindowHandle(this)) / 96.0;
        AppWindow.Resize(new SizeInt32((int)(1380 * scale), (int)(880 * scale)));
        if (AppWindow.Presenter is OverlappedPresenter presenter)
        {
            presenter.PreferredMinimumWidth = (int)(1024 * scale);
            presenter.PreferredMinimumHeight = (int)(680 * scale);
        }

        nav = new()
        {
            [Pages.Home] = NavHome,
            [Pages.Library] = NavLibrary,
            [Pages.Catalog] = NavCatalog,
            [Pages.Downloads] = NavDownloads,
            [Pages.Lotus] = NavLotus,
            [Pages.Settings] = NavSettings,
            [Pages.Help] = NavHelp,
        };
        // "game:<id>" opens a game page directly (Debug --page argument).
        var split = startPage.Split(':', 2);
        Navigate(split[0], split.Length > 1 ? split[1] : null);
        ShowProfile();
        Activated += OnActivated;
        // Windows gives the first nav item keyboard focus when the window opens, which draws a focus ring nobody asked
        // for. Start with quiet (programmatic) focus on the current page's item instead; Tab still shows the ring.
        if (Content is FrameworkElement root)
            root.Loaded += (_, _) => (nav.GetValueOrDefault(split[0]) ?? NavHome).Focus(FocusState.Programmatic);
        // Theme: light/dark for WinUI controls and the caption buttons follow the family theme, live.
        var appearance = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<AppearanceService>(App.Services);
        void ShowTheme()
        {
            Root.RequestedTheme = appearance.Element;
            var light = appearance.Element == ElementTheme.Light;
            titleBar.ButtonForegroundColor = light ? Windows.UI.Color.FromArgb(0xFF, 0x2B, 0x10, 0x20) : Colors.White;
            titleBar.ButtonHoverBackgroundColor = light ? Windows.UI.Color.FromArgb(0x1F, 0x2B, 0x10, 0x20) : Windows.UI.Color.FromArgb(0x26, 0xFF, 0xFF, 0xFF);
        }
        appearance.Changed += ShowTheme;
        ShowTheme();
        Activated += (_, a) => { if (a.WindowActivationState != WindowActivationState.Deactivated) appearance.RefreshFromLotus(); };

        // Controller everywhere; TV mode (full screen, everything larger) from Configurações or the View button.
        navigator = new GamepadNavigator(DispatcherQueue, () => Content?.XamlRoot);
        navigator.Command += OnPadCommand;
        Activated += (_, a) => navigator.SetWindowActive(a.WindowActivationState != WindowActivationState.Deactivated);
        SizeChanged += (_, _) => ApplyScale();
        _ = LoadTvModeAsync();
        _ = ShowWelcomeIfNewAsync();

        // The petal lights up only while Lotus is really connected.
        var bridge = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<LotusBridge>(App.Services);
        void ShowLotus() => NavLotusPetal.IsLit = bridge.Status.State == LotusPorts.Integration.LotusConnectionState.Connected;
        bridge.Changed += ShowLotus;
        ShowLotus();
    }

    /// <summary>Shows the window and puts it in front (a second start of the app was redirected here).</summary>
    public void BringToFront()
    {
        if (AppWindow.Presenter is OverlappedPresenter { State: OverlappedPresenterState.Minimized } p) p.Restore();
        AppWindow.Show();
        Activate();
        SetForegroundWindow(WinRT.Interop.WindowNative.GetWindowHandle(this));
    }

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    public void Navigate(string page, object? parameter = null)
    {
        var type = page switch
        {
            Pages.Library => typeof(LibraryPage),
            Pages.Catalog => typeof(CatalogPage),
            Pages.Downloads => typeof(SimplePage),
            Pages.Lotus => typeof(LotusPage),
            Pages.Settings => typeof(SettingsPage),
            Pages.Help => typeof(HelpPage),
            Pages.Game => typeof(GamePage),
            _ => typeof(HomePage),
        };
        if (page != Pages.Game) { lastPage = page; lastParameter = parameter; }
        // Game pages drill in; screens from the sidebar enter softly; nothing moves when Windows reduces animations.
        NavigationTransitionInfo transition = !Lotus.Design.Motion.Enabled ? new SuppressNavigationTransitionInfo()
            : page == Pages.Game ? new DrillInNavigationTransitionInfo()
            : new EntranceNavigationTransitionInfo();
        ContentFrame.Navigate(type, type == typeof(SimplePage) ? page : parameter, transition);
        navigating = true;
        // A game page keeps the section it was opened from lit: that is where Back returns.
        var active = page == Pages.Game ? lastPage : nav.ContainsKey(page) ? page : Pages.Home;
        foreach (var (id, button) in nav) button.IsChecked = id == active;
        navigating = false;
    }

    private string lastPage = Pages.Home;
    private object? lastParameter;

    /// <summary>From a game page, back to the screen it was opened from.</summary>
    public void GoBack() => Navigate(lastPage, lastParameter);

    private void OnNavChecked(object sender, RoutedEventArgs e)
    {
        if (!navigating && sender is RadioButton { Tag: string page }) Navigate(page);
    }

    private readonly LotusProfileReader profiles = new();
    private LotusProfile? profile;

    /// <summary>The corner shows the same player as Lotus. Re-read when the window comes back, so a profile made in Lotus appears.</summary>
    private void ShowProfile()
    {
        var current = profiles.Read();
        if (profile is not null && current == profile) return;
        profile = current;
        if (profile is { } p)
        {
            ProfileName.Text = p.DisplayName;
            ProfileHint.Text = Strings.Get("ProfileFromLotus");
            ProfilePicture.DisplayName = p.DisplayName;
            ProfilePicture.ProfilePicture = p.AvatarPath is { } path ? new BitmapImage(new Uri(path)) { DecodePixelWidth = 68 } : null;
            ProfilePicture.Visibility = Visibility.Visible;
            ProfilePetal.Visibility = Visibility.Collapsed;
            ProfileArrow.Visibility = Visibility.Collapsed;
            AutomationProperties.SetName(ProfileButton, Strings.Format("ProfileAutomation", p.DisplayName));
        }
        else
        {
            ProfileName.Text = Strings.Get("ProfileCreate");
            ProfileHint.Text = Strings.Get(LotusProfileReader.FindLotusExecutable() is null ? "ProfileGetLotus" : "ProfileInLotus");
            ProfilePicture.Visibility = Visibility.Collapsed;
            ProfilePetal.Visibility = Visibility.Visible;
            ProfileArrow.Visibility = Visibility.Visible;
            AutomationProperties.SetName(ProfileButton, $"{ProfileName.Text}. {ProfileHint.Text}");
        }
    }

    private void OnActivated(object sender, WindowActivatedEventArgs args)
    {
        if (args.WindowActivationState != WindowActivationState.Deactivated) ShowProfile();
    }

    /// <summary>With a profile: opens Lotus, where it is edited. Without: Lotus creates it; without Lotus, its download page.</summary>
    private async void OnProfileClick(object sender, RoutedEventArgs e)
    {
        try
        {
            if (LotusProfileReader.OpenLotus()) return;
        }
        catch (System.ComponentModel.Win32Exception)
        {
            // Lotus could not start (blocked or removed): fall back to its page.
        }
        await Windows.System.Launcher.LaunchUriAsync(LotusProfileReader.DownloadPage);
    }

    // A game file dropped anywhere is read and identified (nothing is copied or moved).
    private void OnDragOver(object sender, DragEventArgs e)
    {
        if (!e.DataView.Contains(Windows.ApplicationModel.DataTransfer.StandardDataFormats.StorageItems)) return;
        e.AcceptedOperation = Windows.ApplicationModel.DataTransfer.DataPackageOperation.Link;
        e.DragUIOverride.Caption = Strings.Get("DropIdentify");
    }

    private async void OnDrop(object sender, DragEventArgs e)
    {
        if (!e.DataView.Contains(Windows.ApplicationModel.DataTransfer.StandardDataFormats.StorageItems)) return;
        var items = await e.DataView.GetStorageItemsAsync();
        if (items.OfType<Windows.Storage.StorageFile>().FirstOrDefault() is not { } file) return;
        var forGame = ContentFrame.Content is GamePage page ? page.Game : null;
        await GamePage.IdentifyAndTellAsync(file.Path, forGame, Content.XamlRoot);
    }

    // ---- Modo TV e controle (phase 7) ----------------------------------------------------------

    // ---- Primeiro uso ----------------------------------------------------------------------------

    /// <summary>The welcome appears on the first launch only (and on demand from Ajuda).</summary>
    private async Task ShowWelcomeIfNewAsync()
    {
        var args = Environment.GetCommandLineArgs();
#if DEBUG
        // Development screenshots open a page directly: the welcome only when asked with --welcome.
        if (args.FirstOrDefault(a => a.StartsWith("--welcome", StringComparison.Ordinal)) is { } w) { ShowWelcome(w.Length > 10 && int.TryParse(w[10..], out var n) ? n : 0); return; }
        if (args.Any(a => a.StartsWith("--page=", StringComparison.Ordinal))) return;
#endif
        var store = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<LotusPorts.Core.Preferences.ISettingsStore>(App.Services);
        try
        {
            if (await store.GetAsync(Controls.WelcomeView.DoneKey) == "true") return;
        }
        catch (Exception ex)
        {
            Serilog.Log.Warning(ex, "Could not read the welcome setting");
            return; // better to skip the welcome than to show it on every launch
        }
        ShowWelcome();
    }

    public void ShowWelcome(int startStep = 0)
    {
        var view = new Controls.WelcomeView { StartStep = startStep };
        view.Closed += () =>
        {
            WelcomeHost.Children.Clear();
            WelcomeHost.Visibility = Visibility.Collapsed;
        };
        WelcomeHost.Children.Clear();
        WelcomeHost.Children.Add(view);
        WelcomeHost.Visibility = Visibility.Visible;
    }

    public const string TvModeKey = "general.tvMode";
    private const double TvScale = 1.35;
    private readonly GamepadNavigator navigator;

    /// <summary>TV mode is on: full screen and the whole interface scaled up for the couch.</summary>
    public bool TvMode { get; private set; }

    public event Action? TvModeChanged;

    private async Task LoadTvModeAsync()
    {
        var store = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<LotusPorts.Core.Preferences.ISettingsStore>(App.Services);
        if (bool.TryParse(await store.GetAsync(TvModeKey), out var on) && on) SetTvMode(true, save: false);
#if DEBUG
        // --tv (Debug only): preview TV mode on screenshots without saving it.
        if (Environment.GetCommandLineArgs().Contains("--tv")) SetTvMode(true, save: false);
#endif
    }

    public void SetTvMode(bool on, bool save = true)
    {
        if (TvMode == on) return;
        TvMode = on;
        AppWindow.SetPresenter(on ? AppWindowPresenterKind.FullScreen : AppWindowPresenterKind.Overlapped);
        ApplyScale();
        if (save)
        {
            var store = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<LotusPorts.Core.Preferences.ISettingsStore>(App.Services);
            _ = store.SetAsync(TvModeKey, on.ToString());
        }
        TvModeChanged?.Invoke();
    }

    /// <summary>
    /// Scales the whole interface: layout is measured at the window size divided by the scale and then drawn larger, so
    /// nothing is cut off and the same screens simply grow.
    /// </summary>
    private void ApplyScale()
    {
        if (Content is not FrameworkElement root) return;
        var scale = TvMode ? TvScale : 1.0;
        if (scale == 1.0)
        {
            root.RenderTransform = null;
            root.Width = double.NaN;
            root.Height = double.NaN;
            return;
        }
        var size = AppWindow.Size;
        var dpi = root.XamlRoot?.RasterizationScale ?? 1.0;
        root.HorizontalAlignment = HorizontalAlignment.Left;
        root.VerticalAlignment = VerticalAlignment.Top;
        root.Width = size.Width / dpi / scale;
        root.Height = size.Height / dpi / scale;
        root.RenderTransform = new Microsoft.UI.Xaml.Media.ScaleTransform { ScaleX = scale, ScaleY = scale };
    }

    private static readonly string[] Sections = [Pages.Home, Pages.Library, Pages.Catalog, Pages.Downloads, Pages.Lotus, Pages.Settings, Pages.Help];

    private void OnPadCommand(PadAction action)
    {
        switch (action)
        {
            case PadAction.Back:
                if (ContentFrame.Content is GamePage) GoBack();
                else if (TvMode) SetTvMode(false);
                break;
            case PadAction.PreviousSection or PadAction.NextSection:
                var current = Array.IndexOf(Sections, lastPage);
                var next = (Math.Max(0, current) + (action == PadAction.NextSection ? 1 : Sections.Length - 1)) % Sections.Length;
                Navigate(Sections[next]);
                break;
            case PadAction.Search:
                if (ContentFrame.Content is LibraryPage library) library.FocusSearch();
                else
                {
                    if (ContentFrame.Content is not HomePage) Navigate(Pages.Home);
                    (ContentFrame.Content as HomePage)?.FocusSearch();
                }
                break;
            case PadAction.ToggleTvMode:
                SetTvMode(!TvMode);
                break;
        }
    }

    private void OnSearchShortcut(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        args.Handled = true;
        if (ContentFrame.Content is LibraryPage library) { library.FocusSearch(); return; }
        if (ContentFrame.Content is not HomePage) Navigate(Pages.Home);
        (ContentFrame.Content as HomePage)?.FocusSearch();
    }

    [LibraryImport("user32.dll")]
    private static partial uint GetDpiForWindow(nint hwnd);
}
