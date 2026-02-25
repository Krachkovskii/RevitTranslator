using WixSharp;
using File = System.IO.File;

namespace Installer;

public static class Installer
{
    private static readonly int[] VersionList = [23, 24, 25, 26];
    private static readonly string MainDirectoryPath = GetMainDirectory();

    public static void Main()
    {
        foreach (var version in VersionList)
        {
            var configDir = GetConfigurationPath(version);
            if (!AreBinariesFresh(configDir)) throw new Exception($"Binaries for Revit 20{version} are not fresh.");
                
            var assemblies = new DirFiles($@"{configDir}\*.*");
            var manifest = new WixSharp.File($@"{MainDirectoryPath}\{Constants.ProjectName}.addin");
            
            var project = new Project
            {
                OutDir = "Installer",
                OutFileName = $"Revit_Translator_V{version}",
                Name = $"Revit Translator for Revit 20{version}",
                Platform = Platform.x64,
                InstallScope = InstallScope.perUser,
                UI = WUI.WixUI_ProgressOnly,
                MajorUpgrade = new MajorUpgrade
                {
                    AllowSameVersionUpgrades = true,
                    DowngradeErrorMessage = $"A newer version of Revit Translator for Revit 20{version} is already installed. Please uninstall it before installing an older version.",
                    Schedule = UpgradeSchedule.afterInstallInitialize
                },
                UpgradeCode = new Guid($"46A798D3-104B-2E3A-8126-CE3A212C98{version}"),
                Version = new Version(Constants.Version),
                ControlPanelInfo =
                {
                    Manufacturer = "Ilia Krachkovskii",
                },
                Dirs =
                [
                    new Dir(@$"%AppData%\Autodesk\Revit\Addins\20{version}\",
                        new Dir($@"{Constants.ProjectName}\", assemblies),
                        manifest
                        )
                ]
            };

            project.BuildMsi();
        }
    }
    
    private static string GetMainDirectory()
    {
        var currentDir = Directory.GetCurrentDirectory();
        var path = currentDir;
        
        while (true)
        {
            foreach (var dir in Directory.GetDirectories(path))
            {
                if (dir.EndsWith(Constants.ProjectName)) return dir;
            }

            var parentDir = Directory.GetParent(path);
            if (parentDir == null) throw new DirectoryNotFoundException("Could not find project directory");

            path = parentDir.FullName;
        }
    }

    private static string GetConfigurationPath(int version)
    {
        return Directory.GetDirectories($@"{MainDirectoryPath}\bin\")
            .First(dir => dir.EndsWith($"Release R{version}"));
    }

    /// <summary>
    /// To avoid using old binaries, if main executable was not updated in the last 30 minutes, installer will not be created
    /// </summary>
    /// <param name="configDir"></param>
    /// <returns></returns>
    private static bool AreBinariesFresh(string configDir)
    {
        if (Environment.GetEnvironmentVariable("CI") == "true") return true;
        var publishTime = File.GetLastWriteTime(Path.Combine(configDir, "RevitTranslator.dll"));
        return publishTime > DateTime.Now.AddMinutes(-30);
    }
}