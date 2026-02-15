using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace RevitTranslator.Common.Models.Categories;

public sealed partial class CategoryTypeViewModel : ObservableObject
{
    private bool _isInternalCheckboxUpdate;
    private int _selectedSubElementsCount;
    
    [ObservableProperty] private bool? _isChecked = false;
    [ObservableProperty] private bool _isVisible = true;
    [ObservableProperty] private bool _isExpanded;
    [ObservableProperty] private int _selectedElementCount;
    [ObservableProperty] private ObservableCollection<CategoryViewModel> _categories = [];
    
    public string Name { get; init; } = string.Empty;

    partial void OnIsCheckedChanged(bool? value)
    {
        if (_isInternalCheckboxUpdate) return;
        if (value is null) return;

        _isInternalCheckboxUpdate = true;
        var isChecked = value is true;
        foreach (var element in Categories)
        {
            element.IsChecked = isChecked;
        }
        _isInternalCheckboxUpdate = false;
    }
    
    partial void OnCategoriesChanged(ObservableCollection<CategoryViewModel>? oldValue, ObservableCollection<CategoryViewModel> newValue)
    {
        foreach (var element in newValue)
        {
            element.PropertyChanged += ElementOnPropertyChanged;
        }
        
        _selectedSubElementsCount = 0;
        if (oldValue is null) return;
        
        foreach (var category in oldValue)
        {
            category.PropertyChanged -= ElementOnPropertyChanged;
        }
    }

    private void ElementOnPropertyChanged(object sender, PropertyChangedEventArgs args)
    {
        if (sender is not CategoryViewModel category) return;
        if (args.PropertyName != nameof(CategoryViewModel.IsChecked)) return;
        
        if (category.IsChecked)
        {
            _selectedSubElementsCount++;
            SelectedElementCount += category.ElementCount;
        }
        else
        {
            _selectedSubElementsCount--;
            SelectedElementCount -= category.ElementCount;
        }
        
        if (_isInternalCheckboxUpdate) return;
        
        _isInternalCheckboxUpdate = true;
        if (_selectedSubElementsCount == 0)
        {
            IsChecked = false;
        }
        else if (_selectedSubElementsCount == Categories.Count)
        {
            IsChecked = true;
        }
        else
        {
            IsChecked = null;
        }
        _isInternalCheckboxUpdate = false;
    }
}