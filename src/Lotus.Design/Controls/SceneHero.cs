using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Foundation;
using Windows.UI;

namespace Lotus.Design.Controls;

/// <summary>
/// Hero backdrop from an illustrated scene (premium art pack), with the reading gradient on the left
/// and a fade into the page at the bottom. Static: decoded once at the size shown, no animation.
/// </summary>
public sealed partial class SceneHero : UserControl
{
    public static readonly DependencyProperty SourceProperty =
        DependencyProperty.Register(nameof(Source), typeof(string), typeof(SceneHero), new PropertyMetadata("", (d, _) => ((SceneHero)d).Load()));

    private readonly Image image = new() { Stretch = Stretch.UniformToFill, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };

    public SceneHero()
    {
        IsTabStop = false;
        IsHitTestVisible = false;
        var baseColor = Color.FromArgb(0xFF, 0x0D, 0x0A, 0x0F);
        Content = new Grid
        {
            Children =
            {
                image,
                // Reading gradient: text sits on the left.
                new Border { Background = Gradient(new Point(0, 0.5), new Point(1, 0.5), (WithAlpha(baseColor, 0xEE), 0), (WithAlpha(baseColor, 0x99), 0.42), (WithAlpha(baseColor, 0x00), 0.78)) },
                // Fade into the page.
                new Border { Background = Gradient(new Point(0.5, 0), new Point(0.5, 1), (WithAlpha(baseColor, 0x00), 0.55), (baseColor, 1)) },
            },
        };
    }

    /// <summary>App-relative path, for example Assets/Art/Scenes/04_FUNDO_SANTUARIO_LUNAR_HERO_WIDE_1920x650.png.</summary>
    public string Source { get => (string)GetValue(SourceProperty); set => SetValue(SourceProperty, value); }

    private void Load()
    {
        if (string.IsNullOrEmpty(Source)) { image.Source = null; return; }
        // Decode at most at 1920 px wide: the hero never shows more than that.
        image.Source = new BitmapImage(new Uri("ms-appx:///" + Source.TrimStart('/'))) { DecodePixelWidth = 1920, DecodePixelType = DecodePixelType.Logical };
    }

    private static Color WithAlpha(Color c, byte a) => Color.FromArgb(a, c.R, c.G, c.B);

    private static LinearGradientBrush Gradient(Point start, Point end, params (Color Color, double Offset)[] stops)
    {
        var brush = new LinearGradientBrush { StartPoint = start, EndPoint = end };
        foreach (var (color, offset) in stops) brush.GradientStops.Add(new GradientStop { Color = color, Offset = offset });
        return brush;
    }
}
