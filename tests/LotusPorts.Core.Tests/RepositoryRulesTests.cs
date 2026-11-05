using System.Text.RegularExpressions;

namespace LotusPorts.Core.Tests;

/// <summary>Project rules checked on the source tree itself.</summary>
public partial class RepositoryRulesTests
{
    private static string RepositoryRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "LotusPorts.slnx"))) dir = dir.Parent;
        return dir?.FullName ?? throw new InvalidOperationException("Repository root not found.");
    }

    [Fact]
    public void App_views_use_design_tokens_not_loose_colors()
    {
        var views = Directory.EnumerateFiles(Path.Combine(RepositoryRoot(), "src", "LotusPorts.App"), "*.xaml", SearchOption.AllDirectories)
            .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}") && !f.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"));

        var offenders = views
            .SelectMany(f => File.ReadAllLines(f).Select((line, i) => (File: Path.GetFileName(f), Line: i + 1, Text: line)))
            .Where(x => HexColor().IsMatch(x.Text))
            .Select(x => $"{x.File}:{x.Line}")
            .ToList();

        offenders.Should().BeEmpty("colors come from Lotus.Design tokens (docs/UX.md)");
    }

    [Fact]
    public void Interface_texts_exist_in_both_languages()
    {
        var root = Path.Combine(RepositoryRoot(), "src", "LotusPorts.App", "Strings");
        HashSet<string> Keys(string lang) => DataName().Matches(File.ReadAllText(Path.Combine(root, lang, "Resources.resw"))).Select(m => m.Groups[1].Value).ToHashSet();

        Keys("pt-BR").Should().BeEquivalentTo(Keys("en-US"));
    }

    [GeneratedRegex("#[0-9A-Fa-f]{6,8}\\b")]
    private static partial Regex HexColor();

    [GeneratedRegex("<data name=\"([^\"]+)\"")]
    private static partial Regex DataName();
}
