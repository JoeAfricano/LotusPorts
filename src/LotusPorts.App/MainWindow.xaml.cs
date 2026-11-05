using System.Runtime.InteropServices;
using LotusPorts.App.Views;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
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
    }

    public void Navigate(string page, object? parameter = null)
    {
        var type = page switch
        {
            Pages.Catalog => typeof(CatalogPage),
            Pages.Downloads or Pages.Lotus => typeof(SimplePage),
            Pages.Settings => typeof(SettingsPage),
            Pages.Help => typeof(HelpPage),
            Pages.Game => typeof(GamePage),
            _ => typeof(LibraryPage),
        };
        ContentFrame.Navigate(type, type == typeof(SimplePage) ? page : parameter);
        navigating = true;
        foreach (var (id, button) in nav) button.IsChecked = id == page || (page == Pages.Game && id == Pages.Library);
        navigating = false;
    }

    private void OnNavChecked(object sender, RoutedEventArgs e)
    {
        if (!navigating && sender is RadioButton { Tag: string page }) Navigate(page);
    }

    private void OnLotusStatusClick(object sender, RoutedEventArgs e) => Navigate(Pages.Lotus);

    private void OnSearchShortcut(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        args.Handled = true;
        if (ContentFrame.Content is not LibraryPage) Navigate(Pages.Library);
        (ContentFrame.Content as LibraryPage)?.FocusSearch();
    }

    [LibraryImport("user32.dll")]
    private static partial uint GetDpiForWindow(nint hwnd);
}
