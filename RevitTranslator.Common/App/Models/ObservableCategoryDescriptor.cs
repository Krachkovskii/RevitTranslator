using CommunityToolkit.Mvvm.ComponentModel;

namespace RevitTranslator.Common.App.Models;

public partial class ObservableCategoryDescriptor : ObservableObject
{
    [ObservableProperty] private bool _isChecked;

    public long Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public ObservableCategoryType CategoryType { get; set; } = null!;
    public bool IsBuiltInCategory { get; init; }
}