using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace LotusPorts.App.Views;

/// <summary>Screens that, in this phase, only explain their honest current state: Catálogo, Downloads, Integração com o Lotus.</summary>
public sealed partial class SimplePage : Page
{
    public SimplePage() => InitializeComponent();

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        var prefix = e.Parameter switch
        {
            Pages.Catalog => "Catalog",
            Pages.Downloads => "Downloads",
            _ => "Lotus",
        };
        Heading.Text = Strings.Get(prefix + "Title");
        Subtitle.Text = Strings.Get(prefix + "Subtitle");
        Empty.Title = Strings.Get(prefix + "EmptyTitle");
        Empty.Message = Strings.Get(prefix + "EmptyMessage");
        // Downloads really is empty (nothing can be downloaded yet); the other two are still being built.
        Empty.BadgeText = prefix == "Downloads" ? "" : Strings.Get("BadgeInDevelopment");

        if (prefix != "Lotus") return;
        BulletsCard.Visibility = Visibility.Visible;
        BulletsTitle.Text = Strings.Get("LotusBulletsTitle");
        foreach (var line in Strings.Get("LotusBullets").Split('|', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
        {
            Bullets.Children.Add(new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 12,
                Children =
                {
                    new FontIcon { Glyph = "", FontSize = 12, Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["Accent.Rose"] },
                    new TextBlock { Text = line, Style = (Style)Application.Current.Resources["Type.Body"] },
                },
            });
        }
    }
}
