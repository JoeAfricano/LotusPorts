using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace Lotus.Design.Controls;

/// <summary>
/// Section title as in the premium mockups: icon, title, a short line of context and "Ver todos" on the right.
/// </summary>
public sealed partial class SectionHeader : UserControl
{
    public static readonly DependencyProperty GlyphProperty = Register(nameof(Glyph));
    public static readonly DependencyProperty TitleProperty = Register(nameof(Title));
    public static readonly DependencyProperty SubtitleProperty = Register(nameof(Subtitle));
    public static readonly DependencyProperty ActionTextProperty = Register(nameof(ActionText));

    private readonly FontIcon icon = new() { FontSize = 18, VerticalAlignment = VerticalAlignment.Center };
    private readonly TextBlock title = new() { FontSize = 20, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, VerticalAlignment = VerticalAlignment.Center };
    private readonly TextBlock subtitle = new() { FontSize = 13, VerticalAlignment = VerticalAlignment.Center, TextTrimming = TextTrimming.CharacterEllipsis };
    private readonly HyperlinkButton action = new() { Padding = new Thickness(8, 4, 4, 4), VerticalAlignment = VerticalAlignment.Center };

    public SectionHeader()
    {
        var resources = Application.Current.Resources;
        icon.Foreground = (Brush)resources["Accent.Pink"];
        title.Foreground = (Brush)resources["Text.Primary"];
        subtitle.Foreground = (Brush)resources["Text.Secondary"];
        action.Foreground = (Brush)resources["Accent.Rose"];
        action.Click += (s, e) => ActionClick?.Invoke(this, e);

        var left = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 12, Children = { icon, title, subtitle } };
        var grid = new Grid { ColumnSpacing = 12 };
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        grid.Children.Add(left);
        Grid.SetColumn(action, 1);
        grid.Children.Add(action);
        Content = grid;
        Loaded += (_, _) => Update();
    }

    /// <summary>"Ver todos" was pressed.</summary>
    public event RoutedEventHandler? ActionClick;

    public string Glyph { get => (string)GetValue(GlyphProperty); set => SetValue(GlyphProperty, value); }
    public string Title { get => (string)GetValue(TitleProperty); set => SetValue(TitleProperty, value); }
    public string Subtitle { get => (string)GetValue(SubtitleProperty); set => SetValue(SubtitleProperty, value); }

    /// <summary>Text of the link on the right; empty hides it.</summary>
    public string ActionText { get => (string)GetValue(ActionTextProperty); set => SetValue(ActionTextProperty, value); }

    private static DependencyProperty Register(string name) =>
        DependencyProperty.Register(name, typeof(string), typeof(SectionHeader), new PropertyMetadata("", (d, _) => ((SectionHeader)d).Update()));

    private void Update()
    {
        icon.Glyph = Glyph;
        icon.Visibility = string.IsNullOrEmpty(Glyph) ? Visibility.Collapsed : Visibility.Visible;
        title.Text = Title;
        subtitle.Text = Subtitle;
        subtitle.Visibility = string.IsNullOrEmpty(Subtitle) ? Visibility.Collapsed : Visibility.Visible;
        action.Content = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 6,
            Children = { new TextBlock { Text = ActionText, FontSize = 13 }, new FontIcon { Glyph = "", FontSize = 11 } },
        };
        action.Visibility = string.IsNullOrEmpty(ActionText) ? Visibility.Collapsed : Visibility.Visible;
        Microsoft.UI.Xaml.Automation.AutomationProperties.SetName(action, $"{ActionText}: {Title}");
    }
}
