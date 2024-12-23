namespace RevitTranslatorAddin.ViewModels;

/// <summary>
/// ListItem for a ListBox in Categories window.
/// Represents a Revit Category and contains Name, Type and Category properties, 
/// as well as the state of the element's checkbox.
/// </summary>
public partial class ListItem : ObservableObject
{
    private readonly RevitTranslator.ViewModels.CategoriesViewModel _viewModel;
    
    [ObservableProperty] private Category _category;
    [ObservableProperty] private bool _isSelected;
    
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

    partial void OnIsSelectedChanged(bool value)
    {
        _viewModel.CountElements();
    }

    partial void OnCategoryChanged(Category category)
    {
        Type = category.CategoryType == CategoryType.AnalyticalModel 
            ? "Analytical Model Categories" 
            : category.CategoryType.ToString() + " Categories";
    }
    
    public ListItem(Category category, RevitTranslator.ViewModels.CategoriesViewModel viewModel)
    {
        _viewModel = viewModel;
        Category = category;
    }
}
