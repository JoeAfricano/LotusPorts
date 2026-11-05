using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace LotusPorts.App.Views;

/// <summary>Página do jogo. Without a catalog (phase 1) every id is honestly "not found".</summary>
public sealed partial class GamePage : Page
{
    public GamePage() => InitializeComponent();

    private void OnBack(object sender, RoutedEventArgs e) => App.MainWindow?.Navigate(Pages.Library);
}
