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
        HistoryTitle.Text = Strings.Get("DownloadsHistory");
        play.Changed += OnChanged;
        Show();
        _ = ShowHistoryAsync();
    }

    private static readonly LotusPorts.Core.Operations.OperationKind[] HistoryKinds =
        [LotusPorts.Core.Operations.OperationKind.Install, LotusPorts.Core.Operations.OperationKind.Update,
         LotusPorts.Core.Operations.OperationKind.Rollback, LotusPorts.Core.Operations.OperationKind.Remove];

    private int activeCount = -1;

    /// <summary>The last installs, updates, rollbacks and removals, from the journal (only finished ones).</summary>
    private async Task ShowHistoryAsync()
    {
        IReadOnlyList<LotusPorts.Core.Operations.OperationEntry> recent;
        try { recent = await App.Services.GetRequiredService<LotusPorts.Core.Operations.IOperationJournal>().ListRecentAsync(80); }
        catch (Exception ex) when (ex is Microsoft.Data.Sqlite.SqliteException or InvalidOperationException) { return; }
        var items = recent.Where(e => HistoryKinds.Contains(e.Kind) && e.FinishedAt is not null && e.PortId is not null).Take(12).ToList();
        History.Children.Clear();
        var ports = App.Services.GetRequiredService<LotusPorts.Infrastructure.Catalog.CatalogService>();
        foreach (var e in items)
        {
            var port = ports.Library.Games.SelectMany(g => g.AllPorts).FirstOrDefault(c => c.Port.Id == e.PortId)?.Port;
            var title = port is null ? e.PortId! : catalog.Title(port.GameId) ?? port.Title;
            var what = Strings.Get("DownloadsKind" + e.Kind) + (e.Outcome == LotusPorts.Core.Operations.OperationOutcome.Success ? "" : " · " + Strings.Get("DownloadsOutcomeFailed"));
            var when = e.FinishedAt!.Value.ToLocalTime().ToString("g", System.Globalization.CultureInfo.CurrentCulture);
            var row = new Grid { ColumnSpacing = 12 };
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            row.Children.Add(new StackPanel
            {
                Spacing = 1,
                Children =
                {
                    new TextBlock { Text = title, Style = (Style)Application.Current.Resources["Type.Body"], TextTrimming = TextTrimming.CharacterEllipsis },
                    new TextBlock { Text = $"{what} · {when}", Style = (Style)Application.Current.Resources["Type.Caption"] },
                },
            });
            if (port is not null)
            {
                var open = new HyperlinkButton { Content = Strings.Get("DownloadsOpenGame"), VerticalAlignment = VerticalAlignment.Center };
                var gameId = port.GameId;
                open.Click += (_, _) => App.MainWindow?.Navigate(Pages.Game, gameId);
                Grid.SetColumn(open, 1);
                row.Children.Add(open);
            }
            History.Children.Add(row);
        }
        HistoryCard.Visibility = items.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e) => play.Changed -= OnChanged;

    private void OnChanged(string _) => Show();

    private void Show()
    {
        var working = play.Working;
        // A download that just finished moves to "Concluídos recentemente".
        if (activeCount > working.Count) _ = ShowHistoryAsync();
        activeCount = working.Count;
        EmptyCard.Visibility = working.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        BulletsCard.Visibility = working.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
        Bullets.Children.Clear();
        foreach (var (port, progress) in working)
        {
            var title = catalog.Title(port.GameId) ?? port.Title;
            var status = ProgressText.For(progress);
            var row = new Grid { ColumnSpacing = 16 };
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            row.Children.Add(new StackPanel
            {
                Spacing = 6,
                Children =
                {
                    new TextBlock { Text = title, Style = (Style)Application.Current.Resources["Type.Body"], FontWeight = Microsoft.UI.Text.FontWeights.SemiBold },
                    new ProgressBar { Maximum = 1, Value = progress.Fraction, IsIndeterminate = ProgressText.Indeterminate(progress) },
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
