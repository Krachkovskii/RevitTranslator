using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using RevitTranslator.Common.App.Models;
using RevitTranslator.Common.Extensions;
using RevitTranslator.Extensions;
using RevitTranslator.UI.Contracts;
using RevitTranslator.Utils.Revit;

namespace RevitTranslator.ViewModels;
public partial class CategoriesViewModel : ObservableValidator, ICategoriesWindowViewModel
{
    [ObservableProperty] private string _mainButtonText = "Select elements to translate";
    [ObservableProperty] private string _searchText = string.Empty;
    [ObservableProperty] private ObservableCollection<ObservableCategoryType> _filteredCategoryTypes = [];
    [ObservableProperty] private bool _isLoading;
    
    [ObservableProperty] 
    [Required]
    [NotifyDataErrorInfo]
    [MinLength(1)]
    [NotifyCanExecuteChangedFor(nameof(TranslateCommand))]
    private List<ObservableCategoryDescriptor> _selectedCategories = [];

    private int _elementCount;
    
    public ObservableCategoryType[] CategoryTypes { get; private set; }
    
    public CategoriesViewModel()
    {
        MainButtonText = "Translate";
        CategoryTypes = CreateCategoryTypes();
    }

    private ObservableCategoryType[] CreateCategoryTypes()
    {
        var categories = CategoryFilter.ValidCategories;
        var categoryTypes = categories.Select(category => category.CategoryType)
            .Distinct()
            .Select(type => new ObservableCategoryType
            {
                Categories = categories
                    .Where(category => category.CategoryType == type)
                    .Select(category => new ObservableCategoryDescriptor
                    {
                        Id = category.Id.ToLong()
                    })
                    .ToArray(),
                Name = type.ToString(),
            })
            .ToArray();

        return categoryTypes;
    }

    private void OnCategoryPropertyChanged(object? sender, PropertyChangedEventArgs args)
    {
        if (args.PropertyName != nameof(ObservableCategoryDescriptor.IsChecked)) return;
        var category = (ObservableCategoryDescriptor)sender!;

        if (category.IsChecked)
        {
            SelectedCategories.Add(category);
            // _elementCount += new Faker().Random.Int(0, 50);
            MainButtonText = $"Translate {_elementCount} elements";
            return;
        }
        
        SelectedCategories.Remove(category);
        // _elementCount -= new Faker().Random.Int(0, 50);
        
        if (_elementCount == 0) MainButtonText = "Select category to translate";
    }

    partial void OnSearchTextChanged(string value)
    {
        Task.Run(() =>
        {
            List<ObservableCategoryType> filteredCategories = [];
            foreach (var categoryType in CategoryTypes)
            {
                categoryType.FilteredCategories.Clear();
                var validCategory = false;
                foreach (var category in categoryType.Categories)
                {
                    var contains = category.Name.Contains(value, StringComparison.OrdinalIgnoreCase);
                    if (!contains) continue;

                    validCategory = true;
                    categoryType.FilteredCategories.Add(category);
                }

                if (validCategory) filteredCategories.Add(categoryType);
            }

            FilteredCategoryTypes = filteredCategories.ToObservableCollection();
        });
    }

    [RelayCommand(CanExecute = nameof(CanTranslate))] 
    private void Translate()
    {
    }

    private bool CanTranslate()
    {
        return !HasErrors;
    }
}

