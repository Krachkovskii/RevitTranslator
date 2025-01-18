using CommunityToolkit.Mvvm.ComponentModel;

namespace RevitTranslator.Common.Models;

public partial class ObservableCategoryType : ObservableObject
{
    [ObservableProperty] private bool? _isChecked = false;
    [ObservableProperty] private ObservableCategoryDescriptor[] _categories = [];
    [ObservableProperty] private List<ObservableCategoryDescriptor> _filteredCategories = [];

    public string Name { get; init; } = string.Empty;
}