using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using Path = Microsoft.UI.Xaml.Shapes.Path;

namespace Lotus.Design.Controls;

/// <summary>Five-petal lotus mark: logo, Lotus connection indicator (lit/unlit) and empty states.</summary>
public sealed partial class LotusPetal : UserControl
{
    private static readonly string[] Petals =
    [
        "M2,22 C8,21 15,26 20,35 C11,36 4,31 2,22 Z",
        "M46,22 C40,21 33,26 28,35 C37,36 44,31 46,22 Z",
        "M10,12 C17,14 21,22 22,34 C13,31 9,22 10,12 Z",
        "M38,12 C31,14 27,22 26,34 C35,31 39,22 38,12 Z",
        "M24,3 C30,11 30,24 24,35 C18,24 18,11 24,3 Z",
        "M12,37 C19,39.5 29,39.5 36,37 C30,41 18,41 12,37 Z",
    ];

    public static readonly DependencyProperty IsLitProperty =
        DependencyProperty.Register(nameof(IsLit), typeof(bool), typeof(LotusPetal), new PropertyMetadata(true, (d, _) => ((LotusPetal)d).Paint()));

    private readonly List<Path> paths = [];

    public LotusPetal()
    {
        var canvas = new Canvas { Width = 48, Height = 42 };
        foreach (var data in Petals)
        {
            var path = new Path
            {
                Data = (Geometry)Microsoft.UI.Xaml.Markup.XamlBindingHelper.ConvertValue(typeof(Geometry), data),
                StrokeThickness = 1.2,
            };
            paths.Add(path);
            canvas.Children.Add(path);
        }
        Content = new Viewbox { Child = canvas, Stretch = Stretch.Uniform };
        IsTabStop = false;
        Loaded += (_, _) => Paint();
    }

    public bool IsLit
    {
        get => (bool)GetValue(IsLitProperty);
        set => SetValue(IsLitProperty, value);
    }

    private void Paint()
    {
        var resources = Application.Current.Resources;
        var fill = (Brush)resources[IsLit ? "Petal.Gradient" : "State.Unknown"];
        var stroke = (Brush)resources["Bg.Base"];
        foreach (var path in paths)
        {
            path.Fill = fill;
            path.Stroke = stroke;
        }
        Opacity = IsLit ? 1 : 0.55;
    }
}
