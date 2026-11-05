using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using Lotus.Design.Controls;
using LotusPorts.Core.Operations;
using LotusPorts.Core.Preferences;
using LotusPorts.Infrastructure;
using Serilog;

namespace LotusPorts.App.ViewModels;

public sealed record JournalRow(string When, string Kind, string? PortId, string Outcome, BadgeTone Tone);

/// <summary>Configurações. Every change is saved right away; the language applies after a restart.</summary>
public sealed partial class SettingsViewModel(PreferencesService preferences, AppPaths paths, SystemFolders folders, IOperationJournal journal) : ObservableObject
{
    private bool loaded;

    [ObservableProperty] private int languageIndex = -1;
    [ObservableProperty] private bool restartNeeded;
    [ObservableProperty] private string libraryFolder = "";
    [ObservableProperty] private string libraryFolderError = "";
    [ObservableProperty] private ExperienceStyle defaultStyle;
    [ObservableProperty] private bool autoUpdate;
    [ObservableProperty] private bool advancedMode;
    [ObservableProperty] private bool showInReview;
    [ObservableProperty] private bool journalEmpty = true;
    [ObservableProperty] private string saveError = "";

    public ObservableCollection<JournalRow> Journal { get; } = [];
    public string DataFolder => paths.Root;
    public string Version => AppInfo.Version;

    public async Task LoadAsync()
    {
        var p = await preferences.LoadAsync();
        LanguageIndex = IndexOf(p.Language);
        LibraryFolder = p.LibraryFolder ?? paths.DefaultLibrary;
        DefaultStyle = p.DefaultStyle;
        AutoUpdate = p.AutoUpdate;
        AdvancedMode = p.AdvancedMode;
        ShowInReview = p.ShowInReview;
        loaded = true;
        if (AdvancedMode) await LoadJournalAsync();
    }

    public void ChooseStyle(ExperienceStyle style)
    {
        if (!loaded || style == DefaultStyle) return;
        DefaultStyle = style;
        Save(() => preferences.SetDefaultStyleAsync(style));
    }

    /// <summary>Only the choice is saved here; the folder itself is created when the first game is installed.</summary>
    public void ChooseLibraryFolder(string path)
    {
        var problem = LibraryFolderRules.Check(path, folders);
        if (problem != LibraryFolderProblem.None)
        {
            LibraryFolderError = Strings.Get("LibraryProblem" + problem);
            return;
        }
        LibraryFolderError = "";
        LibraryFolder = path;
        Save(() => preferences.SetLibraryFolderAsync(path));
    }

    partial void OnLanguageIndexChanged(int value)
    {
        if (!loaded || value < 0) return;
        var language = AppPreferences.SupportedLanguages[value];
        RestartNeeded = language != Strings.Language;
        Save(() => preferences.SetLanguageAsync(language));
    }

    partial void OnAutoUpdateChanged(bool value)
    {
        if (loaded) Save(() => preferences.SetAutoUpdateAsync(value));
    }

    partial void OnShowInReviewChanged(bool value)
    {
        if (loaded) Save(() => preferences.SetShowInReviewAsync(value));
    }

    partial void OnAdvancedModeChanged(bool value)
    {
        if (!loaded) return;
        Save(() => preferences.SetAdvancedModeAsync(value));
        if (value) Save(LoadJournalAsync);
    }

    private async Task LoadJournalAsync()
    {
        var entries = await journal.ListRecentAsync(50);
        Journal.Clear();
        foreach (var e in entries)
        {
            Journal.Add(new JournalRow(
                e.StartedAt.ToLocalTime().ToString("g", CultureInfo.CurrentCulture),
                e.Kind.ToString(),
                e.PortId,
                Strings.Get("Outcome" + (e.Outcome?.ToString() ?? "Open")),
                e.Outcome switch
                {
                    OperationOutcome.Success => BadgeTone.Ok,
                    OperationOutcome.Failure => BadgeTone.Failure,
                    OperationOutcome.Partial => BadgeTone.Warning,
                    _ => BadgeTone.Unknown,
                }));
        }
        JournalEmpty = Journal.Count == 0;
    }

    private async void Save(Func<Task> action)
    {
        try
        {
            await action();
            SaveError = "";
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Saving a preference failed");
            SaveError = Strings.Get("SettingsSaveFailed");
        }
    }

    private static int IndexOf(string language)
    {
        for (var i = 0; i < AppPreferences.SupportedLanguages.Count; i++)
            if (AppPreferences.SupportedLanguages[i] == language) return i;
        return 0;
    }
}
