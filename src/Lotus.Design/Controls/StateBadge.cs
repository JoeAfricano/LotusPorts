using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace Lotus.Design.Controls;

/// <summary>OK / AVISO / FALHA / NÃO VERIFICADO, plus neutral tones. Always text with the color, never color alone.</summary>
public enum BadgeTone
{
    Unknown,
    Ok,
    Warning,
    Failure,
    Accent,
}

public sealed partial class StateBadge : UserControl
{
    public static readonly DependencyProperty ToneProperty =
        DependencyProperty.Register(nameof(Tone), typeof(BadgeTone), typeof(StateBadge), new PropertyMetadata(BadgeTone.Unknown, (d, _) => ((StateBadge)d).Paint()));

    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(nameof(Text), typeof(string), typeof(StateBadge), new PropertyMetadata("", (d, _) => ((StateBadge)d).Paint()));

    private readonly Border border = new() { CornerRadius = new CornerRadius(8), Padding = new Thickness(8, 3, 10, 4), BorderThickness = new Thickness(0) };
    private readonly FontIcon icon = new() { FontSize = 11, VerticalAlignment = VerticalAlignment.Center };
    private readonly TextBlock label = new() { FontSize = 12, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, VerticalAlignment = VerticalAlignment.Center };

    public StateBadge()
    {
        border.Child = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 6, Children = { icon, label } };
        Content = border;
        HorizontalAlignment = HorizontalAlignment.Left;
        VerticalAlignment = VerticalAlignment.Center;
        IsTabStop = false;
        Loaded += (_, _) => Paint();
    }

    public BadgeTone Tone
    {
        get => (BadgeTone)GetValue(ToneProperty);
        set => SetValue(ToneProperty, value);
    }

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    private void Paint()
    {
        var (key, glyph) = Tone switch
        {
            BadgeTone.Ok => ("State.Ok", "\uE73E"),
            BadgeTone.Warning => ("State.Warn", "\uE7BA"),
            BadgeTone.Failure => ("State.Fail", "\uE711"),
            BadgeTone.Accent => ("Accent.Pink", "\uE946"),
            _ => ("State.Unknown", "\uE9CE"),
        };
        var resources = Application.Current.Resources;
        var brush = (Brush)resources[key];
        icon.Glyph = glyph;
        icon.Foreground = brush;
        label.Foreground = brush;
        label.Text = Text;
        border.BorderBrush = brush;
        border.Background = (Brush)resources[key + ".Subtle"];
        Microsoft.UI.Xaml.Automation.AutomationProperties.SetName(this, Text);
    }
}
