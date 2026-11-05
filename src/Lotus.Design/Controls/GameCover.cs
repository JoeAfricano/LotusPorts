using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.Foundation;
using Windows.UI;

namespace Lotus.Design.Controls;

/// <summary>
/// Generated typographic cover: title, original platform and the platform's signature color over the Lotus gradient.
/// Used instead of official box art, which is never embedded.
/// </summary>
public sealed partial class GameCover : UserControl
{
    public static readonly DependencyProperty TitleProperty = Register(nameof(Title), "");
    public static readonly DependencyProperty PlatformLabelProperty = Register(nameof(PlatformLabel), "");
    public static readonly DependencyProperty PlatformKeyProperty = Register(nameof(PlatformKey), "PC");
    public static readonly DependencyProperty TitleBottomInsetProperty =
        DependencyProperty.Register(nameof(TitleBottomInset), typeof(double), typeof(GameCover), new PropertyMetadata(0.0, (d, _) => ((GameCover)d).Update()));
    public static readonly DependencyProperty TitleSizeProperty =
        DependencyProperty.Register(nameof(TitleSize), typeof(double), typeof(GameCover), new PropertyMetadata(18.0, (d, _) => ((GameCover)d).Update()));

    private readonly Border background = new() { CornerRadius = new CornerRadius(12) };
    private readonly Border glow = new() { CornerRadius = new CornerRadius(12) };
    private readonly TextBlock platform = new() { FontSize = 11, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, CharacterSpacing = 80 };
    private readonly TextBlock title = new() { TextWrapping = TextWrapping.WrapWholeWords, MaxLines = 4, TextTrimming = TextTrimming.CharacterEllipsis, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold };
    private readonly Border titleHolder = new() { VerticalAlignment = VerticalAlignment.Bottom };
    // LotusPetal sets its own opacity, so the watermark strength lives on a wrapper.
    private readonly Border mark = new()
    {
        Opacity = 0.22,
        HorizontalAlignment = HorizontalAlignment.Right,
        VerticalAlignment = VerticalAlignment.Top,
        Margin = new Thickness(0, 18, 4, 0),
        Child = new LotusPetal { IsLit = false, Width = 120, Height = 104 },
    };

    public GameCover()
    {
        IsTabStop = false;
        var content = new Grid { Padding = new Thickness(14, 12, 14, 14) };
        content.Children.Add(new Border
        {
            Child = platform,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(7, 2, 7, 3),
            Background = new SolidColorBrush(Color.FromArgb(0x66, 0x0D, 0x0A, 0x0F)),
        });
        content.Children.Add(titleHolder);
        Content = new Grid { Children = { background, glow, mark, content } };
        Loaded += (_, _) => Update();
    }

    public string Title { get => (string)GetValue(TitleProperty); set => SetValue(TitleProperty, value); }
    public string PlatformLabel { get => (string)GetValue(PlatformLabelProperty); set => SetValue(PlatformLabelProperty, value); }

    /// <summary>Catalog platform id (N64, PS1, XBOX_360...). Picks the color token Platform.{key}.</summary>
    public string PlatformKey { get => (string)GetValue(PlatformKeyProperty); set => SetValue(PlatformKeyProperty, value); }

    public double TitleSize { get => (double)GetValue(TitleSizeProperty); set => SetValue(TitleSizeProperty, value); }

    /// <summary>Space kept under the title for something drawn over the cover (a status badge).</summary>
    public double TitleBottomInset { get => (double)GetValue(TitleBottomInsetProperty); set => SetValue(TitleBottomInsetProperty, value); }

    private static DependencyProperty Register(string name, string fallback) =>
        DependencyProperty.Register(name, typeof(string), typeof(GameCover), new PropertyMetadata(fallback, (d, _) => ((GameCover)d).Update()));

    private void Update()
    {
        var resources = Application.Current.Resources;
        var accent = resources.TryGetValue("Color.Platform." + PlatformKey, out var c) && c is Color color ? color : (Color)resources["Color.Accent.Magenta"];
        var baseColor = (Color)resources["Color.Bg.Base"];
        background.Background = new LinearGradientBrush
        {
            StartPoint = new Point(0, 0),
            EndPoint = new Point(1, 1),
            GradientStops =
            {
                new GradientStop { Color = Blend(accent, baseColor, 0.55), Offset = 0 },
                new GradientStop { Color = Blend(accent, baseColor, 0.82), Offset = 0.55 },
                new GradientStop { Color = baseColor, Offset = 1 },
            },
        };
        glow.Background = new LinearGradientBrush
        {
            StartPoint = new Point(0.5, 0.35),
            EndPoint = new Point(0.5, 1),
            GradientStops =
            {
                new GradientStop { Color = Color.FromArgb(0, 0x0D, 0x0A, 0x0F), Offset = 0 },
                new GradientStop { Color = Color.FromArgb(0xE6, 0x0D, 0x0A, 0x0F), Offset = 1 },
            },
        };
        glow.BorderBrush = new SolidColorBrush(Color.FromArgb(0x55, accent.R, accent.G, accent.B));
        glow.BorderThickness = new Thickness(1);
        platform.Text = PlatformLabel.ToUpperInvariant();
        platform.Foreground = new SolidColorBrush(Blend(accent, Microsoft.UI.Colors.White, 0.45));
        title.Text = Title;
        titleHolder.Child ??= title;
        titleHolder.Margin = new Thickness(0, 0, 0, TitleBottomInset);
        title.FontSize = TitleSize;
        title.Foreground = (Brush)resources["Text.Primary"];
        Microsoft.UI.Xaml.Automation.AutomationProperties.SetName(this, $"{Title}, {PlatformLabel}");
    }

    private static Color Blend(Color a, Color b, double amountOfB) => Color.FromArgb(255,
        (byte)(a.R + (b.R - a.R) * amountOfB),
        (byte)(a.G + (b.G - a.G) * amountOfB),
        (byte)(a.B + (b.B - a.B) * amountOfB));
}
