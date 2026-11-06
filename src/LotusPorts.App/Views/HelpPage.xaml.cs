using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace LotusPorts.App.Views;

/// <summary>Ajuda: short answers in plain language, one card per topic, and the welcome again on demand.</summary>
public sealed partial class HelpPage : Page
{
    private static readonly (string Key, string Glyph)[] TopicList =
    [
        ("HelpHow", ""),
        ("HelpLibrary", ""),
        ("HelpFiles", ""),
        ("HelpStyle", ""),
        ("HelpMods", ""),
        ("HelpProblem", ""),
        ("HelpSaves", ""),
        ("HelpLotus", ""),
        ("HelpGamepad", ""),
    ];

    public HelpPage()
    {
        InitializeComponent();
        var res = Application.Current.Resources;
        foreach (var (key, glyph) in TopicList)
        {
            var grid = new Grid { ColumnSpacing = 18 };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.Children.Add(new FontIcon { Glyph = glyph, FontSize = 22, Width = 36, VerticalAlignment = VerticalAlignment.Top, Margin = new Thickness(0, 4, 0, 0), Foreground = (Brush)res["Accent.Rose"] });
            var text = new StackPanel { Spacing = 6 };
            text.Children.Add(new TextBlock { Text = Strings.Get(key + "Title"), Style = (Style)res["Type.Subtitle"] });
            text.Children.Add(new TextBlock { Text = Strings.Get(key + "Text"), Style = (Style)res["Type.Body"], TextWrapping = TextWrapping.Wrap });
            Grid.SetColumn(text, 1);
            grid.Children.Add(text);
            Topics.Children.Add(new Border { Style = (Style)res["Lotus.Card"], Child = grid });
        }
    }

    private void OnShowWelcome(object sender, RoutedEventArgs e) => App.MainWindow?.ShowWelcome();
}
