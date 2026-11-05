using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using Path = Microsoft.UI.Xaml.Shapes.Path;
using Windows.Foundation;
using Windows.UI;

namespace Lotus.Design.Controls;

/// <summary>
/// Night scene behind the hero (moon, Fuji, torii, cherry blossoms). Pure vector and static:
/// drawn once, no timers or animation, so it costs nothing while the app is open.
/// </summary>
public sealed partial class SakuraHero : UserControl
{
    public SakuraHero()
    {
        IsTabStop = false;
        var root = new Grid { IsHitTestVisible = false };
        root.Children.Add(new Rectangle { Fill = Vertical(Hex("#FF3B1238"), Hex("#FF1E0B1E"), Hex("#FF0D0A0F")) });
        root.Children.Add(Moon());
        root.Children.Add(Landscape());
        root.Children.Add(Blossoms());
        // Fades: left for text contrast, bottom to melt into the page background.
        root.Children.Add(new Rectangle { Fill = Horizontal(Hex("#E60D0A0F"), Hex("#660D0A0F"), Hex("#000D0A0F")) });
        root.Children.Add(new Rectangle { Fill = Vertical(Hex("#000D0A0F"), Hex("#000D0A0F"), Hex("#FF0D0A0F")) });
        Content = root;
    }

    private static Grid Moon()
    {
        var moon = new Grid { Width = 300, Height = 300, HorizontalAlignment = HorizontalAlignment.Right, VerticalAlignment = VerticalAlignment.Top, Margin = new Thickness(0, -40, 230, 0) };
        moon.Children.Add(new Ellipse { Fill = Radial(Hex("#66F06FA5"), Hex("#00F06FA5")) });
        moon.Children.Add(new Ellipse { Width = 112, Height = 112, Fill = Radial(Hex("#FFFFD3E4"), Hex("#FFF07AAE")) });
        return moon;
    }

    private static Viewbox Landscape()
    {
        var canvas = new Canvas { Width = 1000, Height = 150 };
        canvas.Children.Add(Shape("M0,150 L0,96 C120,78 220,92 330,70 L520,14 C540,7 560,7 580,14 L760,72 C850,86 930,78 1000,88 L1000,150 Z", Hex("#FF4A1B4A")));
        canvas.Children.Add(Shape("M520,14 C540,7 560,7 580,14 L612,30 L592,26 L576,35 L560,26 L544,35 L528,26 L506,30 Z", Hex("#E6F5CFE0")));
        canvas.Children.Add(Shape("M0,150 L0,120 C200,100 400,126 600,110 C780,97 900,114 1000,104 L1000,150 Z", Hex("#FF1C0C1D")));
        // Torii on the right hill.
        canvas.Children.Add(Shape("M812,74 L900,74 L896,80 L816,80 Z M820,86 L892,86 L892,90 L820,90 Z M828,80 L834,80 L834,112 L828,112 Z M878,80 L884,80 L884,110 L878,110 Z", Hex("#FF0F070F")));
        return new Viewbox { Child = canvas, Stretch = Stretch.Fill, Height = 150, VerticalAlignment = VerticalAlignment.Bottom, Margin = new Thickness(0, 0, 0, 40) };
    }

    private static Canvas Blossoms()
    {
        var canvas = new Canvas { Width = 420, Height = 190, HorizontalAlignment = HorizontalAlignment.Right, VerticalAlignment = VerticalAlignment.Top, Margin = new Thickness(0, 44, 0, 0) };
        canvas.Children.Add(new Path
        {
            Data = Geometry("M420,10 C360,30 300,24 250,52 C210,74 180,70 140,96 M300,30 C290,60 270,80 240,96 M360,22 C350,48 340,70 330,90"),
            Stroke = new SolidColorBrush(Hex("#FF2A1322")),
            StrokeThickness = 4,
        });
        // Fixed seed: the same blossoms every time.
        var random = new Random(17);
        Color[] tones = [Hex("#FFF7A1C4"), Hex("#FFE879A9"), Hex("#FFFFC4DC"), Hex("#FFD65A93")];
        foreach (var (x, y) in new[] { (420.0, 10.0), (250, 52), (140, 96), (300, 30), (240, 96), (360, 22), (330, 90) })
        {
            for (var i = 0; i < 9; i++)
            {
                var size = 7 + random.NextDouble() * 9;
                var dot = new Ellipse
                {
                    Width = size,
                    Height = size,
                    Fill = new SolidColorBrush(tones[random.Next(tones.Length)]),
                    Opacity = 0.55 + random.NextDouble() * 0.45,
                };
                Canvas.SetLeft(dot, x - 22 + random.NextDouble() * 44);
                Canvas.SetTop(dot, y - 16 + random.NextDouble() * 32);
                canvas.Children.Add(dot);
            }
        }
        return canvas;
    }

    private static Path Shape(string data, Color color) => new() { Data = Geometry(data), Fill = new SolidColorBrush(color) };

    private static Geometry Geometry(string data) => (Geometry)XamlBindingHelper.ConvertValue(typeof(Geometry), data);

    private static Color Hex(string hex) => (Color)XamlBindingHelper.ConvertValue(typeof(Color), hex);

    private static LinearGradientBrush Vertical(Color top, Color middle, Color bottom) => new()
    {
        StartPoint = new Point(0.5, 0),
        EndPoint = new Point(0.5, 1),
        GradientStops = { new GradientStop { Color = top, Offset = 0 }, new GradientStop { Color = middle, Offset = 0.6 }, new GradientStop { Color = bottom, Offset = 1 } },
    };

    private static LinearGradientBrush Horizontal(Color left, Color middle, Color right) => new()
    {
        StartPoint = new Point(0, 0.5),
        EndPoint = new Point(1, 0.5),
        GradientStops = { new GradientStop { Color = left, Offset = 0 }, new GradientStop { Color = middle, Offset = 0.45 }, new GradientStop { Color = right, Offset = 0.75 } },
    };

    private static RadialGradientBrush Radial(Color center, Color edge) => new()
    {
        GradientStops = { new GradientStop { Color = center, Offset = 0 }, new GradientStop { Color = edge, Offset = 1 } },
    };
}
