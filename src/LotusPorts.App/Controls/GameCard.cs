using Lotus.Design.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace LotusPorts.App.Controls;

/// <summary>Game cover card (Biblioteca and Catálogo). Opens the game page.</summary>
public sealed partial class GameCard : UserControl
{
    public static readonly DependencyProperty ModelProperty =
        DependencyProperty.Register(nameof(Model), typeof(GameCardModel), typeof(GameCard), new PropertyMetadata(null, (d, _) => ((GameCard)d).Update()));

    private readonly GameCover cover = new() { Height = 236, TitleBottomInset = 30, TitleSize = 17 };
    private readonly StateBadge status = new() { Margin = new Thickness(10, 0, 10, 10), VerticalAlignment = VerticalAlignment.Bottom };
    private readonly StateBadge origin = new() { Tone = BadgeTone.Unknown, Margin = new Thickness(12, 40, 0, 0), HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top };
    private readonly FontIcon heart = new() { Glyph = "\uEB52", FontSize = 14, Margin = new Thickness(0, 12, 12, 0), HorizontalAlignment = HorizontalAlignment.Right, VerticalAlignment = VerticalAlignment.Top };
    private readonly TextBlock line = new() { FontSize = 12, Margin = new Thickness(4, 8, 4, 0), TextTrimming = TextTrimming.CharacterEllipsis };
    private readonly Button button;

    public GameCard()
    {
        var resources = Application.Current.Resources;
        heart.Foreground = (Brush)resources["Accent.Pink"];
        line.Foreground = (Brush)resources["Text.Secondary"];
        var art = new Grid { Children = { cover, status, origin, heart } };
        button = new Button
        {
            Style = (Style)resources["Lotus.CardButton"],
            Width = 180,
            Content = new StackPanel { Padding = new Thickness(2), Children = { art, line } },
        };
        button.Click += (_, _) => { if (Model is not null) App.MainWindow?.Navigate(Pages.Game, Model.Id); };
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
        cover.PlatformLabel = m.PlatformLabel;
        status.Text = m.StatusText;
        status.Tone = m.StatusTone;
        origin.Text = m.OriginText ?? "";
        origin.Visibility = m.HasOrigin ? Visibility.Visible : Visibility.Collapsed;
        heart.Visibility = m.Favorite && !m.HasOrigin ? Visibility.Visible : Visibility.Collapsed;
        line.Text = $"{m.PlatformLabel} · {m.StateLine}";
        AutomationProperties.SetName(button, $"{m.Title}. {m.PlatformLabel}. {m.StatusText}. {m.StateLine}" + (m.OriginText is null ? "" : $". {m.OriginText}"));
    }
}
