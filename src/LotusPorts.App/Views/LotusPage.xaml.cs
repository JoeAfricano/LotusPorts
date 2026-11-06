using Lotus.Design.Controls;
using Lotus.Integration.Contracts;
using LotusPorts.Infrastructure.Lotus;
using LotusPorts.Integration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace LotusPorts.App.Views;

/// <summary>Integração com o Lotus: the real connection state, what works now, and the switches (section 12.10).</summary>
public sealed partial class LotusPage : Page
{
    private readonly LotusBridge bridge = App.Services.GetRequiredService<LotusBridge>();

    /// <summary>Each feature, the capability Lotus must announce for it and the switch that allows it.</summary>
    private static readonly (string Key, string Capability, LotusPermission Permission)[] Features =
    [
        ("LotusFeatureDiagnostics", Methods.DiagnosticsRunSystemChecks, LotusPermission.Diagnostics),
        ("LotusFeatureChanges", Methods.JournalQueryRecent, LotusPermission.Diagnostics),
        ("LotusFeatureProfile", Methods.SessionStart, LotusPermission.PerformanceProfile),
        ("LotusFeatureDependencies", Methods.DependenciesCheck, LotusPermission.Dependencies),
        ("LotusFeatureHistory", Events.GameInstalled, LotusPermission.ShareHistory),
    ];

    public LotusPage() => InitializeComponent();

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        bridge.Changed += Show;
        Show();
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e) => bridge.Changed -= Show;

    private void Show()
    {
        var status = bridge.Status;
        StatusPetal.IsLit = status.State == LotusConnectionState.Connected;
        StatusTitle.Text = status.State == LotusConnectionState.Connected
            ? Strings.Format("LotusStateConnected", status.Version ?? "?")
            : Strings.Get("LotusState" + status.State);
        StatusText.Text = Strings.Get("LotusStateHint" + status.State);
        OpenLotusButton.Content = Strings.Get(status.State == LotusConnectionState.NotFound ? "ProfileGetLotus" : "LotusOpen");
        RetryButton.Visibility = status.State is LotusConnectionState.Connected or LotusConnectionState.NotFound ? Visibility.Collapsed : Visibility.Visible;

        FeatureList.Children.Clear();
        foreach (var (key, capability, permission) in Features)
        {
            var available = status.State == LotusConnectionState.Connected && status.Capabilities.Contains(capability);
            var (text, tone) = !available ? (Strings.Get(status.State == LotusConnectionState.Connected ? "LotusFeatureNotOffered" : "LotusFeatureNeedsLotus"), BadgeTone.Unknown)
                : bridge.IsAllowed(permission) ? (Strings.Get("LotusFeatureOn"), BadgeTone.Ok)
                : (Strings.Get("LotusFeatureOff"), BadgeTone.Warning);
            FeatureList.Children.Add(Row(Strings.Get(key), new StateBadge { Text = text, Tone = tone }));
        }

        PermissionList.Children.Clear();
        foreach (var permission in Enum.GetValues<LotusPermission>())
        {
            var toggle = new ToggleSwitch { IsOn = bridge.IsAllowed(permission), OnContent = "", OffContent = "", MinWidth = 0 };
            toggle.Toggled += async (_, _) => await bridge.SetAllowedAsync(permission, toggle.IsOn);
            PermissionList.Children.Add(Row(Strings.Get("LotusAllow" + permission), toggle));
        }
    }


    private static Grid Row(string text, FrameworkElement right)
    {
        var row = new Grid { ColumnSpacing = 16, MinHeight = 36 };
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        row.Children.Add(new TextBlock { Text = text, Style = (Style)Application.Current.Resources["Type.Body"], VerticalAlignment = VerticalAlignment.Center, TextWrapping = TextWrapping.Wrap });
        right.VerticalAlignment = VerticalAlignment.Center;
        Grid.SetColumn(right, 1);
        row.Children.Add(right);
        return row;
    }

    private async void OnOpenLotus(object sender, RoutedEventArgs e)
    {
        try
        {
            if (LotusProfileReader.OpenLotus()) return;
        }
        catch (System.ComponentModel.Win32Exception)
        {
        }
        await Windows.System.Launcher.LaunchUriAsync(LotusProfileReader.DownloadPage);
    }

    private async void OnRetry(object sender, RoutedEventArgs e) => await bridge.RetryAsync();
}
