using CommunityToolkit.Mvvm.ComponentModel;
using RevitTranslator.Revit.Core.Models;

namespace RevitTranslator.UI.Models;

public sealed partial class ViewViewModel : ObservableObject
{
    [ObservableProperty] private bool _isChecked;
    [ObservableProperty] private bool _isVisible = true;

    public required ViewDto Model { get; init; }
    public string Name => Model.Name;
    public int ElementCount => Model.ElementCount;
}
