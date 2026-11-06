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
            await host.Services.GetRequiredService<AppearanceService>().InitializeAsync(dispatcher);
            var play = host.Services.GetRequiredService<PlayService>();
            await play.InitializeAsync(dispatcher);
            var catalogService = host.Services.GetRequiredService<CatalogService>();
            catalogService.IsInstalled = id => catalogService.Library.Find(id)?.AllPorts.Any(p => play.IsPortInstalled(p.Port.Id)) ?? false;
            await host.Services.GetRequiredService<GameFilesService>().InitializeAsync(dispatcher);
            await host.Services.GetRequiredService<LotusBridge>().StartAsync(dispatcher);
            await host.StartAsync();

            window = new MainWindow(StartPage());
            var server = StartIntegrationServer(dispatcher);
            window.Closed += async (_, _) =>
            {
                await server.DisposeAsync();
#if !DEBUG
                LotusPorts.Integration.PortsRegistration.Unregister();
#endif
                if (host is not null) await host.StopAsync(TimeSpan.FromSeconds(2));
                await Log.CloseAndFlushAsync();
            };
            window.Activate();
            if (DeepLink() is { Play: true } link) _ = host.Services.GetRequiredService<PortsHandler>().LaunchAsync(link.GameId);
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
        services.AddSingleton<LotusPorts.Integration.ILotusDiscovery, LotusPorts.Integration.RegistryLotusDiscovery>();
        services.AddSingleton<LotusPorts.Integration.IPeerValidator, LotusPorts.Integration.ProcessPeerValidator>();
        services.AddSingleton(sp => new LotusPorts.Integration.LotusClient(
            sp.GetRequiredService<LotusPorts.Integration.ILotusDiscovery>(), sp.GetRequiredService<LotusPorts.Integration.IPeerValidator>(),
            sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<LotusPorts.Integration.LotusClient>>(), AppInfo.Version.Split('+')[0]));
        services.AddSingleton<LotusBridge>();
        services.AddSingleton<PortsHandler>();
        services.AddSingleton<AppearanceService>();
        services.AddSingleton<LotusPorts.Infrastructure.Diagnostics.PortDiagnostics>();
        services.AddSingleton<LotusPorts.Infrastructure.Install.StyleService>();
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
        // lotusports://game/{gameId} opens the game page; lotusports://play/{gameId} opens it and plays (section 12.2).
        if (DeepLink() is { } link) return "game:" + link.GameId;
        return Pages.Home;
    }

    /// <summary>
    /// Lotus Ports' own pipe, so Lotus can list and open games (section 12.7). The registry entries that let Lotus find
    /// it and the lotusports:// links are written only by Release builds: development runs never touch the registry.
    /// </summary>
    private LotusPorts.Integration.PortsServer StartIntegrationServer(Microsoft.UI.Dispatching.DispatcherQueue dispatcher)
    {
        var services = host!.Services;
        var handler = services.GetRequiredService<PortsHandler>();
        handler.Attach(dispatcher);
        var server = new LotusPorts.Integration.PortsServer(Lotus.Integration.Contracts.Protocol.PortsPipeName, handler,
            new LotusPorts.Integration.LotusClientValidator(services.GetRequiredService<LotusPorts.Integration.ILotusDiscovery>()),
            services.GetRequiredService<Microsoft.Extensions.Logging.ILogger<LotusPorts.Integration.PortsServer>>(), AppInfo.Version.Split('+')[0]);
        server.Start();
#if !DEBUG
        try
        {
            LotusPorts.Integration.PortsRegistration.Register(AppInfo.Version.Split('+')[0], Environment.ProcessPath!);
            LotusPorts.Integration.PortsRegistration.RegisterLinks(Environment.ProcessPath!);
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or System.Security.SecurityException or IOException)
        {
            Log.Warning(ex, "Integration registry entries not written");
        }
#endif
        return server;
    }

    /// <summary>The lotusports:// link this instance was started with, if any. Only a catalog-shaped id is accepted.</summary>
    public static (string GameId, bool Play)? DeepLink()
    {
        var arg = Environment.GetCommandLineArgs().Skip(1).FirstOrDefault(a => a.StartsWith("lotusports://", StringComparison.OrdinalIgnoreCase));
        if (arg is null || !Uri.TryCreate(arg, UriKind.Absolute, out var uri)) return null;
        var id = uri.AbsolutePath.Trim('/');
        if (id.Length is 0 or > 100 || !id.All(c => c is (>= 'a' and <= 'z') or (>= '0' and <= '9') or '-')) return null;
        return uri.Host.ToLowerInvariant() switch
        {
            "game" => (id, false),
            "play" => (id, true),
            _ => null,
        };
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
