using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Windows.UI;
using Windows.UI.ViewManagement;

namespace Lotus.Design.Themes;

/// <summary>
/// The family's theme system, the same as Lotus: modes Midnight (dark), Light ("rosé porcelana"), Graphite and System
/// (follows Windows), seven accents and custom themes from a Lotus share code. Shared brushes are recolored in place,
/// so every screen changes live. High contrast wins over everything and uses the system colors.
/// </summary>
public sealed class ThemePalette
{
    public static readonly string[] Modes = ["Midnight", "Light", "Graphite", "System"];
    public static readonly string[] Accents = ["rose", "violet", "blue", "green", "crimson", "gold", "dark"];

    private readonly ResourceDictionary resources;
    private readonly UISettings windows = new();
    private string mode = "Midnight", accent = "rose";
    private ThemeSpec? custom;

    public ThemePalette(ResourceDictionary resources)
    {
        this.resources = resources;
        windows.ColorValuesChanged += (_, _) => Dispatcher?.TryEnqueue(Refresh);
    }

    public Microsoft.UI.Dispatching.DispatcherQueue? Dispatcher { get; set; }

    /// <summary>Light or Dark for the root element, so WinUI controls (toggles, menus, dialogs) match.</summary>
    public ElementTheme Element { get; private set; } = ElementTheme.Dark;

    public event EventHandler? Changed;

    /// <summary>The page background now in use; art that fades into the page (banners) follows it.</summary>
    public static Color BaseColor { get; private set; } = Rgb(0x0D, 0x0A, 0x0F);

    /// <summary>Raised after every theme change with the new page background.</summary>
    public static event Action<Color>? BaseColorChanged;

    /// <summary>Applies a mode ("Midnight", "Light", "Graphite", "System" or "Custom" with a theme) and an accent.</summary>
    public void Apply(string modeName, string accentName, ThemeSpec? customTheme = null)
    {
        mode = modeName;
        accent = Accents.Contains(accentName) ? accentName : "rose";
        custom = modeName == "Custom" ? customTheme : null;
        Refresh();
    }

    private bool SystemIsLight()
    {
        var bg = windows.GetColorValue(UIColorType.Background);
        return bg.R + bg.G + bg.B > 382;
    }

    private void Refresh()
    {
        var highContrast = new AccessibilitySettings().HighContrast;
        if (highContrast)
        {
            Element = ElementTheme.Dark;
            ApplyHighContrast();
        }
        else if (custom is not null)
        {
            var k = ThemeEngine.Derive(custom);
            Element = k.Light ? ElementTheme.Light : ElementTheme.Dark;
            static Color C(ThemeColor c) => Rgb(c.R, c.G, c.B);
            ApplySet(new Tokens(C(k.Canvas), C(k.Shell), C(k.Card), C(k.Raised), C(k.Border), C(k.Text), C(k.TextSecondary)),
                k.Light, C(k.AccentFill), C(k.AccentText), C(k.Success), C(k.Warning), C(k.Error), (byte)Math.Round(k.CardOpacity * 255));
        }
        else
        {
            var light = mode == "Light" || (mode == "System" && SystemIsLight());
            Element = light ? ElementTheme.Light : ElementTheme.Dark;
            var tokens = light ? Tokens.Light : mode == "Graphite" ? Tokens.Graphite : Tokens.Dark;
            var (fill, textDark, textLight) = AccentColors(accent);
            ApplySet(tokens, light, fill, light ? textLight : textDark,
                light ? Rgb(0x24, 0x6C, 0x4D) : Rgb(0x5F, 0xD3, 0x8F), light ? Rgb(0x77, 0x50, 0x1B) : Rgb(0xF2, 0xC4, 0x6D), light ? Rgb(0xB3, 0x22, 0x42) : Rgb(0xFF, 0x6B, 0x6B), 255);
        }
        Changed?.Invoke(this, EventArgs.Empty);
    }

    private void ApplySet(Tokens t, bool light, Color fill, Color accentText, Color ok, Color warn, Color fail, byte cardAlpha)
    {
        Set("Bg.Base", "Color.Bg.Base", t.Canvas);
        BaseColor = t.Canvas;
        BaseColorChanged?.Invoke(t.Canvas);
        Set("Bg.Sidebar", "Color.Bg.Sidebar", t.Shell);
        Set("Bg.Surface", "Color.Bg.Surface", WithAlpha(t.Card, cardAlpha));
        Set("Bg.Elevated", "Color.Bg.Elevated", t.Raised);
        Set("Stroke.Subtle", "Color.Stroke.Subtle", t.Border);
        Set("Text.Primary", "Color.Text.Primary", t.Text);
        Set("Text.Secondary", "Color.Text.Secondary", t.TextSecondary);
        Set("Accent.Rose", "Color.Accent.Rose", accentText);
        Set("Accent.Pink", "Color.Accent.Pink", fill);
        Set("Accent.Magenta", "Color.Accent.Magenta", Darken(fill, 0.8));
        Set("State.Ok", "Color.State.Ok", ok);
        Set("State.Warn", "Color.State.Warn", warn);
        Set("State.Fail", "Color.State.Fail", fail);

        // Text on the accent fill: white, unless the fill is light (custom themes), then a dark tone that stays readable.
        var onFill = Luminance(fill) > 0.55 ? Rgb(0x16, 0x12, 0x18) : Rgb(255, 255, 255);
        Set("Accent.Gradient", null, fill);
        Set("Accent.Pink.Subtle", null, fill);
        Set("Accent.OnFill", null, onFill);
        Set("AccentButtonForeground", null, onFill);
        Set("AccentButtonForegroundPointerOver", null, onFill);
        Set("AccentButtonForegroundPressed", null, onFill);

        // Buttons, toggles, progress and text boxes (WinUI's own brushes).
        var hover = Lighten(fill, 0.12);
        var pressed = Darken(fill, 0.82);
        Set("AccentButtonBackground", null, fill);
        Set("AccentButtonBackgroundPointerOver", null, hover);
        Set("AccentButtonBackgroundPressed", null, pressed);
        Set("AccentFillColorDefaultBrush", null, fill);
        Set("AccentFillColorSecondaryBrush", null, hover);
        Set("AccentFillColorTertiaryBrush", null, pressed);
        Set("TextControlBackground", null, WithAlpha(t.Card, cardAlpha));
        Set("TextControlBackgroundPointerOver", null, t.Raised);
        Set("TextControlBorderBrush", null, t.Border);
        Set("TextControlBorderBrushPointerOver", null, t.Border);
        resources["SystemAccentColor"] = fill;
        resources["SystemAccentColorLight1"] = hover;
        resources["SystemAccentColorLight2"] = accentText;
        resources["SystemAccentColorDark1"] = pressed;
        _ = light;
    }

    private void ApplyHighContrast()
    {
        var bg = windows.GetColorValue(UIColorType.Background);
        var fg = windows.GetColorValue(UIColorType.Foreground);
        var accentColor = windows.GetColorValue(UIColorType.Accent);
        foreach (var (brush, color) in new[] { ("Bg.Base", "Color.Bg.Base"), ("Bg.Sidebar", "Color.Bg.Sidebar"), ("Bg.Surface", "Color.Bg.Surface"), ("Bg.Elevated", "Color.Bg.Elevated") })
            Set(brush, color, bg);
        foreach (var (brush, color) in new[] { ("Stroke.Subtle", "Color.Stroke.Subtle"), ("Text.Primary", "Color.Text.Primary"), ("Text.Secondary", "Color.Text.Secondary"), ("Accent.Rose", "Color.Accent.Rose"),
            ("State.Ok", "Color.State.Ok"), ("State.Warn", "Color.State.Warn"), ("State.Fail", "Color.State.Fail") })
            Set(brush, color, fg);
        Set("Accent.Pink", "Color.Accent.Pink", accentColor);
        Set("Accent.Magenta", "Color.Accent.Magenta", accentColor);
        Set("AccentButtonBackground", null, accentColor);
    }

    /// <summary>Accent: the fill (white text on it), the text color on dark surfaces and on light ones. Same values as Lotus.</summary>
    public static (Color Fill, Color TextDark, Color TextLight) AccentColors(string id) => id switch
    {
        "violet" => (Rgb(0x6D, 0x43, 0xB5), Rgb(0xC2, 0xA5, 0xF5), Rgb(0x65, 0x40, 0xA0)),
        "blue" => (Rgb(0x24, 0x5C, 0xC1), Rgb(0x9C, 0xBD, 0xFF), Rgb(0x24, 0x5C, 0xC1)),
        "green" => (Rgb(0x28, 0x71, 0x5A), Rgb(0x8A, 0xD6, 0xB3), Rgb(0x24, 0x6C, 0x4D)),
        "crimson" => (Rgb(0xB0, 0x22, 0x36), Rgb(0xF0, 0x6A, 0x7A), Rgb(0x9E, 0x1E, 0x30)),
        "gold" => (Rgb(0x8F, 0x5E, 0x16), Rgb(0xEE, 0xC0, 0x7A), Rgb(0x7A, 0x4F, 0x12)),
        "dark" => (Rgb(0x3E, 0x32, 0x39), Rgb(0xF2, 0x7B, 0xA8), Rgb(0x8E, 0x24, 0x52)),
        // Lotus Ports' rose is the brighter pink of its concept art; the text tones are Lotus's.
        _ => (Rgb(0xE0, 0x4C, 0x84), Rgb(0xF5, 0x8B, 0xB7), Rgb(0xB0, 0x19, 0x5A)),
    };

    private sealed record Tokens(Color Canvas, Color Shell, Color Card, Color Raised, Color Border, Color Text, Color TextSecondary)
    {
        // Lotus Ports' original dark plum (concept art), the same family as Lotus's Midnight.
        public static readonly Tokens Dark = new(Rgb(0x0D, 0x0A, 0x0F), Rgb(0x11, 0x0C, 0x13), Rgb(0x17, 0x12, 0x19), Rgb(0x22, 0x1A, 0x26),
            Rgb(0x33, 0x26, 0x3A), Rgb(0xF6, 0xF0, 0xF4), Rgb(0xB3, 0xA6, 0xB4));

        // Lotus's "rosé porcelana".
        public static readonly Tokens Light = new(Rgb(0xF7, 0xEC, 0xF1), Rgb(0xF2, 0xE0, 0xE9), Rgb(0xFF, 0xFA, 0xFC), Rgb(0xF5, 0xDC, 0xE7),
            Rgb(0xE3, 0xBC, 0xCD), Rgb(0x2B, 0x10, 0x20), Rgb(0x6A, 0x3E, 0x55));

        public static readonly Tokens Graphite = new(Rgb(0x13, 0x13, 0x16), Rgb(0x19, 0x19, 0x1D), Rgb(0x22, 0x22, 0x28), Rgb(0x2D, 0x2D, 0x34),
            Rgb(0x38, 0x38, 0x40), Rgb(0xF5, 0xF5, 0xF7), Rgb(0xBC, 0xBC, 0xC6));
    }

    private static Color Rgb(byte r, byte g, byte b) => Color.FromArgb(255, r, g, b);

    private static double Luminance(Color c) => (0.2126 * c.R + 0.7152 * c.G + 0.0722 * c.B) / 255.0;

    private static Color WithAlpha(Color c, byte a) => Color.FromArgb(a, c.R, c.G, c.B);

    private static Color Darken(Color c, double f) => Rgb((byte)(c.R * f), (byte)(c.G * f), (byte)(c.B * f));

    private static Color Lighten(Color c, double f) => Rgb((byte)(c.R + (255 - c.R) * f), (byte)(c.G + (255 - c.G) * f), (byte)(c.B + (255 - c.B) * f));

    /// <summary>Recolors a shared brush in place (live everywhere) and updates its Color token for code that reads colors.</summary>
    private void Set(string brushKey, string? colorKey, Color color)
    {
        foreach (var dictionary in Dictionaries())
        {
            if (!dictionary.TryGetValue(brushKey, out var value)) continue;
            if (value is SolidColorBrush brush) brush.Color = color;
            else if (value is LinearGradientBrush gradient && gradient.GradientStops.Count >= 2)
            {
                gradient.GradientStops[0].Color = Lighten(color, 0.12);
                gradient.GradientStops[^1].Color = color;
            }
        }
        if (colorKey is not null) resources[colorKey] = color;
    }

    /// <summary>The app dictionary, the merged ones and every theme dictionary (Dark and Light), so both stay in step.</summary>
    private IEnumerable<ResourceDictionary> Dictionaries()
    {
        IEnumerable<ResourceDictionary> Walk(ResourceDictionary d)
        {
            yield return d;
            foreach (var (_, themed) in d.ThemeDictionaries)
                if (themed is ResourceDictionary t) yield return t;
            foreach (var merged in d.MergedDictionaries)
                foreach (var inner in Walk(merged)) yield return inner;
        }
        return Walk(resources);
    }
}
