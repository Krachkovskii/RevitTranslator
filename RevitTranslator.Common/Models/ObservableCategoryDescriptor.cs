using CommunityToolkit.Mvvm.ComponentModel;
using JetBrains.Annotations;

namespace RevitTranslator.Common.Models;

[UsedImplicitly]
public partial class ObservableCategoryDescriptor : ObservableObject
{
    [ObservableProperty] private bool _isChecked;

    public long Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public ObservableCategoryType CategoryType { get; set; } = null!;
    public bool IsBuiltInCategory { get; init; }
}