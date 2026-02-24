using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using NetSparkleUpdater;
using NetSparkleUpdater.Enums;
using NetSparkleUpdater.Events;
using NetSparkleUpdater.SignatureVerifiers;

namespace RevitTranslator.Services;

public sealed class UpdaterService : IDisposable
{
#if REVIT2023
    private const string AppCastUrl = "https://raw.githubusercontent.com/Krachkovskii/RevitTranslator/main/appcast-R23.xml";
#elif REVIT2024
    private const string AppCastUrl = "https://raw.githubusercontent.com/Krachkovskii/RevitTranslator/main/appcast-R24.xml";
#elif REVIT2025
    private const string AppCastUrl = "https://raw.githubusercontent.com/Krachkovskii/RevitTranslator/main/appcast-R25.xml";
#else
    private const string AppCastUrl = "https://raw.githubusercontent.com/Krachkovskii/RevitTranslator/main/appcast-R26.xml";
#endif
    private const string MsiExtension = ".msi";

    private readonly SparkleUpdater _sparkle;
    private string? _pendingMsiPath;

    public UpdaterService()
    {
        _sparkle = new SparkleUpdater(
            AppCastUrl,
            new Ed25519Checker(SecurityMode.Unsafe),
            Assembly.GetExecutingAssembly().Location)
        {
            UIFactory = null,
            RelaunchAfterUpdate = false,
            ShowsUIOnMainThread = false,
        };

        _sparkle.UpdateDetected += OnUpdateDetected;
        _sparkle.DownloadFinished += OnDownloadFinished;
        _sparkle.DownloadHadError += OnDownloadHadError;
    }

    public bool HasPendingUpdate => _pendingMsiPath is not null;

    public void StartCheckLoop() => _sparkle.StartLoop(true);

    public void TriggerDelayedInstall()
    {
        if (_pendingMsiPath is null) return;
        LaunchDelayedInstallScript(_pendingMsiPath);
    }

    private async void OnUpdateDetected(object sender, UpdateDetectedEventArgs args)
    {
        try
        {
            await _sparkle.InitAndBeginDownload(args.LatestVersion);
        }
        catch (Exception e)
        {
            throw; // TODO handle exception
        }
    }

    private void OnDownloadFinished(AppCastItem item, string path)
    {
        try
        {
            if (path.EndsWith(MsiExtension, StringComparison.OrdinalIgnoreCase))
            {
                _pendingMsiPath = path;
                return;
            }

            var msiPath = $"{path}{MsiExtension}";
            if (File.Exists(msiPath)) File.Delete(msiPath);
            File.Move(path, msiPath);
            _pendingMsiPath = msiPath;
        }
        catch
        {
            // Silently ignore — update will be attempted next session
        }
    }

    private void OnDownloadHadError(AppCastItem item, string? path, Exception exception)
    {
        // Silently ignore — update will be attempted next session
    }

    private static void LaunchDelayedInstallScript(string msiPath)
    {
        try
        {
            var scriptContent =
                "Start-Sleep -Seconds 120\r\n" +
                $"Start-Process msiexec -ArgumentList \"/i `\"{msiPath}`\" /quiet /norestart\" -Wait";

            var scriptPath = Path.Combine(Path.GetTempPath(), "RevitTranslatorUpdate.ps1");
            File.WriteAllText(scriptPath, scriptContent);

            Process.Start(new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-ExecutionPolicy Bypass -NonInteractive -WindowStyle Hidden -File \"{scriptPath}\"",
                UseShellExecute = false,
                CreateNoWindow = true,
            });
        }
        catch
        {
            // Silently ignore — update will be attempted next session
        }
    }

    public void Dispose()
    {
        _sparkle.UpdateDetected -= OnUpdateDetected;
        _sparkle.DownloadFinished -= OnDownloadFinished;
        _sparkle.DownloadHadError -= OnDownloadHadError;
        _sparkle.Dispose();
    }
}
