using System.Text;
using System.Text.RegularExpressions;

namespace LotusPorts.Core.Guides;

public enum GuideBlockKind
{
    Heading,
    Paragraph,
    Bullet,
    Numbered,
    Code,
    Note,
}

/// <summary>A piece of text: plain, a link (only http/https), inline code or bold.</summary>
public sealed record GuideSpan(string Text, Uri? Link = null, bool Code = false, bool Bold = false);

public sealed record GuideBlock(GuideBlockKind Kind, IReadOnlyList<GuideSpan> Spans, int Number = 0, int Level = 0)
{
    public string PlainText => string.Concat(Spans.Select(s => s.Text));
}

/// <summary>The "how to play" part of a port's README, as the authors wrote it.</summary>
public sealed record PortGuide(IReadOnlyList<string> SectionTitles, IReadOnlyList<GuideBlock> Blocks);

/// <summary>
/// Finds the sections of a README that explain how to install and play (Quick Start, How to install, Usage…) and turns
/// their Markdown into simple blocks. Building from source, other systems (Linux, macOS, consoles) and images are left out.
/// Nothing is invented: when the README has no such section, there is no guide.
/// </summary>
public static partial class ReadmeGuide
{
    public const int MaxSections = 2;
    public const int MaxBlocks = 45;

    private sealed record Heading(int Line, int Level, string Title);

    public static PortGuide? Extract(string markdown, Uri baseUrl)
    {
        ArgumentNullException.ThrowIfNull(markdown);
        ArgumentNullException.ThrowIfNull(baseUrl);
        var lines = Clean(markdown).Split('\n');
        var headings = Headings(lines);

        var titles = new List<string>();
        var blocks = new List<GuideBlock>();
        var taken = new List<(int From, int To)>();
        foreach (var h in headings)
        {
            if (titles.Count >= MaxSections || blocks.Count >= MaxBlocks) break;
            if (taken.Any(t => h.Line >= t.From && h.Line < t.To)) continue;
            if (!UsageTitle().IsMatch(h.Title) || Excluded().IsMatch(h.Title)) continue;
            // FAQ questions and navigation bars ("Download · Installation guide · …") are not guides.
            if (h.Title.EndsWith('?') || h.Title.Contains('·') || h.Title.Contains(" | ", StringComparison.Ordinal)) continue;
            var end = headings.FirstOrDefault(n => n.Line > h.Line && n.Level <= h.Level)?.Line ?? lines.Length;
            var sectionBlocks = Blocks(lines, h.Line + 1, end, headings, baseUrl);
            if (sectionBlocks.Count(b => b.Kind != GuideBlockKind.Heading) == 0) continue;
            taken.Add((h.Line, end));
            titles.Add(h.Title);
            if (titles.Count > 1) blocks.Add(new GuideBlock(GuideBlockKind.Heading, [new GuideSpan(h.Title)]));
            blocks.AddRange(sectionBlocks.Take(MaxBlocks - blocks.Count));
        }
        return titles.Count == 0 ? null : new PortGuide(titles, blocks);
    }

    /// <summary>Line endings, HTML comments and HTML tags out; images out.</summary>
    private static string Clean(string markdown)
    {
        var text = markdown.Replace("\r\n", "\n").Replace('\r', '\n');
        text = HtmlComment().Replace(text, "");
        text = Image().Replace(text, "");
        text = HtmlImage().Replace(text, "");
        return text;
    }

    private static List<Heading> Headings(string[] lines)
    {
        var list = new List<Heading>();
        var fence = false;
        for (var i = 0; i < lines.Length; i++)
        {
            if (lines[i].TrimStart().StartsWith("```", StringComparison.Ordinal)) { fence = !fence; continue; }
            if (fence) continue;
            var m = HeadingLine().Match(lines[i]);
            if (m.Success) list.Add(new Heading(i, m.Groups[1].Length, Plain(m.Groups[2].Value)));
            else if (HtmlHeading().Match(lines[i]) is { Success: true } hm) list.Add(new Heading(i, int.Parse(hm.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture), Plain(hm.Groups[2].Value)));
        }
        return list;
    }

    private static List<GuideBlock> Blocks(string[] lines, int from, int to, List<Heading> headings, Uri baseUrl)
    {
        var blocks = new List<GuideBlock>();
        var paragraph = new StringBuilder();
        var code = new StringBuilder();
        var fence = false;
        var skipUntilLevel = 0;

        var item = new StringBuilder();
        var itemKind = GuideBlockKind.Bullet;
        var itemNumber = 0;
        var skipItem = false;

        void Flush()
        {
            if (paragraph.Length == 0) return;
            var text = paragraph.ToString().Trim();
            paragraph.Clear();
            if (text.Length > 0 && !PlatformStart().IsMatch(Plain(text)) && Inline(text, baseUrl) is { Count: > 0 } spans) blocks.Add(new GuideBlock(GuideBlockKind.Paragraph, spans));
        }

        void FlushItem()
        {
            if (item.Length == 0) return;
            var text = item.ToString().Trim();
            item.Clear();
            if (text.Length > 0 && Inline(text, baseUrl) is { Count: > 0 } spans) blocks.Add(new GuideBlock(itemKind, spans, itemNumber));
        }

        void FlushAll()
        {
            Flush();
            FlushItem();
        }

        // Items about other systems ("macOS: …", a Linux row of a table) are left out with their wrapped lines.
        void StartItem(GuideBlockKind kind, string text, int number)
        {
            FlushAll();
            skipItem = PlatformStart().IsMatch(Plain(text));
            if (skipItem) return;
            itemKind = kind;
            itemNumber = number;
            item.Append(text.Trim());
        }

        for (var i = from; i < to; i++)
        {
            var line = lines[i];
            if (line.TrimStart().StartsWith("```", StringComparison.Ordinal))
            {
                if (skipUntilLevel > 0) { fence = !fence; continue; }
                if (fence)
                {
                    if (code.Length > 0) blocks.Add(new GuideBlock(GuideBlockKind.Code, [new GuideSpan(code.ToString().TrimEnd(), Code: true)]));
                    code.Clear();
                }
                else FlushAll();
                fence = !fence;
                continue;
            }
            if (fence)
            {
                if (skipUntilLevel == 0 && !skipItem) code.Append(line).Append('\n');
                continue;
            }

            if (headings.FirstOrDefault(h => h.Line == i) is { } sub)
            {
                FlushAll();
                skipItem = false;
                if (skipUntilLevel > 0 && sub.Level > skipUntilLevel) continue;
                skipUntilLevel = 0;
                if (Excluded().IsMatch(sub.Title)) { skipUntilLevel = sub.Level; continue; }
                if (sub.Title.Length > 0) blocks.Add(new GuideBlock(GuideBlockKind.Heading, Inline(sub.Title, baseUrl), Level: sub.Level));
                continue;
            }
            if (skipUntilLevel > 0) continue;

            var trimmed = line.Trim();
            if (trimmed.Length == 0 || TableRule().IsMatch(trimmed) || trimmed is "---" or "***" || HtmlOnly().IsMatch(trimmed))
            {
                FlushAll();
                skipItem = false;
                continue;
            }
            if (BulletLine().Match(line) is { Success: true } b)
            {
                StartItem(GuideBlockKind.Bullet, b.Groups[1].Value, 0);
                continue;
            }
            if (NumberedLine().Match(line) is { Success: true } n)
            {
                StartItem(GuideBlockKind.Numbered, n.Groups[2].Value, int.Parse(n.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture));
                continue;
            }
            if (trimmed.StartsWith('>'))
            {
                FlushAll();
                var quote = AlertMarker().Replace(trimmed.TrimStart('>').Trim(), "");
                if (quote.Length > 0 && !PlatformStart().IsMatch(quote) && Inline(quote, baseUrl) is { Count: > 0 } spans) blocks.Add(new GuideBlock(GuideBlockKind.Note, spans));
                continue;
            }
            if (trimmed.StartsWith('|'))
            {
                var cells = trimmed.Trim('|').Split('|').Select(c => c.Trim()).Where(c => c.Length > 0).ToList();
                if (cells.Count > 0) StartItem(GuideBlockKind.Bullet, string.Join(" · ", cells), 0);
                FlushAll();
                continue;
            }
            // A wrapped line belongs to the list item above it (or is dropped with a left-out item).
            if (item.Length > 0) item.Append(' ').Append(trimmed);
            else if (!skipItem) paragraph.Append(paragraph.Length > 0 ? " " : "").Append(trimmed);
        }
        FlushAll();
        // Headings with nothing under them (everything was left out) are dropped; a heading followed by deeper ones stays.
        return blocks.Where((block, index) =>
        {
            if (block.Kind != GuideBlockKind.Heading) return true;
            if (StepTitle().IsMatch(block.PlainText)) return true; // "2. Download…" is a step even with nothing under it
            for (var j = index + 1; j < blocks.Count; j++)
            {
                if (blocks[j].Kind != GuideBlockKind.Heading) return true;
                if (blocks[j].Level <= block.Level) return false;
            }
            return false;
        }).ToList();
    }

    /// <summary>Links, inline code and bold; other Markdown marks and HTML tags are removed.</summary>
    public static List<GuideSpan> Inline(string text, Uri baseUrl)
    {
        ArgumentNullException.ThrowIfNull(baseUrl);
        var spans = new List<GuideSpan>();
        var position = 0;
        foreach (Match m in InlineToken().Matches(text))
        {
            if (m.Index > position) AddPlain(spans, text[position..m.Index]);
            if (m.Groups["code"].Success) spans.Add(new GuideSpan(m.Groups["code"].Value, Code: true));
            else if (m.Groups["bold"].Success) AddPlain(spans, m.Groups["bold"].Value, bold: true);
            else if (m.Groups["label"].Success)
            {
                var label = Plain(m.Groups["label"].Value);
                var link = Resolve(m.Groups["url"].Value, baseUrl);
                if (label.Length > 0) spans.Add(new GuideSpan(label, link));
            }
            else if (m.Groups["auto"].Success) spans.Add(new GuideSpan(m.Groups["auto"].Value, Resolve(m.Groups["auto"].Value, baseUrl)));
            position = m.Index + m.Length;
        }
        if (position < text.Length) AddPlain(spans, text[position..]);
        // Trim the edges so a block never starts or ends with stray spaces.
        if (spans.Count > 0 && spans[0] is { Code: false } first) spans[0] = first with { Text = first.Text.TrimStart() };
        if (spans.Count > 0 && spans[^1] is { Code: false } last) spans[^1] = last with { Text = last.Text.TrimEnd() };
        spans.RemoveAll(s => s.Text.Length == 0);
        return spans;
    }

    private static void AddPlain(List<GuideSpan> spans, string text, bool bold = false)
    {
        var plain = HtmlTag().Replace(text, "").Replace("\\", "", StringComparison.Ordinal);
        plain = Emphasis().Replace(plain, "$1");
        plain = System.Net.WebUtility.HtmlDecode(plain);
        if (plain.Length > 0) spans.Add(new GuideSpan(plain, Bold: bold));
    }

    private static Uri? Resolve(string url, Uri baseUrl)
    {
        url = url.Trim().Split(' ')[0].Trim('<', '>');
        if (url.StartsWith('#')) return null;
        // "/docs/x.md" on GitHub means the repository root, not the site root.
        if (url.StartsWith('/') && !url.StartsWith("//", StringComparison.Ordinal)) url = "." + url;
        if (!Uri.TryCreate(baseUrl, url, out var uri)) return null;
        if (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp) return null;
        // Never a clickable link to a game file (ROM, disc image, BIOS): the text stays, the link goes.
        return GameFileLink().IsMatch(uri.AbsolutePath) ? null : uri;
    }

    /// <summary>A heading's text without Markdown marks or links.</summary>
    private static string Plain(string text)
    {
        var t = LinkOnly().Replace(text, "$1");
        t = HtmlTag().Replace(t, "");
        t = t.Replace("**", "", StringComparison.Ordinal).Replace("__", "", StringComparison.Ordinal).Replace("`", "", StringComparison.Ordinal).Trim().TrimEnd('#').Trim();
        return System.Net.WebUtility.HtmlDecode(t);
    }

    [GeneratedRegex(@"(?i)\b(quick[\s-]?start|getting\s+started|get\s+started|how\s+to\s+(install|play|use|run|set\s?up|get\s+started|launch)|installation|installing|install|usage|playing|setup|set\s?up|first\s+(run|launch|time)|running\s+the\s+game|launching|launch\s+the\s+game|como\s+(jogar|instalar|usar))\b")]
    private static partial Regex UsageTitle();

    [GeneratedRegex(@"(?i)\b(build|building|compil\w*|develop\w*|contribut\w*|from\s+source|source\s+code|linux|mac\s?os|macos|osx|android|ios|switch|wii\s?u|ps\s?vita|vita|3ds|docker|nix|flatpak|appimage|steam\s?deck|arch|ubuntu|debian|fedora|emscripten|web\s?assembly|wasm|ai|disclosure)\b")]
    private static partial Regex Excluded();

    [GeneratedRegex(@"(?i)^\W*(the\s+|on\s+|for\s+)?(linux|mac\s?os|macos|osx|android|ios|steam\s?deck|switch|flatpak|appimage|docker|source\s+builds?)\b")]
    private static partial Regex PlatformStart();

    [GeneratedRegex(@"^\s*(step\s+)?\d{1,2}[.):]\s")]
    private static partial Regex StepTitle();

    [GeneratedRegex(@"^(#{1,6})\s+(.+?)\s*#*\s*$")]
    private static partial Regex HeadingLine();

    [GeneratedRegex(@"(?i)^\s*<h([1-6])[^>]*>(.*?)</h\1>\s*$")]
    private static partial Regex HtmlHeading();

    [GeneratedRegex(@"^\s*[-*+]\s+(?!\[[ xX]\]\s*$)(.+)$")]
    private static partial Regex BulletLine();

    [GeneratedRegex(@"^\s*(\d{1,3})[.)]\s+(.+)$")]
    private static partial Regex NumberedLine();

    [GeneratedRegex(@"^\|?\s*:?-{3,}:?\s*(\|\s*:?-{3,}:?\s*)*\|?$")]
    private static partial Regex TableRule();

    [GeneratedRegex(@"(?s)<!--.*?-->")]
    private static partial Regex HtmlComment();

    [GeneratedRegex(@"\[?!\[[^\]]*\]\([^)]*\)(\]\([^)]*\))?")]
    private static partial Regex Image();

    [GeneratedRegex(@"(?i)<img\b[^>]*>")]
    private static partial Regex HtmlImage();

    [GeneratedRegex(@"(?i)^(</?[a-z][^>]*>\s*)+$")]
    private static partial Regex HtmlOnly();

    [GeneratedRegex(@"<[^>]+>")]
    private static partial Regex HtmlTag();

    [GeneratedRegex(@"(?<![\w*])[*_]{1,2}([^*_\n]+?)[*_]{1,2}(?![\w*])")]
    private static partial Regex Emphasis();

    [GeneratedRegex(@"\[([^\]]*)\]\([^)]*\)")]
    private static partial Regex LinkOnly();

    [GeneratedRegex(@"(?i)\.(z64|n64|v64|iso|bin|cue|chd|cso|rom|sfc|smc|nes|fds|gb|gbc|gba|nds|3ds|cia|xex|xbe|wbfs|wad|rvz|gcm|gcz|nsp|xci|pbp|ecm|mdf|img|ccd|elf|bios)$")]
    private static partial Regex GameFileLink();

    [GeneratedRegex(@"(?i)^\[!(note|tip|important|warning|caution)\]\s*")]
    private static partial Regex AlertMarker();

    [GeneratedRegex(@"`(?<code>[^`]+)`|\*\*(?<bold>[^*]+)\*\*|\[(?<label>[^\]]+)\]\((?<url>[^)\s]+(?:\s+""[^""]*"")?)\)|(?<auto>https?://[^\s)<>\]]+)")]
    private static partial Regex InlineToken();
}
