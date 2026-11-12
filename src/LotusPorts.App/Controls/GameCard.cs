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

    private readonly GameCover cover = new() { Height = 264, TitleBottomInset = 30, TitleSize = 17 };
    private readonly StateBadge status = new() { OverArt = true, Margin = new Thickness(10, 0, 10, 10), VerticalAlignment = VerticalAlignment.Bottom };
    private readonly StateBadge origin = new() { OverArt = true, Tone = BadgeTone.Unknown, Margin = new Thickness(12, 40, 0, 0), HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top };
    private readonly FontIcon heart = new() { Glyph = "\uEB52", FontSize = 14, Margin = new Thickness(0, 12, 12, 0), HorizontalAlignment = HorizontalAlignment.Right, VerticalAlignment = VerticalAlignment.Top };
    private readonly TextBlock name = new() { FontSize = 13, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, Margin = new Thickness(4, 9, 4, 0), TextTrimming = TextTrimming.CharacterEllipsis, MaxLines = 1 };
    private readonly TextBlock line = new() { FontSize = 12, TextTrimming = TextTrimming.CharacterEllipsis, VerticalAlignment = VerticalAlignment.Center };
    private readonly PlatformIcon platformIcon = new() { Size = 15, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 6, 0) };
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
            Content = new StackPanel { Padding = new Thickness(2), Children = { art, name, LineRow() } },
        };
        button.Click += (_, _) => { if (Model is not null) App.MainWindow?.Navigate(Pages.Game, Model.Id); };
        Lotus.Design.Motion.Attach(button);
        Content = button;
        // Live state (downloading, playing, update…): only while the card is on screen.
        Loaded += (_, _) => { Live.GameChanged += OnGameChanged; ShowLive(); };
        Unloaded += (_, _) => Live.GameChanged -= OnGameChanged;
    }

    /// <summary>Platform icon, then "Nintendo 64 · Pronto para jogar".</summary>
    private Grid LineRow()
    {
        var row = new Grid { Margin = new Thickness(4, 3, 4, 0) };
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        row.ColumnDefinitions.Add(new ColumnDefinition());
        row.Children.Add(platformIcon);
        Grid.SetColumn(line, 1);
        row.Children.Add(line);
        return row;
    }

    private static LiveState Live => Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<LiveState>(App.Services);

    private void OnGameChanged(string gameId)
    {
        if (Model is { } m && (gameId == m.Id || gameId == "*")) ShowLive();
    }

    private void ShowLive()
    {
        if (Model is not { } m) return;
        var live = Live.LineFor(m.Id);
        line.Text = $"{m.PlatformLabel} · {live.Text}";
        line.Foreground = (Brush)Application.Current.Resources[live.Tone switch
        {
            BadgeTone.Ok => "State.Ok",
            BadgeTone.Warning => "State.Warn",
            BadgeTone.Accent => "Accent.Rose",
            _ => "Text.Secondary",
        }];
        AutomationProperties.SetName(button, $"{m.Title}. {m.PlatformLabel}. {m.StatusText}. {live.Text}" + (m.OriginText is null ? "" : $". {m.OriginText}"));
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
        platformIcon.PlatformKey = m.PlatformKey;
        platformIcon.Foreground = PlatformIcon.SignatureBrush(m.PlatformKey) ?? (Brush)Application.Current.Resources["Text.Secondary"];
        // Only statuses that tell the player something get a badge on the cover; "not rated" stays on the game page.
        status.Text = m.StatusText;
        status.Tone = m.StatusTone;
        status.Visibility = m.ShowStatus ? Visibility.Visible : Visibility.Collapsed;
        cover.TitleBottomInset = m.ShowStatus ? 30 : 0;
        cover.CoverUrl = m.CoverUrl;
        origin.Text = m.OriginText ?? "";
        origin.Visibility = m.HasOrigin ? Visibility.Visible : Visibility.Collapsed;
        heart.Visibility = m.Favorite && !m.HasOrigin ? Visibility.Visible : Visibility.Collapsed;
        name.Text = m.Title;
        ShowLive();
    }
}
