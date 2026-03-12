namespace RevitTranslator.Revit.Core.Contracts;

public interface ISettingsValidator
{
    Task<bool> TryEnforceValidSettingsAsync();
}
