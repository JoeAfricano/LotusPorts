using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace LotusPorts.Core.Styles;

/// <summary>
/// Writes a style's declared keys into a port's configuration text, leaving everything else as it was
/// (master prompt, section 5.6). JSON keys are dotted paths ("graphics.resolution"); INI keys are "Section.Key"
/// (a key without a dot lives before the first section). Returns what each key held before, so it can be undone.
/// </summary>
public static class PresetWriter
{
    public static bool Supports(string? format) => format is "json" or "ini";

    /// <summary>Applies <paramref name="values"/> and returns the new text and the previous value of each key (null = the key did not exist).</summary>
    public static (string Text, IReadOnlyDictionary<string, string?> Previous) Apply(string text, string format, IReadOnlyDictionary<string, JsonElement> values)
    {
        ArgumentNullException.ThrowIfNull(values);
        var wanted = values.ToDictionary(kv => kv.Key, kv => (string?)kv.Value.GetRawText());
        return format switch
        {
            "json" => Json(text, wanted),
            "ini" => Ini(text, wanted.ToDictionary(kv => kv.Key, kv => kv.Value is null ? null : IniValue(values[kv.Key]))),
            _ => throw new NotSupportedException($"Configuration format '{format}' is not supported."),
        };
    }

    /// <summary>Puts back the previous values (removing keys that did not exist before). Other changes stay.</summary>
    public static string Revert(string text, string format, IReadOnlyDictionary<string, string?> previous)
    {
        ArgumentNullException.ThrowIfNull(previous);
        return format switch
        {
            "json" => Json(text, previous.ToDictionary(kv => kv.Key, kv => kv.Value)).Text,
            "ini" => Ini(text, previous.ToDictionary(kv => kv.Key, kv => kv.Value)).Text,
            _ => throw new NotSupportedException($"Configuration format '{format}' is not supported."),
        };
    }

    /// <summary>True when every declared key reads back with the declared value.</summary>
    public static bool Verify(string text, string format, IReadOnlyDictionary<string, JsonElement> values)
    {
        ArgumentNullException.ThrowIfNull(values);
        var current = Read(text, format, values.Keys);
        return values.All(kv => current.TryGetValue(kv.Key, out var v) && v is not null
            && (format == "json" ? JsonEquals(v, kv.Value.GetRawText()) : v == IniValue(kv.Value)));
    }

    public static IReadOnlyDictionary<string, string?> Read(string text, string format, IEnumerable<string> keys) => format switch
    {
        "json" => keys.ToDictionary(k => k, k => Find(Parse(text), k)?.ToJsonString()),
        "ini" => keys.ToDictionary(k => k, k => IniRead(text, k)),
        _ => throw new NotSupportedException($"Configuration format '{format}' is not supported."),
    };

    // ---- JSON --------------------------------------------------------------------------------

    private static JsonObject Parse(string text) =>
        string.IsNullOrWhiteSpace(text) ? [] : JsonNode.Parse(text, documentOptions: new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true }) as JsonObject
            ?? throw new InvalidDataException("The configuration is not a JSON object.");

    private static JsonNode? Find(JsonObject root, string path)
    {
        JsonNode? node = root;
        foreach (var part in path.Split('.'))
        {
            if (node is not JsonObject o || !o.TryGetPropertyValue(part, out node)) return null;
        }
        return node;
    }

    /// <summary>Sets (or removes, for null) each dotted key; returns the previous raw JSON of each.</summary>
    private static (string Text, IReadOnlyDictionary<string, string?> Previous) Json(string text, Dictionary<string, string?> changes)
    {
        var root = Parse(text);
        var previous = new Dictionary<string, string?>();
        foreach (var (path, raw) in changes)
        {
            var parts = path.Split('.');
            var parent = root;
            for (var i = 0; i < parts.Length - 1; i++)
            {
                if (parent[parts[i]] is not JsonObject child)
                {
                    if (raw is null) { parent = null; break; }
                    child = [];
                    parent[parts[i]] = child;
                }
                parent = child;
            }
            var leaf = parts[^1];
            previous[path] = parent is not null && parent.TryGetPropertyValue(leaf, out var old) ? old?.ToJsonString() : null;
            if (parent is null) continue;
            if (raw is null) parent.Remove(leaf);
            else parent[leaf] = JsonNode.Parse(raw);
        }
        return (root.ToJsonString(new JsonSerializerOptions { WriteIndented = true }) + "\n", previous);
    }

    private static bool JsonEquals(string a, string b) => JsonNode.DeepEquals(JsonNode.Parse(a), JsonNode.Parse(b));

    // ---- INI ---------------------------------------------------------------------------------

    private static string IniValue(JsonElement value) => value.ValueKind switch
    {
        JsonValueKind.String => value.GetString()!,
        JsonValueKind.True => "true",
        JsonValueKind.False => "false",
        _ => value.GetRawText(),
    };

    private static (string Section, string Key) Split(string path)
    {
        var dot = path.IndexOf('.');
        return dot < 0 ? ("", path) : (path[..dot], path[(dot + 1)..]);
    }

    private static string? IniRead(string text, string path)
    {
        var (section, key) = Split(path);
        var current = "";
        foreach (var raw in text.Replace("\r\n", "\n").Split('\n'))
        {
            var line = raw.Trim();
            if (line.StartsWith('[') && line.EndsWith(']')) { current = line[1..^1].Trim(); continue; }
            if (!string.Equals(current, section, StringComparison.OrdinalIgnoreCase) || line.StartsWith(';') || line.StartsWith('#')) continue;
            var eq = line.IndexOf('=');
            if (eq > 0 && string.Equals(line[..eq].Trim(), key, StringComparison.OrdinalIgnoreCase)) return line[(eq + 1)..].Trim();
        }
        return null;
    }

    /// <summary>Edits INI lines in place: comments, order and other keys are kept; missing keys/sections are added.</summary>
    private static (string Text, IReadOnlyDictionary<string, string?> Previous) Ini(string text, Dictionary<string, string?> changes)
    {
        var newline = text.Contains("\r\n", StringComparison.Ordinal) ? "\r\n" : "\n";
        var lines = text.Length == 0 ? [] : text.Replace("\r\n", "\n").TrimEnd('\n').Split('\n').ToList();
        var previous = new Dictionary<string, string?>();
        foreach (var (path, value) in changes)
        {
            var (section, key) = Split(path);
            previous[path] = IniRead(string.Join("\n", lines), path);
            var current = "";
            int sectionEnd = section.Length == 0 ? FirstSection(lines) : -1, found = -1;
            for (var i = 0; i < lines.Count; i++)
            {
                var line = lines[i].Trim();
                if (line.StartsWith('[') && line.EndsWith(']'))
                {
                    if (string.Equals(current, section, StringComparison.OrdinalIgnoreCase) && section.Length > 0) sectionEnd = i;
                    current = line[1..^1].Trim();
                    continue;
                }
                if (!string.Equals(current, section, StringComparison.OrdinalIgnoreCase)) continue;
                if (section.Length > 0) sectionEnd = i + 1;
                var eq = line.IndexOf('=');
                if (eq > 0 && !line.StartsWith(';') && !line.StartsWith('#') && string.Equals(line[..eq].Trim(), key, StringComparison.OrdinalIgnoreCase)) found = i;
            }
            if (found >= 0)
            {
                if (value is null) lines.RemoveAt(found);
                else lines[found] = $"{key}={value}";
            }
            else if (value is not null)
            {
                if (sectionEnd < 0)
                {
                    if (section.Length > 0) lines.Add($"[{section}]");
                    lines.Add($"{key}={value}");
                }
                else lines.Insert(sectionEnd, $"{key}={value}");
            }
        }
        return (string.Join(newline, lines) + newline, previous);
    }

    private static int FirstSection(List<string> lines)
    {
        var index = lines.FindIndex(l => l.Trim().StartsWith('['));
        return index < 0 ? lines.Count : index;
    }
}
