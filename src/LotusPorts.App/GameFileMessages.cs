using LotusPorts.Catalog;
using LotusPorts.Core.GameFiles;

namespace LotusPorts.App;

public sealed record GameFileMessage(string Title, string Body, bool Recognized, GameFileMatch? Match);

/// <summary>Plain words for what a file turned out to be (master prompt, section 8.1).</summary>
public static class GameFileMessages
{
    /// <param name="forGame">The game whose page the file was added on; null when dropped anywhere in the window.</param>
    public static GameFileMessage Describe(KnownGameFile file, LibraryGame? forGame)
    {
        ArgumentNullException.ThrowIfNull(file);
        var header = file.Info.Header is { InternalName.Length: > 0 } h
            ? " " + Strings.Format("GameFileHeader", h.InternalName, h.GameCode.Length > 0 ? h.GameCode : "?")
            : "";

        var mine = forGame is null ? file.Matches.FirstOrDefault() : file.Matches.FirstOrDefault(m => m.Game.Id == forGame.Id);
        if (mine is not null)
            return new GameFileMessage(Strings.Format("GameFileRecognizedTitle", mine.VersionLabel),
                Strings.Format("GameFileRecognizedBody", mine.Game.Game.Title, mine.VersionLabel, mine.MatchedBy) + " " + SetupHint(mine.Port, file.Info.Path)
                + (file.FromZip is { } zip ? " " + Strings.Format("GameFileFromZip", Path.GetFileName(zip)) : ""), true, mine);

        if (file.Matches.FirstOrDefault() is { } other)
            return new GameFileMessage(Strings.Get("GameFileOtherGameTitle"),
                Strings.Format("GameFileOtherGameBody", other.Game.Game.Title, other.VersionLabel), false, other);

        if (file.Info.Format == GameFileFormat.Unknown)
            return new GameFileMessage(Strings.Get("GameFileUnknownTitle"), Strings.Get("GameFileUnknownBody"), false, null);

        if (forGame is not null && !GameFileMatcher.FitsPlatform(file.Info.Format, forGame.Game.OriginalPlatform))
            return new GameFileMessage(Strings.Get("GameFileWrongConsoleTitle"),
                Strings.Format("GameFileWrongConsoleBody", CatalogLabels.Platform(forGame.Game.OriginalPlatform)) + header, false, null);

        if (file.Info.Hashes is null)
            return new GameFileMessage(Strings.Get("GameFileNotCheckedTitle"), Strings.Get("GameFileTooLargeBody") + header, false, null);

        var accepted = forGame is null ? [] : GameFileMatcher.AcceptedLabels(forGame.Recommended.Port);
        var body = accepted.Count > 0
            ? Strings.Format("GameFileOtherVersionBody", string.Join(", ", accepted))
            : Strings.Get("GameFileNoListBody");
        return new GameFileMessage(Strings.Get("GameFileNotCheckedTitle"), body + header, false, null);
    }

    /// <summary>What to do with a recognized file, by how the port uses it (setup.kind).</summary>
    public static string SetupHint(Port port, string path)
    {
        ArgumentNullException.ThrowIfNull(port);
        return port.Setup.Kind switch
        {
            "select-on-first-launch" => Strings.Format("GameFileHintSelect", path),
            "beside-exe" => Strings.Get("GameFileHintBeside"),
            _ => port.Setup.NotesPtBr ?? Strings.Get("GameFileHintDocs"),
        };
    }
}
