using System.Globalization;
using LotusPorts.Core.Install;

namespace LotusPorts.App;

/// <summary>"Jogado por 3 h 20 min · 5 partidas · última vez ontem", from what Lotus Ports recorded when games closed.</summary>
public static class PlayTime
{
    public static string? Line(InstalledPort installed, DateTimeOffset? now = null)
    {
        ArgumentNullException.ThrowIfNull(installed);
        if (installed.Sessions == 0 || installed.LastRun is not { } last) return null;
        var parts = new List<string> { Strings.Format("PlayTimeTotal", Duration(TimeSpan.FromSeconds(installed.PlayedSeconds))) };
        parts.Add(Strings.Format(installed.Sessions == 1 ? "PlayTimeSessionsOne" : "PlayTimeSessionsMany", installed.Sessions));
        parts.Add(Strings.Format("PlayTimeLast", When(last.At, now ?? DateTimeOffset.Now)));
        return string.Join(" · ", parts);
    }

    public static string Duration(TimeSpan time) =>
        time.TotalHours >= 1 ? Strings.Format("PlayTimeHours", (int)time.TotalHours, time.Minutes)
        : Strings.Format("PlayTimeMinutes", Math.Max(1, (int)Math.Round(time.TotalMinutes)));

    private static string When(DateTimeOffset at, DateTimeOffset now)
    {
        var day = at.ToLocalTime().Date;
        var today = now.ToLocalTime().Date;
        if (day == today) return Strings.Get("PlayTimeToday");
        if (day == today.AddDays(-1)) return Strings.Get("PlayTimeYesterday");
        return day.ToString("d", CultureInfo.CurrentCulture);
    }
}
