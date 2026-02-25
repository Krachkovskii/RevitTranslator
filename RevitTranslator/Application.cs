using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xaml.Behaviors;
using Nice3point.Revit.Toolkit.External;
using Nice3point.Revit.Toolkit.External.Handlers;
using RevitTranslator.Services;
using TranslationService.Utils;
using TriggerBase = Microsoft.Xaml.Behaviors.TriggerBase;

namespace RevitTranslator;
/// <summary>
///     Application entry point
/// </summary>
[UsedImplicitly]
public class Application : ExternalApplication
{
    public override void OnStartup()
    {
        _ = new Host();
        var ribbonService = Host.ServiceProvider.GetRequiredService<RibbonService>();
        ribbonService.CreateRibbonPanel(Application);
        FixBehaviors();
        Host.ServiceProvider.GetRequiredService<UpdaterService>().StartCheckLoop();
        ValidateSettingsOnStartupAsync(ribbonService);
    }

    private static async void ValidateSettingsOnStartupAsync(RibbonService ribbonService)
    {
        try
        {
            var client = Host.ServiceProvider.GetRequiredService<DeeplTranslationClient>();
            var isValid = await DeeplSettingsUtils.ValidateAsync(client);
            ribbonService.ChangeRevitButtonsState(isValid);
        }
        catch
        {
            // Buttons remain disabled if validation fails unexpectedly
        }
    }

    public override void OnShutdown()
    {
        Host.ServiceProvider.GetRequiredService<UpdaterService>().TriggerDelayedInstall();
    }

    private static void FixBehaviors()
    {
        //https://github.com/microsoft/XamlBehaviorsWpf/issues/86
        _ = new DefaultTriggerAttribute(typeof(Trigger), typeof(TriggerBase), null);
    }
}
