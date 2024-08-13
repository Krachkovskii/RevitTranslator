using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RevitTranslatorAddin.ViewModels;

/// <summary>
/// ListItem for a ListBox in Categories window.
/// Represents a Revit Category and contains Name, Type and Category properties, 
/// as well as the state of the element's checkbox.
/// </summary>
public class ListItem : INotifyPropertyChanged
{
    private readonly TranslateCategoriesViewModel _viewModel;

    /// <summary>
    /// Name of the category
    /// </summary>
    public string Name
    {
        get; private set;
    }

    /// <summary>
    /// CategoryType of the category
    /// </summary>
    public string Type
    {
        get; private set;
    }

    private Category _category;
    /// <summary>
    /// The category
    /// </summary>
    public Category Category
    {
        get => _category;
        set
        {
            _category = value;
            Name = _category.Name;
            if (_category.CategoryType == CategoryType.AnalyticalModel)
            {
                Type = "Analytical Model Categories";
            }
            else
            {
                Type = _category.CategoryType.ToString() + " Categories";
            }
        }
    }

    private bool _isSelected;
    
    /// <summary>
    /// Whether the category is selected in the UI
    /// </summary>
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected != value)
            {
                _isSelected = value;
                OnPropertyChanged();
            }
            _viewModel.CountElements();
        }
    }
    public ListItem(Category category, TranslateCategoriesViewModel viewModel)
    {
        _viewModel = viewModel;
        Category = category;
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
