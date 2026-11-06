using System.IO.Compression;

namespace LotusPorts.Infrastructure.Install;

public enum ExtractionProblem
{
    /// <summary>An entry would land outside the version folder (zip-slip).</summary>
    OutsideFolder,

    /// <summary>The unpacked size or the number of files is above the limit.</summary>
    TooLarge,

    /// <summary>The file is not a readable zip.</summary>
    NotAnArchive,
}

public sealed class ExtractionException(ExtractionProblem problem, string message) : IOException(message)
{
    public ExtractionProblem Problem { get; } = problem;
}

public sealed record ExtractionLimits(long MaxBytes, int MaxEntries)
{
    /// <summary>32 GB and 200 000 files: far above any port, low enough to stop a zip bomb.</summary>
    public static ExtractionLimits Default { get; } = new(32L * 1024 * 1024 * 1024, 200_000);
}

/// <summary>
/// Unpacks a release zip into a new folder: no entry may escape the folder, links are skipped, the unpacked size is
/// limited (also counted while writing, so a lying header does not help) and a single top folder is flattened.
/// </summary>
public static class SafeExtractor
{
    private const int UnixFileTypeMask = 0xF000;
    private const int UnixSymlink = 0xA000;
    private const int WindowsReparsePoint = 0x400;

    /// <returns>Number of files written.</returns>
    public static int ExtractZip(string zipFile, string destination, ExtractionLimits? limits = null, IProgress<double>? progress = null, CancellationToken cancellationToken = default)
    {
        limits ??= ExtractionLimits.Default;
        ZipArchive archive;
        try { archive = ZipFile.OpenRead(zipFile); }
        catch (InvalidDataException ex) { throw new ExtractionException(ExtractionProblem.NotAnArchive, ex.Message); }

        using (archive)
        {
            var entries = archive.Entries;
            if (entries.Count > limits.MaxEntries) throw new ExtractionException(ExtractionProblem.TooLarge, "Too many files.");
            long announced = entries.Sum(e => e.Length);
            if (announced > limits.MaxBytes) throw new ExtractionException(ExtractionProblem.TooLarge, "Unpacked size is above the limit.");

            var root = Path.GetFullPath(destination);
            var rootWithSlash = root.EndsWith(Path.DirectorySeparatorChar) ? root : root + Path.DirectorySeparatorChar;
            Directory.CreateDirectory(root);
            var prefix = CommonTopFolder(entries);
            long written = 0;
            var files = 0;
            var buffer = new byte[1 << 16];

            foreach (var entry in entries)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (IsLink(entry)) continue;
                var name = entry.FullName.Replace('\\', '/');
                if (prefix is not null) name = name[prefix.Length..];
                if (name.Length == 0) continue;

                var target = Path.GetFullPath(Path.Combine(root, name));
                if (!target.StartsWith(rootWithSlash, StringComparison.OrdinalIgnoreCase) && !string.Equals(target.TrimEnd(Path.DirectorySeparatorChar), root, StringComparison.OrdinalIgnoreCase))
                    throw new ExtractionException(ExtractionProblem.OutsideFolder, "An entry points outside the folder.");

                if (name.EndsWith('/'))
                {
                    Directory.CreateDirectory(target);
                    continue;
                }
                Directory.CreateDirectory(Path.GetDirectoryName(target)!);
                using (var input = entry.Open())
                using (var output = new FileStream(target, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                {
                    int read;
                    while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        written += read;
                        if (written > limits.MaxBytes) throw new ExtractionException(ExtractionProblem.TooLarge, "Unpacked size is above the limit.");
                        output.Write(buffer, 0, read);
                    }
                }
                files++;
                if (announced > 0) progress?.Report(Math.Min(1, (double)written / announced));
            }
            return files;
        }
    }

    /// <summary>"Game-v1.0/…" for every entry: the folder is dropped so the program sits at the top of the version folder.</summary>
    private static string? CommonTopFolder(IReadOnlyCollection<ZipArchiveEntry> entries)
    {
        string? top = null;
        foreach (var entry in entries)
        {
            if (IsLink(entry)) continue;
            var name = entry.FullName.Replace('\\', '/');
            var slash = name.IndexOf('/');
            if (slash <= 0) return null; // a file at the top: nothing to flatten
            var first = name[..(slash + 1)];
            if (first is "../" or "./") return null;
            if (top is null) top = first;
            else if (!string.Equals(top, first, StringComparison.Ordinal)) return null;
        }
        return top;
    }

    private static bool IsLink(ZipArchiveEntry entry)
    {
        var unixMode = (entry.ExternalAttributes >> 16) & UnixFileTypeMask;
        return unixMode == UnixSymlink || (entry.ExternalAttributes & WindowsReparsePoint) != 0;
    }
}
