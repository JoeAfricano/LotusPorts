using Lotus.Design.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace LotusPorts.App.Controls;

/// <summary>
/// Compact game card (the mockups' list cards): small cover on the left, title, one line of context and the
/// status badge when it says something. Used for lists that should read as a tidy grid.
/// </summary>
public sealed partial class GameTile : UserControl
{
    public static readonly DependencyProperty ModelProperty =
        DependencyProperty.Register(nameof(Model), typeof(GameCardModel), typeof(GameTile), new PropertyMetadata(null, (d, _) => ((GameTile)d).Update()));

    private readonly GameCover cover = new() { Width = 64, Height = 96, TitleSize = 10 };
    private readonly TextBlock name = new() { FontSize = 14, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, TextTrimming = TextTrimming.CharacterEllipsis, MaxLines = 2, TextWrapping = TextWrapping.WrapWholeWords };
    private readonly TextBlock line = new() { FontSize = 12, TextTrimming = TextTrimming.CharacterEllipsis };
    private readonly StateBadge status = new();
    private readonly Button button;

    public GameTile()
    {
        var resources = Application.Current.Resources;
        line.Foreground = (Brush)resources["Text.Secondary"];
        var text = new StackPanel { Spacing = 4, VerticalAlignment = VerticalAlignment.Center, Children = { name, line, status } };
        var grid = new Grid { ColumnSpacing = 14, Padding = new Thickness(8) };
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        grid.Children.Add(cover);
        Grid.SetColumn(text, 1);
        grid.Children.Add(text);
        var chevron = new FontIcon { Glyph = "", FontSize = 12, Foreground = (Brush)resources["Text.Secondary"], VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 6, 0) };
        Grid.SetColumn(chevron, 2);
        grid.Children.Add(chevron);

        button = new Button
        {
            Style = (Style)resources["Lotus.CardButton"],
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Content = new Border
            {
                Background = (Brush)resources["Bg.Surface"],
                BorderBrush = (Brush)resources["Stroke.Subtle"],
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(12),
                Child = grid,
            },
        };
        button.Click += (_, _) => { if (Model is not null) App.MainWindow?.Navigate(Pages.Game, Model.Id); };
        Lotus.Design.Motion.Attach(button, lift: 3f, grow: 1.015f);
        Content = button;
    }

    public GameCardModel? Model
    {
        get => (GameCardModel?)GetValue(ModelProperty);
        set => SetValue(ModelProperty, value);
    }

    private void Update()
    {
        if (Model is not { } m) return;
        cover.Title = m.Title;
        cover.PlatformKey = m.PlatformKey;
        cover.PlatformLabel = "";
        cover.CoverUrl = m.CoverUrl;
        name.Text = m.Title;
        line.Text = $"{m.PlatformLabel} · {m.StateLine}";
        status.Text = m.OriginText ?? m.StatusText;
        status.Tone = m.OriginText is not null ? BadgeTone.Unknown : m.StatusTone;
        status.Visibility = m.ShowStatus || m.HasOrigin ? Visibility.Visible : Visibility.Collapsed;
        AutomationProperties.SetName(button, $"{m.Title}. {m.PlatformLabel}. {m.StateLine}");
    }
}
