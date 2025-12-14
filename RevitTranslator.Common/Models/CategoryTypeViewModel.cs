using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace RevitTranslator.Common.Models;

public partial class CategoryTypeViewModel : ObservableObject
{
    private bool _isInternalCheckboxUpdate;
    private int _selectedCategoriesCount;
    
    [ObservableProperty] private bool? _isChecked = false;
    [ObservableProperty] private bool _isVisible = true;
    [ObservableProperty] private bool _isExpanded;
    [ObservableProperty] private int _selectedElementCount;
    [ObservableProperty] private ObservableCollection<CategoryViewModel> _categories = [];
    
    public string Name { get; init; } = string.Empty;

    partial void OnIsVisibleChanged(bool value)
    {
        Console.WriteLine($"TYPE {Name} is visible changed to: {value}");
    }

    partial void OnIsCheckedChanged(bool? value)
    {
        if (_isInternalCheckboxUpdate) return;
        if (value is null) return;

        _isInternalCheckboxUpdate = true;
        var isChecked = value is true;
        foreach (var category in Categories)
        {
            category.IsChecked = isChecked;
        }
        _isInternalCheckboxUpdate = false;
    }
    
    partial void OnCategoriesChanged(ObservableCollection<CategoryViewModel>? oldValue, ObservableCollection<CategoryViewModel> newValue)
    {
        foreach (var category in newValue)
        {
            category.PropertyChanged += CategoryOnPropertyChanged;
        }
        
        _selectedCategoriesCount = 0;
        if (oldValue is null) return;
        
        foreach (var category in oldValue)
        {
            category.PropertyChanged -= CategoryOnPropertyChanged;
        }
    }

    private void CategoryOnPropertyChanged(object sender, PropertyChangedEventArgs args)
    {
        if (sender is not CategoryViewModel category) return;
        if (args.PropertyName != nameof(CategoryViewModel.IsChecked)) return;
        
        if (category.IsChecked)
        {
            _selectedCategoriesCount++;
            SelectedElementCount += category.ElementCount;
        }
        else
        {
            _selectedCategoriesCount--;
            SelectedElementCount -= category.ElementCount;
        }
        
        if (_isInternalCheckboxUpdate) return;
        
        _isInternalCheckboxUpdate = true;
        if (_selectedCategoriesCount == 0)
        {
            IsChecked = false;
        }
        else if (_selectedCategoriesCount == Categories.Count)
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