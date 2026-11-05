using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Lotus.Design.Controls;

/// <summary>Empty / not-available message with a dim lotus, a useful sentence and an optional state badge.</summary>
public sealed partial class EmptyState : UserControl
{
    public static readonly DependencyProperty TitleProperty = Register(nameof(Title));
    public static readonly DependencyProperty MessageProperty = Register(nameof(Message));
    public static readonly DependencyProperty BadgeTextProperty = Register(nameof(BadgeText));
    public static readonly DependencyProperty IllustrationProperty = Register(nameof(Illustration));

    private readonly TextBlock title;
    private readonly TextBlock message;
    private readonly StateBadge badge = new() { Tone = BadgeTone.Unknown, HorizontalAlignment = HorizontalAlignment.Center };
    private readonly LotusPetal petal = new() { IsLit = false, Width = 64, Height = 56, Margin = new Thickness(0, 0, 0, 8) };
    private readonly Image art = new() { Width = 150, Height = 150, Margin = new Thickness(0, 0, 0, 4), Visibility = Visibility.Collapsed };

    public EmptyState()
    {
        var resources = Application.Current.Resources;
        title = new TextBlock { Style = (Style)resources["Type.Subtitle"], TextAlignment = TextAlignment.Center };
        message = new TextBlock { Style = (Style)resources["Type.Caption"], TextAlignment = TextAlignment.Center, MaxWidth = 460 };
        Content = new StackPanel
        {
            Spacing = 10,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Padding = new Thickness(24, 48, 24, 48),
            Children = { petal, art, title, message, badge },
        };
        Loaded += (_, _) => Update();
    }

    public string Title { get => (string)GetValue(TitleProperty); set => SetValue(TitleProperty, value); }
    public string Message { get => (string)GetValue(MessageProperty); set => SetValue(MessageProperty, value); }
    public string BadgeText { get => (string)GetValue(BadgeTextProperty); set => SetValue(BadgeTextProperty, value); }

    /// <summary>Optional app-relative image (art pack illustration). Without it, the dim lotus is shown.</summary>
    public string Illustration { get => (string)GetValue(IllustrationProperty); set => SetValue(IllustrationProperty, value); }

    private static DependencyProperty Register(string name) =>
        DependencyProperty.Register(name, typeof(string), typeof(EmptyState), new PropertyMetadata("", (d, _) => ((EmptyState)d).Update()));

    private void Update()
    {
        title.Text = Title;
        message.Text = Message;
        message.Visibility = string.IsNullOrEmpty(Message) ? Visibility.Collapsed : Visibility.Visible;
        badge.Text = BadgeText;
        badge.Visibility = string.IsNullOrEmpty(BadgeText) ? Visibility.Collapsed : Visibility.Visible;
        var hasArt = !string.IsNullOrEmpty(Illustration);
        art.Visibility = hasArt ? Visibility.Visible : Visibility.Collapsed;
        petal.Visibility = hasArt ? Visibility.Collapsed : Visibility.Visible;
        if (hasArt && art.Source is null)
            art.Source = new Microsoft.UI.Xaml.Media.Imaging.BitmapImage(new Uri("ms-appx:///" + Illustration.TrimStart('/'))) { DecodePixelWidth = 300 };
    }
}
