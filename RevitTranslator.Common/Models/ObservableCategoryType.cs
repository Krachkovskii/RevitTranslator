using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace RevitTranslator.Common.Models;

public partial class ObservableCategoryType : ObservableObject
{
    [ObservableProperty] private bool? _isChecked = false;
    [ObservableProperty] private ObservableCategoryDescriptor[] _categories = [];
    [ObservableProperty] private List<ObservableCategoryDescriptor> _filteredCategories = [];

    private bool _isInternalChange;

    public string Name { get; init; } = string.Empty;

    partial void OnCategoriesChanged(ObservableCategoryDescriptor[]? oldValue, ObservableCategoryDescriptor[] newValue)
    {
        foreach (var category in newValue)
        {
            category.PropertyChanged += OnCategoryPropertyChanged;
        }
        if (oldValue is null) return;
        
        foreach (var category in oldValue)
        {
            category.PropertyChanged -= OnCategoryPropertyChanged;
        }
    }

    partial void OnIsCheckedChanged(bool? value)
    {
        if (value is null || _isInternalChange)
        {
            _isInternalChange = false;
            return;
        }
        
        _isInternalChange = true;
        foreach (var category in Categories)
        {
            category.IsChecked = (bool)value;
        }
        _isInternalChange = false;
    }

    private void OnCategoryPropertyChanged(object? sender, PropertyChangedEventArgs args)
    {
        if (args.PropertyName != nameof(IsChecked)) return;
        if (_isInternalChange) return;
        
        _isInternalChange = true;
        if (Categories.All(category => category.IsChecked))
        {
            IsChecked = true;
        }
        else if (Categories.All(category => !category.IsChecked))
        {
            IsChecked = false;
        }
        else
        {
            IsChecked = null;
        }
        _isInternalChange = false;
    }
}