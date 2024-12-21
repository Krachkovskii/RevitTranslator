using CommunityToolkit.Mvvm.Input;

namespace RevitTranslator.UI.Contracts;

public interface ISettingsViewModel
{
    public string DeeplApiKey { get; set; }
    public string ButtonText { get; set; }
    public bool IsAutoDetectChecked { get; set; }
    public bool IsPaidPlan { get; set; }
    public int? SelectedSourceLanguageIndex { get; set; }
    public int SelectedTargetLanguageIndex { get; set; }

    public LanguageDescriptor[] Languages { get; }
    
    IRelayCommand OpenLinkedinCommand { get; }
    IAsyncRelayCommand SaveSettingsCommand { get; }
    IRelayCommand SwitchLanguagesCommand { get; }
}