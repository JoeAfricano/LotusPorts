using System.Globalization;
using System.Reflection;
using Microsoft.UI.Xaml.Markup;
using Microsoft.Windows.ApplicationModel.Resources;
using Serilog;

namespace LotusPorts.App;

/// <summary>Interface texts from Strings/{language}/Resources.resw. Portuguese is the default.</summary>
public static class Strings
{
    private static ResourceMap? map;
    private static ResourceContext? context;

    /// <summary>Language in use since startup. A new choice applies after restarting.</summary>
    public static string Language { get; private set; } = "pt-BR";

    public static void Initialize(string language)
    {
        Language = language;
        var manager = new ResourceManager();
        context = manager.CreateResourceContext();
        context.QualifierValues["Language"] = language;
        map = manager.MainResourceMap.GetSubtree("Resources");
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo(language);
        CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture = new CultureInfo(language);
    }

    public static string Get(string key)
    {
        if (map is null) Initialize("pt-BR");
        try
        {
            var value = map!.GetValue(key, context)?.ValueAsString;
            if (!string.IsNullOrEmpty(value)) return value;
        }
        catch (Exception ex) when (ex is ArgumentException or System.Runtime.InteropServices.COMException)
        {
        }
        Log.Warning("Missing interface text {Key}", key);
        return key;
    }

    public static string Format(string key, params object[] args) => string.Format(CultureInfo.CurrentCulture, Get(key), args);
}

/// <summary>XAML: Text="{local:Str Key=LibraryTitle}".</summary>
[MarkupExtensionReturnType(ReturnType = typeof(string))]
public sealed partial class Str : MarkupExtension
{
    public string Key { get; set; } = "";

    protected override object ProvideValue() => Strings.Get(Key);
}

public static class AppInfo
{
    public static string Version { get; } =
        typeof(AppInfo).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "?";
}

/// <summary>Screen ids used by the sidebar, the search and (later) deep links.</summary>
public static class Pages
{
    public const string Library = "library";
    public const string Catalog = "catalog";
    public const string Downloads = "downloads";
    public const string Lotus = "lotus";
    public const string Settings = "settings";
    public const string Help = "help";
    public const string Game = "game";
}
