using Nice3point.Revit.Toolkit.External;
using RevitTranslatorAddin.Utils.App;
using RevitTranslatorAddin.Utils.DeepL;
using RevitTranslatorAddin.Utils.Revit;

namespace RevitTranslatorAddin.Commands;
public class BaseCommand : ExternalCommand
{
    protected ProgressWindowUtils _progressWindowUtils = null;
    protected Models.DeeplSettings _settings = null;
    protected TranslationUtils _translationUtils = null;

    /// <summary>
    /// Creates and sets all necessary utils, i.e. progress window, translation etc.
    /// </summary>
    protected void CreateAndSetUtils()
    {
        _settings = Models.DeeplSettings.LoadFromJson();
        _progressWindowUtils = new ProgressWindowUtils();
        ElementUpdateHandler.ProgressWindowUtils = _progressWindowUtils;
        _translationUtils = new TranslationUtils(_settings, _progressWindowUtils);
    }

    public override void Execute() => throw new NotImplementedException();
}
