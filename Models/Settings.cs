using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Newtonsoft.Json.Converters;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using RevitTranslatorAddin.Utils;

namespace RevitTranslatorAddin.Models;

public class Settings
{
    public string DeeplApiKey { get; set; } = null;
    public string SourceLanguage { get; set; } = "English";
    public string TargetLanguage { get; set; } = "Italian";
    public List<string> IgnoreParameters { get; set; } = [];
    public List<string> IgnoreValues { get; set; } = [];
    public SortedList<string, string> Languages { get; set; } = DeeplLanguageCodes.LanguageCodes;
    private static string _jsonPath = string.Empty;

    public static Settings LoadFromJson()
    {
        if (_jsonPath == string.Empty)
        {
            _jsonPath = GetJsonPath();
        }

        try
        {
            var json = File.ReadAllText(_jsonPath);
            return JsonConvert.DeserializeObject<Settings>(json);
        }
        catch
        {
            return new Settings();
        }
    }

    public void SaveToJson()
    {
        var json = JsonConvert.SerializeObject(this, Formatting.Indented);
        File.WriteAllText(_jsonPath, json);
    }

    internal static string GetJsonPath()
    {
        string revitVersion = RevitUtils.App.VersionNumber;
        string roamingAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string addinName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
        string settingsDirectory = Path.Combine(roamingAppDataPath, "Autodesk", "Revit", "Addins", revitVersion, addinName, "Settings");

        Directory.CreateDirectory(settingsDirectory);

        string jsonPath = Path.Combine(settingsDirectory, "settings.json");
        return jsonPath;
    }
}
