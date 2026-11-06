using System.Globalization;
using System.Text;
using LotusPorts.Core.Diagnostics;
using LotusPorts.Core.Privacy;
using LotusPorts.Core.Verification;

namespace LotusPorts.App;

/// <summary>Plain words for the "Resolver" checks, and the report people paste in the port's community.</summary>
public static class ResolveText
{
    public static string Title(ResolveCheckKind kind) => Strings.Get("ResolveKind" + kind);

    public static string Explain(ResolveCheck check)
    {
        ArgumentNullException.ThrowIfNull(check);
        var facts = check.Facts.Select(f => (object)PathAnonymizer.Anonymize(f)).ToArray();
        return facts.Length == 0 ? Strings.Get("Resolve" + check.Reason) : Strings.Format("Resolve" + check.Reason, facts);
    }

    public static string State(CheckState state) => Strings.Get("Check" + state);

    public static string Action(ResolveAction action) => Strings.Get("ResolveAction" + action);

    /// <summary>Markdown for a forum or Discord post. User names in paths are replaced; nothing is sent anywhere by the app.</summary>
    public static string Report(ResolveReport report)
    {
        ArgumentNullException.ThrowIfNull(report);
        var sb = new StringBuilder();
        sb.AppendLine(CultureInfo.InvariantCulture, $"### Lotus Ports — {Strings.Get("ResolveTitle")}");
        sb.AppendLine();
        sb.AppendLine(CultureInfo.InvariantCulture, $"- **{report.Game}** · {report.Port} · {report.Version ?? "-"}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"- Windows {Environment.OSVersion.Version} · {report.At.ToLocalTime().ToString("g", CultureInfo.CurrentCulture)}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"- Lotus Ports {AppInfo.Version.Split('+')[0]}");
        sb.AppendLine();
        foreach (var check in report.Checks)
            sb.AppendLine(CultureInfo.InvariantCulture, $"- **{Title(check.Kind)}** — {State(check.State)}: {Explain(check)}");
        return PathAnonymizer.Anonymize(sb.ToString());
    }
}
