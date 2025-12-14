using CommunityToolkit.Mvvm.Input;

namespace RevitTranslator.UI.Contracts;

public interface IProgressWindowViewModel
{
    int TotalTranslationCount { get; set; }
    int FinishedTranslationCount { get; set; }
    int MonthlyCharacterLimit { get; set; }
    int MonthlyCharacterCount { get; set; }
    int SessionCharacterCount { get; set; }
    string ButtonText { get; set; }
    bool IsProgressBarIntermediate { get; set; }
    
    void UpdateProgress(int translationLength);
    void CloseRequested();
    
    IRelayCommand CancelTranslationCommand { get; }
    IAsyncRelayCommand LoadedCommand { get; }
}