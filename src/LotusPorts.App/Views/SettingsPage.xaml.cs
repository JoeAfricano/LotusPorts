using System.Diagnostics;
using LotusPorts.App.ViewModels;
using LotusPorts.Core.Preferences;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage.Pickers;

namespace LotusPorts.App.Views;

public sealed partial class SettingsPage : Page
{
    public SettingsPage()
    {
        ViewModel = App.Services.GetRequiredService<SettingsViewModel>();
        InitializeComponent();
        Loaded += async (_, _) =>
        {
            await ViewModel.LoadAsync();
            (ViewModel.DefaultStyle == ExperienceStyle.Remastered ? StyleRemastered : StyleOriginal).IsChecked = true;
        };
    }

    public SettingsViewModel ViewModel { get; }

    private void OnStyleChecked(object sender, RoutedEventArgs e)
    {
        if (sender is RadioButton { Tag: string tag } && Enum.TryParse<ExperienceStyle>(tag, out var style)) ViewModel.ChooseStyle(style);
    }

    private async void OnChooseLibraryFolder(object sender, RoutedEventArgs e)
    {
        var picker = new FolderPicker { SuggestedStartLocation = PickerLocationId.ComputerFolder };
        picker.FileTypeFilter.Add("*");
        WinRT.Interop.InitializeWithWindow.Initialize(picker, WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow));
        var folder = await picker.PickSingleFolderAsync();
        if (folder is not null) ViewModel.ChooseLibraryFolder(folder.Path);
    }

    private void OnOpenDataFolder(object sender, RoutedEventArgs e) =>
        Process.Start(new ProcessStartInfo("explorer.exe", $"\"{ViewModel.DataFolder}\"") { UseShellExecute = true });
}
