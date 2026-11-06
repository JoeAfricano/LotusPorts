using System.IO.Compression;

namespace LotusPorts.Core.GameFiles;

/// <summary>
/// Game files the player keeps inside a .zip (the usual way people store their dumps). The .zip is only read: the game
/// files in it are copied out to a folder of Lotus Ports, because ports need the plain file (a .z64, an .iso…).
/// </summary>
public static class GameFileZip
{
    public const int MaxCandidates = 4;

    public static bool IsZip(string path) => string.Equals(Path.GetExtension(path), ".zip", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Copies the entries that look like game files (by extension, at most <paramref name="maxBytes"/> each and
    /// <see cref="MaxCandidates"/> in all) into <paramref name="folder"/>, keeping only the file name. A copy that is
    /// already there with the same size is reused. Returns the copied paths (empty: nothing in it looked like a game file).
    /// </summary>
    public static async Task<IReadOnlyList<string>> ExtractCandidatesAsync(string zip, string folder, long maxBytes, CancellationToken cancellationToken = default)
    {
        using var archive = ZipFile.OpenRead(zip);
        var entries = archive.Entries
            .Where(e => e.Name.Length > 0 && GameFileReader.IsCandidate(e.Name) && e.Length > 0 && e.Length <= maxBytes)
            .Take(MaxCandidates)
            .ToList();
        var result = new List<string>();
        if (entries.Count == 0) return result;
        Directory.CreateDirectory(folder);
        foreach (var entry in entries)
        {
            cancellationToken.ThrowIfCancellationRequested();
            // Only the file name: folders inside the .zip (or "..") never decide where the copy goes.
            var target = Path.Combine(folder, Path.GetFileName(entry.Name));
            if (File.Exists(target) && new FileInfo(target).Length == entry.Length) { result.Add(target); continue; }
            var temp = target + ".part";
            await using (var input = entry.Open())
            await using (var output = new FileStream(temp, FileMode.Create, FileAccess.Write, FileShare.None, 1 << 16, useAsync: true))
                await input.CopyToAsync(output, cancellationToken).ConfigureAwait(false);
            File.Move(temp, target, overwrite: true);
            result.Add(target);
        }
        return result;
    }
}
