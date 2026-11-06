using Lotus.Design.Controls;
using LotusPorts.App.ViewModels;
using LotusPorts.Catalog;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace LotusPorts.App.Controls;

/// <summary>
/// Platforms as a row of icon chips (Catálogo and Biblioteca): "Todas" first, then each platform with its icon and how
/// many games it has. One is always selected. Scrolls sideways with the arrows, the wheel or touch.
/// </summary>
public sealed partial class PlatformRail : UserControl
{
    private readonly StackPanel row = new() { Orientation = Orientation.Horizontal, Spacing = 8, Padding = new Thickness(2, 6, 2, 6) };
    private readonly ScrollViewer scroller;
    private readonly Button back;
    private readonly Button forward;
    private readonly string group = "platforms-" + Guid.NewGuid().ToString("N");
    private bool filling;

    public event EventHandler<Platform?>? SelectionChanged;

    public Platform? Selected { get; private set; }

    public PlatformRail()
    {
        scroller = new ScrollViewer
        {
            Content = row,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden,
            HorizontalScrollMode = ScrollMode.Enabled,
            VerticalScrollBarVisibility = ScrollBarVisibility.Disabled,
            VerticalScrollMode = ScrollMode.Disabled,
        };
        scroller.ViewChanged += (_, _) => UpdateArrows();
        scroller.SizeChanged += (_, _) => UpdateArrows();
        // A plain mouse wheel scrolls the row sideways.
        scroller.PointerWheelChanged += (_, e) =>
        {
            var delta = e.GetCurrentPoint(scroller).Properties.MouseWheelDelta;
            scroller.ChangeView(scroller.HorizontalOffset - delta, null, null, disableAnimation: false);
            e.Handled = true;
        };
        back = Arrow("", -1);
        forward = Arrow("", 1);
        var grid = new Grid { ColumnSpacing = 6 };
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        grid.ColumnDefinitions.Add(new ColumnDefinition());
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        grid.Children.Add(back);
        Grid.SetColumn(scroller, 1);
        grid.Children.Add(scroller);
        Grid.SetColumn(forward, 2);
        grid.Children.Add(forward);
        Content = grid;
        AutomationProperties.SetName(this, Strings.Get("FilterPlatform"));
    }

    /// <summary>Fills the rail (the first item is "Todas", with a null platform) and selects one without raising the event.</summary>
    public void SetPlatforms(IReadOnlyList<PlatformChip> chips, Platform? selected)
    {
        filling = true;
        row.Children.Clear();
        foreach (var chip in chips) row.Children.Add(Chip(chip));
        Selected = chips.Any(c => c.Platform == selected) ? selected : null;
        foreach (RadioButton chip in row.Children) chip.IsChecked = (Platform?)chip.Tag == Selected;
        filling = false;
        DispatcherQueue.TryEnqueue(() =>
        {
            UpdateArrows();
            if (row.Children.OfType<RadioButton>().FirstOrDefault(c => c.IsChecked == true) is { } current && Selected is not null)
                current.StartBringIntoView(new BringIntoViewOptions { AnimationDesired = false, HorizontalAlignmentRatio = 0.5 });
        });
    }

    private RadioButton Chip(PlatformChip chip)
    {
        var resources = Application.Current.Resources;
        var key = chip.Platform?.ToString();
        var signature = key is null ? null : PlatformIcon.SignatureBrush(key);
        FrameworkElement icon = key is null
            ? new FontIcon { Glyph = "", FontSize = 15 }
            : new PlatformIcon { PlatformKey = key, Size = 20, Foreground = signature ?? (Brush)resources["Text.Secondary"] };
        var count = new TextBlock { Text = chip.Count.ToString(System.Globalization.CultureInfo.CurrentCulture), FontSize = 11, Opacity = 0.7, VerticalAlignment = VerticalAlignment.Center };
        var button = new RadioButton
        {
            Style = (Style)resources["Lotus.Pill"],
            GroupName = group,
            Tag = chip.Platform,
            Padding = new Thickness(12, 7, 14, 7),
            Content = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 8,
                Children = { icon, new TextBlock { Text = chip.Label, VerticalAlignment = VerticalAlignment.Center }, count },
            },
        };
        ToolTipService.SetToolTip(button, chip.Label);
        AutomationProperties.SetName(button, Strings.Format("PlatformChipName", chip.Label, chip.Count));
        // Selected: the icon takes the fill's text color; otherwise the platform's own color.
        void Tint() { if (icon is PlatformIcon p) p.Foreground = button.IsChecked == true ? (Brush)resources["Accent.OnFill"] : signature ?? (Brush)resources["Text.Secondary"]; }
        button.Checked += (_, _) =>
        {
            Tint();
            if (filling) return;
            Selected = chip.Platform;
            SelectionChanged?.Invoke(this, Selected);
        };
        button.Unchecked += (_, _) => Tint();
        Lotus.Design.Motion.Attach(button, lift: 2f, grow: 1.04f);
        return button;
    }

    private Button Arrow(string glyph, int direction)
    {
        var button = new Button
        {
            Content = new FontIcon { Glyph = glyph, FontSize = 12 },
            Width = 34,
            Height = 34,
            Padding = new Thickness(0),
            CornerRadius = new CornerRadius(17),
            VerticalAlignment = VerticalAlignment.Center,
            Background = (Brush)Application.Current.Resources["Bg.Surface"],
            BorderBrush = (Brush)Application.Current.Resources["Stroke.Subtle"],
            IsTabStop = false,
            Visibility = Visibility.Collapsed,
        };
        AutomationProperties.SetName(button, Strings.Get(direction < 0 ? "RailBack" : "RailForward"));
        button.Click += (_, _) => scroller.ChangeView(scroller.HorizontalOffset + direction * Math.Max(200, scroller.ViewportWidth * 0.7), null, null);
        return button;
    }

    private void UpdateArrows()
    {
        var scrollable = scroller.ScrollableWidth > 1;
        back.Visibility = scrollable && scroller.HorizontalOffset > 1 ? Visibility.Visible : Visibility.Collapsed;
        forward.Visibility = scrollable && scroller.HorizontalOffset < scroller.ScrollableWidth - 1 ? Visibility.Visible : Visibility.Collapsed;
    }
}
