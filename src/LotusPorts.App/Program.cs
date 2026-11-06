using System.Runtime.InteropServices;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;

namespace LotusPorts.App;

/// <summary>
/// One Lotus Ports window at a time. A second start (a shortcut, a lotusports:// link from Lotus or the browser)
/// hands its arguments to the window already open and quits, so a link never opens a second copy of the app.
/// </summary>
public static partial class Program
{
#if DEBUG
    // Development runs (and screenshots) never take over the installed app's window.
    private const string InstanceKey = "LotusPorts.Dev";
#else
    private const string InstanceKey = "LotusPorts.Main";
#endif

    [STAThread]
    private static int Main(string[] args)
    {
        WinRT.ComWrappersSupport.InitializeComWrappers();
        if (!IsDevScreenshotRun(args))
        {
            var main = AppInstance.FindOrRegisterForKey(InstanceKey);
            if (!main.IsCurrent)
            {
                // Let the window already open come to the front, then hand it this start's arguments.
                AllowSetForegroundWindow(main.ProcessId);
                var activation = AppInstance.GetCurrent().GetActivatedEventArgs();
                Task.Run(() => main.RedirectActivationToAsync(activation).AsTask()).Wait();
                return 0;
            }
            main.Activated += (_, e) => App.OnRedirected(e);
        }

        Application.Start(_ =>
        {
            SynchronizationContext.SetSynchronizationContext(new DispatcherQueueSynchronizationContext(DispatcherQueue.GetForCurrentThread()));
            new App();
        });
        return 0;
    }

    /// <summary>Debug runs with --page or --welcome are screenshots and tests: each gets its own window.</summary>
    private static bool IsDevScreenshotRun(string[] args)
    {
#if DEBUG
        return args.Any(a => a.StartsWith("--page=", StringComparison.Ordinal) || a.StartsWith("--welcome", StringComparison.Ordinal));
#else
        return false;
#endif
    }

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool AllowSetForegroundWindow(uint processId);
}
