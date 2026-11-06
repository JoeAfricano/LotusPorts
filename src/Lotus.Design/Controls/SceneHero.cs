using Lotus.Design.Themes;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Foundation;
using Windows.UI;

namespace Lotus.Design.Controls;

/// <summary>
/// A scene or banner from the art package behind a page header, with a reading gradient on the left and a fade into the
/// page. Both gradients use the current theme's page background, so the art blends into any theme (dark, light, custom).
/// </summary>
public sealed partial class SceneHero : UserControl
{
    public static readonly DependencyProperty SourceProperty =
        DependencyProperty.Register(nameof(Source), typeof(string), typeof(SceneHero), new PropertyMetadata("", (d, _) => ((SceneHero)d).Load()));

    private readonly Image image = new() { Stretch = Stretch.UniformToFill, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
    private readonly LinearGradientBrush reading;
    private readonly LinearGradientBrush fade;

    public SceneHero()
    {
        IsTabStop = false;
        IsHitTestVisible = false;
        reading = Gradient(new Point(0, 0.5), new Point(1, 0.5), (0xEE, 0), (0x99, 0.42), (0x00, 0.78));
        fade = Gradient(new Point(0.5, 0), new Point(0.5, 1), (0x00, 0.55), (0xFF, 1));
        Content = new Grid
        {
            Children =
            {
                image,
                new Border { Background = reading }, // text sits on the left
                new Border { Background = fade },    // the art fades into the page
            },
        };
        Recolor(ThemePalette.BaseColor);
        Loaded += (_, _) => ThemePalette.BaseColorChanged += Recolor;
        Unloaded += (_, _) => ThemePalette.BaseColorChanged -= Recolor;
    }

    /// <summary>App-relative path, for example Assets/Art/Banners/lunar_torii.png.</summary>
    public string Source { get => (string)GetValue(SourceProperty); set => SetValue(SourceProperty, value); }

    private void Load()
    {
        if (string.IsNullOrEmpty(Source)) { image.Source = null; return; }
        // Decode at most at 1920 px wide: the hero never shows more than that.
        image.Source = new BitmapImage(new Uri("ms-appx:///" + Source.TrimStart('/'))) { DecodePixelWidth = 1920, DecodePixelType = DecodePixelType.Logical };
    }

    private void Recolor(Color baseColor)
    {
        foreach (var brush in new[] { reading, fade })
            foreach (var stop in brush.GradientStops)
                stop.Color = Color.FromArgb(stop.Color.A, baseColor.R, baseColor.G, baseColor.B);
    }

    private static LinearGradientBrush Gradient(Point start, Point end, params (byte Alpha, double Offset)[] stops)
    {
        var brush = new LinearGradientBrush { StartPoint = start, EndPoint = end };
        foreach (var (alpha, offset) in stops) brush.GradientStops.Add(new GradientStop { Color = Color.FromArgb(alpha, 0, 0, 0), Offset = offset });
        return brush;
    }
}
