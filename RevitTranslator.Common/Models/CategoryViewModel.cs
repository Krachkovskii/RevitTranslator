using CommunityToolkit.Mvvm.ComponentModel;
using JetBrains.Annotations;

namespace RevitTranslator.Common.Models;

[UsedImplicitly]
public partial class CategoryViewModel : ObservableObject
{
    [ObservableProperty] private bool _isChecked;
    [ObservableProperty] private bool _isVisible = true;
    
    partial void OnIsVisibleChanged(bool value)
    {
        Console.WriteLine($"CAT {Name} is visible changed to: {value}");
    }

    public long Id { get; init; }
    public int CategoryTypeEnum { get; init; }
    public string Name { get; init; } = string.Empty;
    public int ElementCount { get; init; }
    public bool IsBuiltInCategory { get; init; }
}