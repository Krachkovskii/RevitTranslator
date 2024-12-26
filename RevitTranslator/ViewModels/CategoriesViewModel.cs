using System.Collections.ObjectModel;
using RevitTranslator.Commands;
using RevitTranslator.Models;

namespace RevitTranslator.ViewModels;
public partial class CategoriesViewModel : ObservableValidator, ICategoriesViewModel
{
    //TODO: Implement validation logic
    private readonly TranslationUtils _translationUtils = null;
    
    [ObservableProperty] private string _mainButtonText;
    [ObservableProperty] private ObservableCollection<CategoryDescriptor> _selectedCategories;
    
    public CategoriesViewModel(TranslationUtils utils, ProgressWindowUtils windowUtils, List<CategoryDescriptor> categoryDescriptors)
    {
        AllCategories = categoryDescriptors.ToArray();

        _translationUtils = utils;
        _progressWindowUtils = windowUtils;
        MainButtonText = "Translate";
    }
    public CategoryDescriptor[] AllCategories { get; }
    
    /// <summary>
    /// Counts the number of elements in all selected categories
    /// </summary>
    private int ElementCount
    {
        set
        {
            if (value == 0)
            {
                MainButtonText = Enumerable.Any<CategoryDescriptor>(SelectedCategories, categoryDescriptor => categoryDescriptor.IsChecked) 
                    ? "No elements of selected categories" 
                    : "Select Categories to translate";
            }
            else
            {
                MainButtonText = $"Translate {value} elements";
            }
        }
    }

    private ProgressWindowUtils _progressWindowUtils { get; set; } = null;
    /// <summary>
    /// Counts the number of elements in selected categories and asynchronously updates the value.
    /// </summary>
    public void CountElements()
    {
        Task.Run(() =>
        {
            // var categories = AllCategories.Where(item => item.IsChecked).Select(c => c.Category.BuiltInCategory).ToList();
            // var count = TranslateCategoriesCommand.GetElementsFromCategories(categories).Count;
            //
            // ElementCount = count;
        });
    }

    /// <summary>
    /// Checks or unchecks all categories in selected Category Type.
    /// </summary>
    /// <param name="parameter">
    /// Passed by GroupCheckboxStateConverter.
    /// </param>
    [RelayCommand]
    private void SelectAllInGroup(object param)
    {
        if (param is not Tuple<bool, string> parameter) return;

        bool isChecked = parameter.Item1;
        string type = parameter.Item2;

        // Example: Update all items in the group
        var itemsInGroup = AllCategories.Where(item => item.Type == type);
        foreach (var item in itemsInGroup)
        {
            item.IsChecked = isChecked;
        }
    }
    
    /// <summary>
    /// Command for translation of all elements in selected categories. 
    /// Starts translation process and calls the raise of ExternalEvent to update Revit model.
    /// </summary>
    [RelayCommand]
    private void TranslateSelected()
    {
        TranslateCategoriesCommand.Window.Close();
        TranslateCategoriesCommand.Window = null;

        BaseTranslationCommand.StartCategoryTranslationCommand(Enumerable.ToList<CategoryDescriptor>(SelectedCategories), _progressWindowUtils, _translationUtils, false, false);
    }
}

