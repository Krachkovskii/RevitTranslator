using CommunityToolkit.Mvvm.Input;
using TranslationService.Models;

namespace RevitTranslator.UI.Contracts;

public interface ISettingsViewModel
{
    public string DeeplApiKey { get; set; }
    public string ButtonText { get; set; }
    public bool IsAutoDetectChecked { get; set; }
    public bool IsPaidPlan { get; set; }
    public LanguageDescriptor? SelectedSourceLanguage { get; set; }
    public LanguageDescriptor SelectedTargetLanguage { get; set; }

    IRelayCommand OpenLinkedinCommand { get; }
    IRelayCommand SaveSettingsCommand { get; }
    IRelayCommand SwitchLanguagesCommand { get; }
}