using System.Text.RegularExpressions;

namespace LotusPorts.Catalog;

public enum SourceAddressKind
{
    /// <summary>https://github.com/{owner}/{repo}: catalog.json is read from the default branch.</summary>
    Repository,

    /// <summary>https://raw.githubusercontent.com/{owner}/{repo}/{ref}/.../catalog.json</summary>
    RawFile,
}

/// <summary>The only kinds of link accepted for catalog sources and "add port by link".</summary>
public sealed partial record SourceAddress(SourceAddressKind Kind, string Owner, string Repo, Uri? RawUrl)
{
    public static bool TryParse(string? input, out SourceAddress? address)
    {
        address = null;
        if (string.IsNullOrWhiteSpace(input) || !Uri.TryCreate(input.Trim(), UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps) return false;
        var parts = uri.AbsolutePath.Trim('/').Split('/');
        if (uri.Host.Equals("github.com", StringComparison.OrdinalIgnoreCase) && parts.Length >= 2 && Name().IsMatch(parts[0]) && Name().IsMatch(parts[1]))
        {
            address = new(SourceAddressKind.Repository, parts[0], parts[1].EndsWith(".git", StringComparison.OrdinalIgnoreCase) ? parts[1][..^4] : parts[1], null);
            return parts.Length == 2;
        }
        if (uri.Host.Equals("raw.githubusercontent.com", StringComparison.OrdinalIgnoreCase) && parts.Length >= 4 && Name().IsMatch(parts[0]) && Name().IsMatch(parts[1])
            && parts[^1].Equals("catalog.json", StringComparison.OrdinalIgnoreCase) && string.IsNullOrEmpty(uri.Query))
        {
            address = new(SourceAddressKind.RawFile, parts[0], parts[1], uri);
            return true;
        }
        return false;
    }

    public string RepoUrl => $"https://github.com/{Owner}/{Repo}";

    [GeneratedRegex("^[A-Za-z0-9._-]{1,100}$")]
    private static partial Regex Name();
}
