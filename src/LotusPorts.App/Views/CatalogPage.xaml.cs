using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using LotusPorts.App.ViewModels;
using LotusPorts.Catalog;
using LotusPorts.Infrastructure.Catalog;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage.Pickers;

namespace LotusPorts.App.Views;

public sealed partial class CatalogPage : Page
{
    public CatalogPage()
    {
        ViewModel = App.Services.GetRequiredService<CatalogViewModel>();
        InitializeComponent();
        Loaded += async (_, _) =>
        {
            await ViewModel.LoadAsync();
            loading = true;
            PlatformBox.ItemsSource = ViewModel.Platforms;
            GenreBox.ItemsSource = ViewModel.Genres;
            // Opened from Início with a console or a category already chosen.
            var wantedPlatform = initial?.Platform;
            PlatformBox.SelectedIndex = wantedPlatform is null ? 0 : Math.Max(0, ViewModel.Platforms.FindIndex(p => p.Platform == wantedPlatform));
            var wantedGenre = initial?.Genre;
            GenreBox.SelectedIndex = wantedGenre is null ? 0 : Math.Max(0, ViewModel.Genres.FindIndex(g => string.Equals(g, wantedGenre, StringComparison.OrdinalIgnoreCase)));
            SortBox.SelectedIndex = (int)(initial?.Sort ?? CatalogSort.Name);
            loading = false;
            OnFilterChanged(this, EventArgs.Empty);
        };
    }

    public CatalogViewModel ViewModel { get; }

    private CatalogService Service => ViewModel.Service;

    private CatalogFilterArgument? initial;
    private bool loading;

    protected override void OnNavigatedTo(Microsoft.UI.Xaml.Navigation.NavigationEventArgs e) => initial = e.Parameter as CatalogFilterArgument;

    private void OnFilterChanged(object sender, object e)
    {
        if (loading) return;
        ViewModel.Filter(FilterBox.Text, (PlatformBox.SelectedItem as PlatformChip)?.Platform, GenreBox.SelectedIndex <= 0 ? null : GenreBox.SelectedItem as string,
            (CatalogSort)Math.Max(0, SortBox.SelectedIndex));
    }

    private async void OnAddSource(object sender, RoutedEventArgs e)
    {
        AddSourceButton.IsEnabled = false;
        try
        {
            var result = await Service.AddSourceAsync(SourceLink.Text);
            SourceResult.Severity = result.Outcome == AddSourceOutcome.Added ? InfoBarSeverity.Success
                : result.Outcome is AddSourceOutcome.GameFilesRejected or AddSourceOutcome.Invalid ? InfoBarSeverity.Error : InfoBarSeverity.Warning;
            SourceResult.Message = CatalogViewModel.AddResultMessage(result);
            SourceResult.IsOpen = true;
            if (result.Outcome == AddSourceOutcome.Added) SourceLink.Text = "";
            await ViewModel.LoadAsync();
        }
        finally
        {
            AddSourceButton.IsEnabled = true;
        }
    }

    private void OnSourceMenuOpening(object sender, object e)
    {
        var menu = (MenuFlyout)sender;
        if (menu.Target is not Button { Tag: string id }) return;
        var source = Service.Sources.FirstOrDefault(s => s.Id == id);
        if (source is null) return;
        menu.Items.Clear();
        menu.Items.Add(Item(Strings.Get("SourceRefresh"), "\uE72C", async () => await Service.RefreshSourceAsync(id)));
        menu.Items.Add(Item(Strings.Get(source.Active ? "SourceDeactivate" : "SourceActivate"), "\uE7E8", () => Service.SetSourceActiveAsync(id, !source.Active)));
        menu.Items.Add(Item(Strings.Get(source.Trusted ? "SourceUntrust" : "SourceTrust"), "\uE8FB", () => Service.SetSourceTrustedAsync(id, !source.Trusted)));
        menu.Items.Add(new MenuFlyoutSeparator());
        menu.Items.Add(Item(Strings.Get("SourceRemove"), "\uE74D", async () =>
        {
            // Removals always ask, and say what is kept.
            if (await Confirm(Strings.Get("SourceRemoveTitle"), Strings.Format("SourceRemoveMessage", source.Name), Strings.Get("SourceRemove")))
                await Service.RemoveSourceAsync(id);
        }));
    }

    private MenuFlyoutItem Item(string text, string glyph, Func<Task> action)
    {
        var item = new MenuFlyoutItem { Text = text, Icon = new FontIcon { Glyph = glyph } };
        item.Click += async (_, _) =>
        {
            await action();
            await ViewModel.LoadAsync();
        };
        return item;
    }

    private async Task<bool> Confirm(string title, string message, string primary)
    {
        var dialog = new ContentDialog
        {
            XamlRoot = XamlRoot,
            Title = title,
            Content = new TextBlock { Text = message, TextWrapping = TextWrapping.Wrap },
            PrimaryButtonText = primary,
            CloseButtonText = Strings.Get("Cancel"),
            DefaultButton = ContentDialogButton.Close,
        };
        return await dialog.ShowAsync() == ContentDialogResult.Primary;
    }

    // ---- Advanced: add a port from a GitHub link ----------------------------------------------

    private async void OnAddPortByLink(object sender, RoutedEventArgs e)
    {
        var link = new TextBox { PlaceholderText = "https://github.com/usuario/port", Header = Strings.Get("UserPortLink") };
        var read = new Button { Content = Strings.Get("UserPortRead") };
        var status = new TextBlock { TextWrapping = TextWrapping.Wrap, Style = (Style)Application.Current.Resources["Type.Caption"] };
        var gameTitle = new TextBox { Header = Strings.Get("UserPortGameTitle") };
        var platform = new ComboBox { Header = Strings.Get("FilterPlatform"), ItemsSource = Enum.GetValues<Platform>().Select(p => new PlatformChip(p, CatalogLabels.Platform(p))).ToList(), DisplayMemberPath = "Label", SelectedIndex = 0, MinWidth = 220 };
        var portType = new ComboBox { Header = Strings.Get("AboutPortType"), MinWidth = 220, SelectedIndex = 0 };
        string[] types = ["decomp-port", "static-recomp", "source-port", "engine-remake"];
        foreach (var t in types) portType.Items.Add(CatalogLabels.PortType(t));
        var setup = new ComboBox { Header = Strings.Get("AboutSetup"), MinWidth = 220, SelectedIndex = 0 };
        string?[] kinds = [null, "select-on-first-launch", "beside-exe", "manual"];
        foreach (var k in kinds) setup.Items.Add(CatalogLabels.Setup(k));
        var exe = new ComboBox { Header = Strings.Get("UserPortExecutable"), MinWidth = 320 };
        var form = new StackPanel { Spacing = 10, Visibility = Visibility.Collapsed, Children = { gameTitle, platform, portType, setup, exe } };

        var dialog = new ContentDialog
        {
            XamlRoot = XamlRoot,
            Title = Strings.Get("UserPortAdd"),
            Content = new ScrollViewer { Content = new StackPanel { Spacing = 12, Width = 460, Children = { new TextBlock { Text = Strings.Get("UserPortHint"), TextWrapping = TextWrapping.Wrap }, link, read, status, form } } },
            PrimaryButtonText = Strings.Get("UserPortConfirm"),
            CloseButtonText = Strings.Get("Cancel"),
            IsPrimaryButtonEnabled = false,
        };

        PortSuggestion? suggestion = null;
        read.Click += async (_, _) =>
        {
            read.IsEnabled = false;
            status.Text = Strings.Get("UserPortReading");
            var (found, problem) = await Service.InspectRepositoryAsync(link.Text);
            read.IsEnabled = true;
            suggestion = found;
            if (found is null)
            {
                status.Text = Strings.Get("UserPortProblem" + problem);
                form.Visibility = Visibility.Collapsed;
                dialog.IsPrimaryButtonEnabled = false;
                return;
            }
            status.Text = Strings.Format("UserPortFound", $"{found.Repository.Owner}/{found.Repository.Name}", found.Release.Tag, found.Asset.Name)
                + (found.Release.Prerelease ? " " + Strings.Get("UserPortPrerelease") : "");
            gameTitle.Text = found.Repository.Name;
            exe.ItemsSource = found.Executables;
            exe.SelectedIndex = found.Executables.Count > 0 ? 0 : -1;
            form.Visibility = Visibility.Visible;
            dialog.IsPrimaryButtonEnabled = true;
        };

        dialog.PrimaryButtonClick += async (d, args) =>
        {
            if (suggestion is null || string.IsNullOrWhiteSpace(gameTitle.Text)) { args.Cancel = true; return; }
            var deferral = args.GetDeferral();
            try
            {
                var (game, port) = BuildUserEntry(suggestion, gameTitle.Text.Trim(), ((PlatformChip)platform.SelectedItem).Platform!.Value,
                    types[portType.SelectedIndex], kinds[setup.SelectedIndex], exe.SelectedItem as string);
                var result = await Service.AddUserPortAsync(game, port);
                if (!result.IsValid)
                {
                    status.Text = Strings.Format("UserPortInvalid", string.Join("; ", result.Issues.Take(3).Select(i => i.Message)));
                    args.Cancel = true;
                }
            }
            finally
            {
                deferral.Complete();
            }
        };

        if (await dialog.ShowAsync() == ContentDialogResult.Primary) await ViewModel.LoadAsync();
    }

    private static (Game Game, Port Port) BuildUserEntry(PortSuggestion s, string title, Platform platform, string portType, string? setupKind, string? executable)
    {
        var gameId = Slug(title);
        var portId = "user-" + Slug(s.Repository.Name);
        var repo = $"https://github.com/{s.Repository.Owner}/{s.Repository.Name}";
        var package = s.Asset.Name.EndsWith(".zip", StringComparison.OrdinalIgnoreCase) ? "zip" : "portable-exe";
        var game = new Game(1, gameId, title, platform, null, [], Strings.Get("UserPortDescription"), portId, null, [], "port:" + gameId,
            new Review(ReviewState.Pending, null, null, null));
        var port = new Port(1, portId, gameId, s.Repository.Name, portType, PortStatus.EXPERIMENTAL, [], null, null,
            new RepoRef(s.Repository.Owner, s.Repository.Name), s.Repository.License,
            // Pinned to today's file name: whoever adds it to the official catalog generalizes the pattern.
            new WindowsAsset("(?i)^" + Regex.Escape(s.Asset.Name) + "$", null, s.Release.Prerelease, package),
            package == "portable-exe" ? s.Asset.Name : executable, [],
            new PortSetup(setupKind, null, null, null, null, null, null), [], [],
            new ExperiencePresets(null, null, new Preset(new Dictionary<string, System.Text.Json.JsonElement>(), false), new Preset(new Dictionary<string, System.Text.Json.JsonElement>(), false)),
            null, null, null, [], new PortLinks(repo, repo + "#readme", null), null, null,
            new PortEvidence(DateTime.UtcNow.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), s.Release.Tag,
                s.Release.PublishedAt?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), null, s.Release.Prerelease, s.Asset.Name, s.Asset.Size, s.Asset.Sha256));
        return (game, port);
    }

    private static string Slug(string text)
    {
        var normalized = text.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder();
        foreach (var c in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark) continue;
            builder.Append(char.IsLetterOrDigit(c) ? char.ToLowerInvariant(c) : '-');
        }
        var slug = Regex.Replace(builder.ToString(), "-+", "-").Trim('-');
        return slug.Length == 0 ? "jogo" : slug[..Math.Min(slug.Length, 60)].Trim('-');
    }

    private async void OnExportUserPorts(object sender, RoutedEventArgs e)
    {
        var json = Service.ExportUserCatalog();
        if (json is null) return;
        var picker = new FileSavePicker { SuggestedFileName = "lotusports-meus-ports", SuggestedStartLocation = PickerLocationId.DocumentsLibrary };
        picker.FileTypeChoices.Add("JSON", [".json"]);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow));
        var file = await picker.PickSaveFileAsync();
        if (file is null) return;
        await File.WriteAllTextAsync(file.Path, json);
        SourceResult.Severity = InfoBarSeverity.Success;
        SourceResult.Message = Strings.Format("UserPortExported", file.Name);
        SourceResult.IsOpen = true;
    }
}
