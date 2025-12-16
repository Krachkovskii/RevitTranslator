using CommunityToolkit.Mvvm.ComponentModel;

namespace RevitTranslator.Common.Models.Views;

public sealed partial class ViewViewModel : ObservableObject
{
    [ObservableProperty] private bool _isChecked;
    [ObservableProperty] private bool _isVisible = true;
    
    public required ViewDto Model { get; init; }
    public string Name => Model.Name;
}