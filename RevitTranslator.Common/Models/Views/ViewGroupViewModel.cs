using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace RevitTranslator.Common.Models.Views;

public sealed partial class ViewGroupViewModel : ObservableObject
{
    private bool _isInternalCheckboxUpdate;
    private int _selectedSubElementsCount;
    
    [ObservableProperty] private bool _isVisible = true;
    [ObservableProperty] private bool? _isChecked = false;
    [ObservableProperty] private bool _isExpanded;
    [ObservableProperty] private int _selectedElementCount;
    [ObservableProperty] private ObservableCollection<ViewViewModel> _views = [];

    public ViewTypeInternal Type { get; init; }
    public string Name { get; init; } = "";
    public bool IsSheetCollection { get; init; }
    
    partial void OnIsCheckedChanged(bool? value)
    {
        if (_isInternalCheckboxUpdate) return;
        if (value is null) return;

        _isInternalCheckboxUpdate = true;
        var isChecked = value is true;
        foreach (var element in Views)
        {
            element.IsChecked = isChecked;
        }
        _isInternalCheckboxUpdate = false;
    }
    
    partial void OnViewsChanged(ObservableCollection<ViewViewModel>? oldValue, ObservableCollection<ViewViewModel> newValue)
    {
        foreach (var category in newValue)
        {
            category.PropertyChanged += CategoryOnPropertyChanged;
        }
        
        _selectedSubElementsCount = 0;
        if (oldValue is null) return;
        
        foreach (var category in oldValue)
        {
            category.PropertyChanged -= CategoryOnPropertyChanged;
        }
    }

    private void CategoryOnPropertyChanged(object sender, PropertyChangedEventArgs args)
    {
        if (sender is not ViewViewModel view) return;
        if (args.PropertyName != nameof(ViewViewModel.IsChecked)) return;

        if (view.IsChecked)
        {
            _selectedSubElementsCount++;
            SelectedElementCount += view.ElementCount;
        }
        else
        {
            _selectedSubElementsCount--;
            SelectedElementCount -= view.ElementCount;
        }

        if (_isInternalCheckboxUpdate) return;

        _isInternalCheckboxUpdate = true;
        if (_selectedSubElementsCount == 0)
        {
            IsChecked = false;
        }
        else if (_selectedSubElementsCount == Views.Count)
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