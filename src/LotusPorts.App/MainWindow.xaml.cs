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
    }

    public void Navigate(string page, object? parameter = null)
    {
        var type = page switch
        {
            Pages.Library => typeof(LibraryPage),
            Pages.Catalog => typeof(CatalogPage),
            Pages.Downloads or Pages.Lotus => typeof(SimplePage),
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
        var active = nav.ContainsKey(page) ? page : page == Pages.Game ? null : Pages.Home;
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
