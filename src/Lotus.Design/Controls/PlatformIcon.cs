using System.Globalization;
using System.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using Path = Microsoft.UI.Xaml.Shapes.Path;

namespace Lotus.Design.Controls;

/// <summary>
/// Icon of an original platform, drawn here as a simple silhouette of its controller or device (24×24 grid, holes cut
/// out with even-odd). Original drawings, never a brand logo. Keyed by the catalog name (N64, PS2, XBOX_360…);
/// unknown keys get a disc.
/// </summary>
public sealed partial class PlatformIcon : UserControl
{
    public static readonly DependencyProperty PlatformKeyProperty =
        DependencyProperty.Register(nameof(PlatformKey), typeof(string), typeof(PlatformIcon), new PropertyMetadata("", (d, _) => ((PlatformIcon)d).Draw()));

    public static readonly DependencyProperty SizeProperty =
        DependencyProperty.Register(nameof(Size), typeof(double), typeof(PlatformIcon), new PropertyMetadata(20.0, (d, _) => ((PlatformIcon)d).Resize()));

    private readonly Path path = new() { Width = 24, Height = 24, Stretch = Stretch.None };
    private readonly Viewbox box = new() { Stretch = Stretch.Uniform };

    public PlatformIcon()
    {
        IsTabStop = false;
        path.SetBinding(Shape.FillProperty, new Microsoft.UI.Xaml.Data.Binding { Source = this, Path = new PropertyPath(nameof(Foreground)) });
        box.Child = new Canvas { Width = 24, Height = 24, Children = { path } };
        Content = box;
        Resize();
        Draw();
    }

    public string PlatformKey { get => (string)GetValue(PlatformKeyProperty); set => SetValue(PlatformKeyProperty, value); }

    public double Size { get => (double)GetValue(SizeProperty); set => SetValue(SizeProperty, value); }

    /// <summary>The platform's signature color (Color.Platform.*), or null when the theme has none for it.</summary>
    public static Brush? SignatureBrush(string key) =>
        Application.Current.Resources.TryGetValue("Color.Platform." + key, out var value) && value is Windows.UI.Color color ? new SolidColorBrush(color) : null;

    private void Resize() => box.Width = box.Height = Size;

    private void Draw() =>
        path.Data = (Geometry)Microsoft.UI.Xaml.Markup.XamlBindingHelper.ConvertValue(typeof(Geometry), Data(PlatformKey));

    /// <summary>Path markup for a platform (exposed for tests).</summary>
    public static string Data(string? key) => "F0 " + (Family(key) switch
    {
        "nes" => "M2,7.5 H22 A1,1 0 0 1 23,8.5 V15.5 A1,1 0 0 1 22,16.5 H2 A1,1 0 0 1 1,15.5 V8.5 A1,1 0 0 1 2,7.5 Z"
            + Plus(6, 12, 4.4, 1.5) + Circle(16.3, 12.6, 1.3) + Circle(19.6, 12.6, 1.3) + Rect(10.3, 12.2, 1.4, 0.8) + Rect(12.4, 12.2, 1.4, 0.8),
        "snes" => "M7,6.5 H17 A5.5,5.5 0 0 1 17,17.5 H7 A5.5,5.5 0 0 1 7,6.5 Z"
            + Plus(6.6, 12, 4.2, 1.4) + Circle(17, 9.6, 1) + Circle(17, 14.4, 1) + Circle(14.6, 12, 1) + Circle(19.4, 12, 1),
        "n64" => "M3,6.5 Q3,4 6,4 H18 Q21,4 21,6.5 L20.6,13 Q20.2,16.2 18.6,16.2 Q17.1,16.2 16.8,13.6 L15.4,10.4 H14.2 L13.3,19 Q12,21.6 10.7,19 L9.8,10.4 H8.6 L7.2,13.6 Q6.9,16.2 5.4,16.2 Q3.8,16.2 3.4,13 Z"
            + Plus(6.4, 7.6, 3.4, 1.1) + Circle(12, 8.4, 1.4) + Circle(17.4, 6.9, 0.8) + Circle(18.6, 8.8, 0.8),
        "gamecube" => "M4,7 Q8,5 12,7 Q16,5 20,7 Q23.2,9 22.2,15 Q21.3,19.2 18.5,18.2 Q16.5,17.4 15.5,14.2 H8.5 Q7.5,17.4 5.5,18.2 Q2.7,19.2 1.8,15 Q0.8,9 4,7 Z"
            + Circle(6.6, 10.2, 1.7) + Circle(17.2, 10.4, 1.8) + Circle(14.4, 12.6, 0.8) + Circle(19.8, 8.2, 0.7) + Circle(14.6, 15.4, 0.9),
        // PS1: digital pad, no sticks (select/start in the middle).
        "pad-ps1" => Pad + Plus(6, 10.4, 4, 1.3) + Buttons(18, 10.4, 1.7, 0.85) + Rect(9.4, 10, 1.8, 0.8) + Rect(12.8, 10, 1.8, 0.8),
        "pad-ps2" => Pad + Plus(6, 10, 3.6, 1.2) + Buttons(18, 10, 1.6, 0.8) + Circle(9.2, 13.4, 1.4) + Circle(14.8, 13.4, 1.4),
        // PS3: the PS2 pad plus the round home button between the sticks.
        "pad-ps3" => Pad + Plus(6, 10, 3.6, 1.2) + Buttons(18, 10, 1.6, 0.8) + Circle(8.8, 13.4, 1.35) + Circle(15.2, 13.4, 1.35) + Circle(12, 12.6, 0.8),
        // PS4: touchpad in the middle.
        "pad-ps4" => Pad + Rect(9.2, 7.2, 5.6, 3.8) + Plus(5.4, 10, 3.4, 1.1) + Buttons(18.6, 10, 1.5, 0.75) + Circle(9.2, 13.4, 1.3) + Circle(14.8, 13.4, 1.3),
        // PS5: rounder body, wider touchpad.
        "pad-ps5" => "M6,5.4 H18 Q21.6,5.4 22.4,10 L23.2,16.4 Q23.6,19.8 21,19.8 Q19.2,19.8 17.9,16.8 Q15,15.8 12,15.8 Q9,15.8 6.1,16.8 Q4.8,19.8 3,19.8 Q0.4,19.8 0.8,16.4 L1.6,10 Q2.4,5.4 6,5.4 Z"
            + Rect(8.4, 6.6, 7.2, 4.2) + Plus(5, 10, 3.4, 1.1) + Buttons(19, 10, 1.5, 0.75) + Circle(9, 13.4, 1.3) + Circle(15, 13.4, 1.3),
        "pad-xbox" => Pad + Circle(6.4, 9.8, 1.6) + Plus(9.6, 13.6, 3.2, 1.1) + Circle(17.6, 8.4, 0.75) + Circle(17.6, 11.4, 0.75) + Circle(16.1, 9.9, 0.75)
            + Circle(19.1, 9.9, 0.75) + Circle(14.6, 13.6, 1.4) + Circle(12, 8.6, 1),
        "sega" => "M3,9 Q12,5 21,9 Q23.6,10.6 22.6,14 Q21.6,17.6 18,16.6 Q12,15 6,16.6 Q2.4,17.6 1.4,14 Q0.4,10.6 3,9 Z"
            + Circle(6, 12, 1.9) + Circle(14.6, 12.6, 0.95) + Circle(17.2, 11.8, 0.95) + Circle(19.8, 11.2, 0.95),
        "dreamcast" => "M5,4 H19 Q22,4 22.5,9 L23,17 Q23,21 20.5,21 Q18.5,21 17.5,17 H6.5 Q5.5,21 3.5,21 Q1,21 1,17 L1.5,9 Q2,4 5,4 Z"
            + Rect(9, 6, 6, 6.5) + Circle(5.6, 8.6, 1.3) + Plus(6, 14, 3.4, 1.1) + Circle(18.4, 8.6, 0.8) + Circle(18.4, 11.6, 0.8) + Circle(16.9, 10.1, 0.8) + Circle(19.9, 10.1, 0.8),
        "gameboy" => "M7,2 H17 Q18,2 18,3 V19 Q18,22 15,22 H7 Q6,22 6,21 V3 Q6,2 7,2 Z"
            + Rect(8, 4, 8, 7) + Plus(9.2, 15.4, 3.4, 1.1) + Circle(15.2, 14.6, 0.95) + Circle(13.4, 16.2, 0.95),
        "handheld-wide" => "M4.5,6 H19.5 Q23,6 23,12 Q23,18 19.5,18 H4.5 Q1,18 1,12 Q1,6 4.5,6 Z"
            + Rect(7, 8.2, 10, 7.6) + Plus(4.2, 12, 3, 1) + Circle(20.4, 11, 0.85) + Circle(19, 13, 0.85),
        "psp" => "M3,7 H21 Q23,7 23,12 Q23,17 21,17 H3 Q1,17 1,12 Q1,7 3,7 Z"
            + Rect(6.2, 8.4, 11.6, 7.2) + Plus(3.6, 11.4, 2.8, 0.9) + Circle(20.4, 10.6, 0.7) + Circle(20.4, 13.2, 0.7) + Circle(4, 14.6, 0.7),
        "ds" => "M4.5,1.5 H19.5 Q20.5,1.5 20.5,2.5 V10.5 H3.5 V2.5 Q3.5,1.5 4.5,1.5 Z M3.5,12.5 H20.5 V21.5 Q20.5,22.5 19.5,22.5 H4.5 Q3.5,22.5 3.5,21.5 Z"
            + Rect(6.5, 3, 11, 6) + Rect(8, 14, 8, 6.5) + Circle(5.4, 16, 0.7) + Circle(18.6, 16, 0.7),
        "switch" => "M5.5,5.5 H18.5 V18.5 H5.5 Z M5,5.5 H3.4 Q1,5.5 1,8.5 V15.5 Q1,18.5 3.4,18.5 H5 Z M19,5.5 H20.6 Q23,5.5 23,8.5 V15.5 Q23,18.5 20.6,18.5 H19 Z"
            + Rect(6.8, 6.8, 10.4, 10.4) + Circle(3, 9.4, 0.9) + Circle(21, 14.4, 0.9),
        // Switch 2: wider screen, squarer Joy-Cons, the extra C button on the right one.
        "switch2" => "M5.2,6.4 H18.8 V17.6 H5.2 Z M4.7,6 H2.2 Q1,6 1,7.4 V16.6 Q1,18 2.2,18 H4.7 Z M19.3,6 H21.8 Q23,6 23,7.4 V16.6 Q23,18 21.8,18 H19.3 Z"
            + Rect(6.3, 7.5, 11.4, 9) + Circle(2.85, 9.3, 0.95) + Circle(21.15, 14.2, 0.95) + Circle(21.15, 10.4, 0.55) + Plus(2.85, 14, 2.4, 0.8),
        "wii-remote" => "M10,1 H14 Q16,1 16,3 V21 Q16,23 14,23 H10 Q8,23 8,21 V3 Q8,1 10,1 Z"
            + Plus(12, 5.4, 3.6, 1.1) + Circle(12, 10.2, 1.3) + Circle(12, 16.4, 0.6) + Circle(12, 18.4, 0.6),
        "wii-u" => "M3,6 H21 Q23,6 23,9 V15 Q23,18 21,18 H3 Q1,18 1,15 V9 Q1,6 3,6 Z"
            + Rect(6.2, 7.6, 11.6, 8.8) + Circle(3.6, 9.4, 1) + Circle(20.4, 9.4, 1) + Plus(3.6, 14, 2.6, 0.8),
        "virtual-boy" => "M2.5,6 H21.5 Q23,6 23,8 V12.5 Q23,14.5 21,14.5 H15 L13.6,12.6 H10.4 L9,14.5 H3 Q1,14.5 1,12.5 V8 Q1,6 2.5,6 Z M11,14.5 H13 V21 H11 Z"
            + Rect(3, 8, 7, 3.6) + Rect(14, 8, 7, 3.6),
        "arcade" => Circle(8, 5.5, 3.2) + "M7.2,8.7 H8.8 V14 H7.2 Z M2,14 H22 V20 Q22,21.5 20.5,21.5 H3.5 Q2,21.5 2,20 Z"
            + Circle(14.6, 17.6, 1.3) + Circle(18.6, 17.6, 1.3),
        "pc" => "M2,3 H22 Q23,3 23,4 V16 Q23,17 22,17 H2 Q1,17 1,16 V4 Q1,3 2,3 Z M10,17 H14 L15,20 H9 Z M7,20 H17 V21.6 H7 Z"
            + Rect(2.6, 4.6, 18.8, 10.8),
        "mobile" => "M8,1 H16 Q18,1 18,3 V21 Q18,23 16,23 H8 Q6,23 6,21 V3 Q6,1 8,1 Z" + Rect(7.6, 3.4, 8.8, 15.4) + Circle(12, 20.8, 0.8),
        "floppy" => "M3,3 H18 L21,6 V21 H3 Z" + Rect(7, 4.4, 8, 5) + Rect(6, 12.6, 12, 7),
        _ => Circle(12, 12, 10) + Circle(12, 12, 2.6),
    });

    private const string Pad = "M6,6 H18 Q21,6 22,10 L23,16 Q23.5,19.2 21,19.2 Q19.5,19.2 18.5,16.6 L17.6,15.2 H6.4 L5.5,16.6 Q4.5,19.2 3,19.2 Q0.5,19.2 1,16 L2,10 Q3,6 6,6 Z";

    /// <summary>The drawing used for a platform: platforms of one family share a silhouette.</summary>
    public static string Family(string? key) => key switch
    {
        "NES" => "nes",
        "SNES" => "snes",
        "N64" => "n64",
        "GAMECUBE" => "gamecube",
        "WII" => "wii-remote",
        "WII_U" => "wii-u",
        "SWITCH" => "switch",
        "SWITCH_2" => "switch2",
        "GB" or "GBC" => "gameboy",
        "GBA" or "GAME_GEAR" => "handheld-wide",
        "NDS" or "N3DS" => "ds",
        "VIRTUAL_BOY" => "virtual-boy",
        "PS1" => "pad-ps1",
        "PS2" => "pad-ps2",
        "PS3" => "pad-ps3",
        "PS4" => "pad-ps4",
        "PS5" => "pad-ps5",
        "PSP" => "psp",
        "XBOX" or "XBOX_360" => "pad-xbox",
        "GENESIS" or "SEGA_32X" or "SATURN" => "sega",
        "DREAMCAST" => "dreamcast",
        "ARCADE" => "arcade",
        "PC" => "pc",
        "MOBILE" => "mobile",
        "AMIGA" => "floppy",
        _ => "disc",
    };

    private static string N(double v) => v.ToString("0.##", CultureInfo.InvariantCulture);

    private static string Circle(double cx, double cy, double r) =>
        $" M{N(cx - r)},{N(cy)} A{N(r)},{N(r)} 0 1 0 {N(cx + r)},{N(cy)} A{N(r)},{N(r)} 0 1 0 {N(cx - r)},{N(cy)} Z";

    /// <summary>Four face buttons in a diamond around (cx, cy).</summary>
    private static string Buttons(double cx, double cy, double spread, double r) =>
        Circle(cx, cy - spread, r) + Circle(cx, cy + spread, r) + Circle(cx - spread, cy, r) + Circle(cx + spread, cy, r);

    private static string Rect(double x, double y, double w, double h) => $" M{N(x)},{N(y)} H{N(x + w)} V{N(y + h)} H{N(x)} Z";

    /// <summary>A d-pad: a plus of the given span and arm thickness.</summary>
    private static string Plus(double cx, double cy, double span, double arm)
    {
        double s = span / 2, a = arm / 2;
        var b = new StringBuilder();
        b.Append($" M{N(cx - a)},{N(cy - s)} H{N(cx + a)} V{N(cy - a)} H{N(cx + s)} V{N(cy + a)} H{N(cx + a)} V{N(cy + s)}");
        b.Append($" H{N(cx - a)} V{N(cy + a)} H{N(cx - s)} V{N(cy - a)} H{N(cx - a)} Z");
        return b.ToString();
    }
}
