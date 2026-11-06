using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace Lotus.Design.Controls;

/// <summary>
/// A compact banner header for a page: art from the package behind the page title. Placed first on a page with
/// negative margins equal to the page padding, so the art reaches the edges.
/// </summary>
public sealed partial class PageBanner : UserControl
{
    public static readonly DependencyProperty SourceProperty =
        DependencyProperty.Register(nameof(Source), typeof(string), typeof(PageBanner), new PropertyMetadata("", (d, e) => ((PageBanner)d).hero.Source = (string)e.NewValue));

    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(nameof(Title), typeof(string), typeof(PageBanner), new PropertyMetadata("", (d, e) => ((PageBanner)d).title.Text = (string)e.NewValue));

    private readonly SceneHero hero = new();
    private readonly TextBlock title = new();

    public PageBanner()
    {
        IsTabStop = false;
        Height = 170;
        title.Style = (Style)Application.Current.Resources["Type.Title"];
        title.Margin = new Thickness(28, 0, 28, 20);
        title.VerticalAlignment = VerticalAlignment.Bottom;
        title.Foreground = (Brush)Application.Current.Resources["Text.Primary"];
        Content = new Border
        {
            CornerRadius = new CornerRadius(16),
            BorderThickness = new Thickness(1),
            BorderBrush = (Brush)Application.Current.Resources["Stroke.Subtle"],
            Child = new Grid { Children = { hero, title } },
        };
    }

    public string Source { get => (string)GetValue(SourceProperty); set => SetValue(SourceProperty, value); }

    public string Title { get => (string)GetValue(TitleProperty); set => SetValue(TitleProperty, value); }
}
