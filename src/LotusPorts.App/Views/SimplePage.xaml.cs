using Lotus.Design.Controls;
using LotusPorts.Infrastructure.Install;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace LotusPorts.App.Views;

/// <summary>Downloads: installs and updates in progress, live, with Cancel; an honest empty state otherwise.</summary>
public sealed partial class SimplePage : Page
{
    private readonly PlayService play = App.Services.GetRequiredService<PlayService>();
    private readonly CatalogLookup catalog = new();

    public SimplePage() => InitializeComponent();

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        Heading.Title = Strings.Get("DownloadsTitle");
        Subtitle.Text = Strings.Get("DownloadsSubtitle");
        Empty.Title = Strings.Get("DownloadsEmptyTitle");
        Empty.Message = Strings.Get("DownloadsEmptyMessage");
        Empty.BadgeText = "";
        Empty.Illustration = "Assets/Art/Illustrations/download.png";
        BulletsTitle.Text = Strings.Get("DownloadsActive");
        play.Changed += OnChanged;
        Show();
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e) => play.Changed -= OnChanged;

    private void OnChanged(string _) => Show();

    private void Show()
    {
        var working = play.Working;
        EmptyCard.Visibility = working.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        BulletsCard.Visibility = working.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
        Bullets.Children.Clear();
        foreach (var (port, progress) in working)
        {
            var title = catalog.Title(port.GameId) ?? port.Title;
            var status = progress.Stage switch
            {
                InstallStage.Downloading => Strings.Format("PlayDownloading", (int)Math.Round(progress.Fraction * 100)),
                InstallStage.Extracting => Strings.Format("PlayExtracting", (int)Math.Round(progress.Fraction * 100)),
                _ => Strings.Get("PlayFinishing"),
            };
            var row = new Grid { ColumnSpacing = 16 };
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            row.Children.Add(new StackPanel
            {
                Spacing = 6,
                Children =
                {
                    new TextBlock { Text = title, Style = (Style)Application.Current.Resources["Type.Body"], FontWeight = Microsoft.UI.Text.FontWeights.SemiBold },
                    new ProgressBar { Maximum = 1, Value = progress.Fraction, IsIndeterminate = progress.Stage == InstallStage.Finishing },
                    new TextBlock { Text = status, Style = (Style)Application.Current.Resources["Type.Caption"] },
                },
            });
            var cancel = new Button { Content = Strings.Get("PlayCancel"), VerticalAlignment = VerticalAlignment.Center };
            var id = port.Id;
            cancel.Click += (_, _) => play.Cancel(id);
            Grid.SetColumn(cancel, 1);
            row.Children.Add(cancel);
            var open = new HyperlinkButton { Content = Strings.Get("DownloadsOpenGame"), Padding = new Thickness(0, 2, 0, 2) };
            var gameId = port.GameId;
            open.Click += (_, _) => App.MainWindow?.Navigate(Pages.Game, gameId);
            Bullets.Children.Add(new StackPanel { Spacing = 2, Children = { row, open } });
        }
    }

    private sealed class CatalogLookup
    {
        private readonly LotusPorts.Infrastructure.Catalog.CatalogService service = App.Services.GetRequiredService<LotusPorts.Infrastructure.Catalog.CatalogService>();

        public string? Title(string gameId) => service.Library.Find(gameId)?.Game.Title;
    }
}
