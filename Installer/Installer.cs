using WixSharp;

namespace Installer;

public static class Installer
{
    private static readonly int[] VersionList = [23, 24, 25];
    private static readonly string MainDirectoryPath = GetMainDirectory();

    public static void Main()
    {
        foreach (var version in VersionList)
        {
            var configDir = GetConfigurationPath(version);
                
            var assemblies = new Files(configDir);
            var manifest = new Files($@"{MainDirectoryPath}\{Constants.ProjectName}.addin");
            
            var project = new Project
            {
                OutDir = "Installer",
                OutFileName = $"Revit_Translator_V{version}",
                Name = $"Revit Translator for Revit 20{version}",
                Platform = Platform.x64,
                InstallScope = InstallScope.perMachine,
                UI = WUI.WixUI_ProgressOnly,
                MajorUpgrade = MajorUpgrade.Default,
                GUID = new Guid($"46A798D3-104B-2E3A-8126-CE3A212C97{version}"),
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
}