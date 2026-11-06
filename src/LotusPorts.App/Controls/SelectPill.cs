using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace LotusPorts.App.Controls;

/// <summary>
/// A choice shown as a rounded pill ("Gênero · Ação ▾") that opens a menu with the options, the chosen one checked.
/// Replaces plain combo boxes in the filters.
/// </summary>
public sealed partial class SelectPill : UserControl
{
    private readonly DropDownButton button;
    private readonly TextBlock caption = new() { VerticalAlignment = VerticalAlignment.Center };
    private readonly TextBlock value = new() { VerticalAlignment = VerticalAlignment.Center, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, MaxWidth = 220, TextTrimming = TextTrimming.CharacterEllipsis };
    private readonly MenuFlyout menu = new() { Placement = Microsoft.UI.Xaml.Controls.Primitives.FlyoutPlacementMode.BottomEdgeAlignedLeft };
    private readonly string group = "pill-" + Guid.NewGuid().ToString("N");
    private IReadOnlyList<string> items = [];
    private int selectedIndex = -1;

    public event EventHandler? SelectionChanged;

    public SelectPill()
    {
        var resources = Application.Current.Resources;
        caption.Foreground = (Brush)resources["Text.Secondary"];
        button = new DropDownButton
        {
            Flyout = menu,
            CornerRadius = new CornerRadius(999),
            Padding = new Thickness(16, 8, 12, 8),
            Background = (Brush)resources["Bg.Surface"],
            BorderBrush = (Brush)resources["Stroke.Subtle"],
            Content = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 6, Children = { caption, value } },
        };
        Lotus.Design.Motion.Attach(button, lift: 2f, grow: 1.03f);
        Content = button;
    }

    /// <summary>What is being chosen ("Gênero"), shown before the value.</summary>
    public string Label
    {
        get => caption.Text;
        set
        {
            caption.Text = value;
            AutomationProperties.SetName(button, value);
        }
    }

    public IReadOnlyList<string> Items
    {
        get => items;
        set
        {
            items = value;
            menu.Items.Clear();
            for (var i = 0; i < items.Count; i++)
            {
                var index = i;
                var item = new RadioMenuFlyoutItem { Text = items[i], GroupName = group };
                item.Click += (_, _) => Select(index, raise: true);
                menu.Items.Add(item);
            }
            Show();
        }
    }

    public int SelectedIndex
    {
        get => selectedIndex;
        set => Select(value, raise: false);
    }

    public string? SelectedItem => selectedIndex >= 0 && selectedIndex < items.Count ? items[selectedIndex] : null;

    private void Select(int index, bool raise)
    {
        var changed = index != selectedIndex;
        selectedIndex = index;
        Show();
        if (raise && changed) SelectionChanged?.Invoke(this, EventArgs.Empty);
    }

    private void Show()
    {
        value.Text = SelectedItem ?? "";
        for (var i = 0; i < menu.Items.Count; i++)
            if (menu.Items[i] is RadioMenuFlyoutItem item) item.IsChecked = i == selectedIndex;
        AutomationProperties.SetItemStatus(button, value.Text);
    }
}
