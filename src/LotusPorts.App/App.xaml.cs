using LotusPorts.App.ViewModels;
using LotusPorts.Core.Operations;
using LotusPorts.Core.Preferences;
using LotusPorts.Infrastructure;
using LotusPorts.Infrastructure.Database;
using LotusPorts.Infrastructure.Database.Migrations;
using LotusPorts.Infrastructure.Catalog;
using LotusPorts.Infrastructure.Network;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using Serilog;

namespace LotusPorts.App;

public partial class App : Application
{
    private IHost? host;
    private MainWindow? window;
    private ErrorWindow? errorWindow;

    public App()
    {
        InitializeComponent();
        UnhandledException += (_, e) =>
        {
            Log.Error(e.Exception, "Unhandled UI exception");
            e.Handled = true;
            ShowError(e.Exception, canContinue: true);
        };
        AppDomain.CurrentDomain.UnhandledException += (_, e) => Log.Fatal(e.ExceptionObject as Exception, "Unhandled exception");
        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            Log.Error(e.Exception, "Unobserved task exception");
            e.SetObserved();
        };
    }

    public static IServiceProvider Services => ((App)Current).host?.Services ?? throw new InvalidOperationException("Services are not ready.");

    public static MainWindow? MainWindow => ((App)Current).window;

    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        try
        {
            var paths = AppPaths.ForCurrentUser();
            paths.EnsureStartupFolders();
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.File(Path.Combine(paths.Logs, "lotusports-.log"), rollingInterval: RollingInterval.Day, retainedFileCountLimit: 14)
                .CreateLogger();
            Log.Information("Lotus Ports {Version} starting", AppInfo.Version);

            host = BuildHost(paths);
            await host.Services.GetRequiredService<MigrationRunner>().MigrateAsync();
            var preferences = await host.Services.GetRequiredService<PreferencesService>().LoadAsync();
            Strings.Initialize(preferences.Language);
            await host.Services.GetRequiredService<CatalogService>().LoadAsync();
            var covers = host.Services.GetRequiredService<CoverCache>();
            covers.Shrink = CoverShrinker.ShrinkAsync;
            Lotus.Design.Controls.GameCover.CoverResolver = covers.GetAsync;
            var dispatcher = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
            var play = host.Services.GetRequiredService<PlayService>();
            await play.InitializeAsync(dispatcher);
            var catalogService = host.Services.GetRequiredService<CatalogService>();
            catalogService.IsInstalled = id => catalogService.Library.Find(id)?.AllPorts.Any(p => play.IsPortInstalled(p.Port.Id)) ?? false;
            await host.Services.GetRequiredService<GameFilesService>().InitializeAsync(dispatcher);
            await host.StartAsync();

            window = new MainWindow(StartPage());
            window.Closed += async (_, _) =>
            {
                if (host is not null) await host.StopAsync(TimeSpan.FromSeconds(2));
                await Log.CloseAndFlushAsync();
            };
            window.Activate();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Startup failed");
            ShowError(ex, canContinue: false);
        }
    }

    private static IHost BuildHost(AppPaths paths)
    {
        var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings { DisableDefaults = true });
        var services = builder.Services;
        services.AddSerilog();
        services.Configure<ConsoleLifetimeOptions>(o => o.SuppressStatusMessages = true);
        services.AddSingleton(paths);
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton(new SqliteDatabase(paths.DatabaseFile));
        foreach (var migration in AllMigrations.List) services.AddSingleton(migration);
        services.AddSingleton<MigrationRunner>();
        services.AddSingleton<ISettingsStore, SqliteSettingsStore>();
        services.AddSingleton<IOperationJournal, SqliteOperationJournal>();
        services.AddSingleton<PreferencesService>();
        services.AddSingleton(SystemFolders.FromEnvironment());
        services.AddSingleton<SqliteGameStateStore>();
        services.AddSingleton<SqliteCatalogSourceStore>();
        services.AddSingleton(_ =>
        {
            // Public GitHub data only. No token, no user data in requests.
            var http = new HttpClient(new SocketsHttpHandler { PooledConnectionLifetime = TimeSpan.FromMinutes(5) }) { Timeout = TimeSpan.FromSeconds(20) };
            http.DefaultRequestHeaders.UserAgent.ParseAdd($"LotusPorts/{AppInfo.Version.Split('+')[0]}");
            return new GitHubClient(http);
        });
        services.AddSingleton<CatalogService>();
        services.AddSingleton<CoverCache>();
        services.AddSingleton<LotusPorts.Infrastructure.Install.PortLibrary>();
        services.AddSingleton<LotusPorts.Infrastructure.Install.PortInstaller>();
        services.AddSingleton<LotusPorts.Infrastructure.Install.GameLauncher>();
        services.AddSingleton<PlayService>();
        services.AddSingleton<PortGuideService>();
        services.AddSingleton<LotusPorts.Infrastructure.GameFiles.GameFileStore>();
        services.AddSingleton<LotusPorts.Infrastructure.GameFiles.GameFileScanner>();
        services.AddSingleton<GameFilesService>();
        services.AddSingleton<LotusPorts.Infrastructure.Diagnostics.PortDiagnostics>();
        services.AddTransient<HomeViewModel>();
        services.AddTransient<LibraryViewModel>();
        services.AddTransient<CatalogViewModel>();
        services.AddTransient<SettingsViewModel>();
        return builder.Build();
    }

    /// <summary>Debug builds accept --page=&lt;id&gt; to open a screen directly (used to check screens not reachable yet).</summary>
    private static string StartPage()
    {
#if DEBUG
        var arg = Environment.GetCommandLineArgs().FirstOrDefault(a => a.StartsWith("--page=", StringComparison.Ordinal));
        if (arg is not null) return arg["--page=".Length..];
#endif
        return Pages.Home;
    }

    private void ShowError(Exception exception, bool canContinue)
    {
        if (errorWindow is not null) return;
        errorWindow = new ErrorWindow(exception, canContinue);
        errorWindow.Closed += (_, _) =>
        {
            errorWindow = null;
            if (!canContinue) Exit();
        };
        errorWindow.Activate();
    }
}
