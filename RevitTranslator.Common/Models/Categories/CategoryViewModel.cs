using CommunityToolkit.Mvvm.ComponentModel;
using JetBrains.Annotations;

namespace RevitTranslator.Common.Models.Categories;

[UsedImplicitly]
public sealed partial class CategoryViewModel : ObservableObject
{
    [ObservableProperty] private bool _isChecked;
    [ObservableProperty] private bool _isVisible = true;

    public long Id { get; init; }
    public int CategoryTypeEnum { get; init; }
    public string Name { get; init; } = string.Empty;
    public int ElementCount { get; init; }
    public bool IsBuiltInCategory { get; init; }
}