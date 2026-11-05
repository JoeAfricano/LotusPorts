using LotusPorts.Core.Privacy;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.ApplicationModel.DataTransfer;
using Windows.Graphics;

namespace LotusPorts.App;

/// <summary>Friendly error screen: plain message, technical details folded away, copy button.</summary>
public sealed partial class ErrorWindow : Window
{
    public ErrorWindow(Exception exception, bool canContinue)
    {
        Title = "Lotus Ports";
        var details = PathAnonymizer.Anonymize($"Lotus Ports {AppInfo.Version}\n{exception}");
        var resources = Application.Current.Resources;
        var copy = new Button { Content = Strings.Get("ErrorCopy") };
        var copied = new TextBlock { Style = (Style)resources["Type.Caption"], VerticalAlignment = VerticalAlignment.Center };
        copy.Click += (_, _) =>
        {
            var package = new DataPackage();
            package.SetText(details);
            Clipboard.SetContent(package);
            copied.Text = Strings.Get("ErrorCopied");
        };
        var close = new Button { Content = Strings.Get(canContinue ? "ErrorContinue" : "ErrorClose"), Style = (Style)resources["AccentButtonStyle"] };
        close.Click += (_, _) => Close();

        Content = new Grid
        {
            Background = (Brush)resources["Bg.Base"],
            Padding = new Thickness(32),
            Children =
            {
                new StackPanel
                {
                    Spacing = 14,
                    Children =
                    {
                        new TextBlock { Text = Strings.Get("ErrorTitle"), Style = (Style)resources["Type.Title"] },
                        new TextBlock { Text = Strings.Get(canContinue ? "ErrorMessageContinue" : "ErrorMessageStartup"), Style = (Style)resources["Type.Body"] },
                        new Expander
                        {
                            Header = Strings.Get("ErrorDetails"),
                            HorizontalAlignment = HorizontalAlignment.Stretch,
                            HorizontalContentAlignment = HorizontalAlignment.Stretch,
                            Content = new TextBox { Text = details, IsReadOnly = true, TextWrapping = TextWrapping.Wrap, MaxHeight = 220, FontFamily = new FontFamily("Cascadia Mono, Consolas") },
                        },
                        new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10, Children = { close, copy, copied } },
                    },
                },
            },
        };
        AppWindow.Resize(new SizeInt32(720, 460));
    }
}
