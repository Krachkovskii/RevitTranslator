using CommunityToolkit.Mvvm.ComponentModel;

namespace RevitTranslator.Common.App.Models;

public partial class ObservableCategoryDescriptor : ObservableObject
{
    [ObservableProperty] private bool _isChecked;

    public long Id { get; init; }
    public string Name { get; init; }
    public ObservableCategoryType CategoryType { get; init; }
    public bool IsBuiltInCategory { get; init; }
}