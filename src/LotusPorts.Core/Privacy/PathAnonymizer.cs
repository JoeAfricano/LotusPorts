namespace LotusPorts.Core.Privacy;

/// <summary>Removes the Windows user name from text before it reaches logs, the journal or a copied report.</summary>
public static class PathAnonymizer
{
    public static string Anonymize(string text) =>
        Anonymize(text, Environment.UserName, Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));

    public static string Anonymize(string text, string? userName, string? userProfile)
    {
        ArgumentNullException.ThrowIfNull(text);
        if (!string.IsNullOrEmpty(userProfile))
            text = text.Replace(userProfile.TrimEnd('\\'), "%USERPROFILE%", StringComparison.OrdinalIgnoreCase);
        if (!string.IsNullOrEmpty(userName) && userName.Length > 1)
            text = text.Replace(@"\" + userName + @"\", @"\<user>\", StringComparison.OrdinalIgnoreCase);
        return text;
    }
}
