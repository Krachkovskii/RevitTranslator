using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using RevitTranslator.Common.Models;
using RevitTranslator.Extensions;
using RevitTranslator.Services;
using RevitTranslator.UI.Contracts;
using RevitTranslator.Utils;

namespace RevitTranslator.ViewModels;
public partial class CategoriesWindowViewModel : ObservableValidator, ICategoriesWindowViewModel
{
    private readonly BaseTranslationService _baseTranslationService;
    
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private string _mainButtonText = "Select categories to translate";
    [ObservableProperty] private string _searchText = string.Empty;
    [ObservableProperty] private List<ObservableCategoryType> _filteredCategoryTypes = [];
    [ObservableProperty] private List<ObservableCategoryDescriptor> _selectedCategories = [];
    
    [Required]
    [NotifyDataErrorInfo]
    [Range(1, int.MaxValue)]
    [NotifyCanExecuteChangedFor(nameof(TranslateCommand))]
    [ObservableProperty] private int _elementCount;

    // private string _previousSearch = string.Empty;
    private bool _isInternalCheckboxUpdate;

    public ObservableCategoryType[] CategoryTypes { get; private set; } = [];
    
    public CategoriesWindowViewModel(BaseTranslationService baseTranslationService)
    {
        _baseTranslationService = baseTranslationService;
        IsLoading = true;
        Task.Run(() =>
        {
            CategoryTypes = CreateCategoryTypes();
            FilteredCategoryTypes = CategoryTypes.ToList();
            
            IsLoading = false;
        });
    }

    private ObservableCategoryType[] CreateCategoryTypes()
    {
            var categories = CategoryManager.ValidCategories;
            var categoryTypes = categories.Select(category => category.CategoryType)
                .Distinct()
                .OrderBy(type => type.ToString())
                .Select(type => new ObservableCategoryType
                {
                    Categories = categories
                        .Where(category => category.CategoryType == type)
                        .Select(category => new ObservableCategoryDescriptor
                        {
                            Name = category.Name,
                            IsBuiltInCategory = category.BuiltInCategory != BuiltInCategory.INVALID,
                            Id = category.Id.ToLong()
                        })
                        .OrderBy(category => category.Name)
                        .ToArray(),
                    Name = type.ToString().StartsWith("Analytical") ? "Analytical Model" : type.ToString()
                })
                .ToArray();

            foreach (var categoryType in categoryTypes)
            {
                categoryType.FilteredCategories = categoryType.Categories.ToList();
                foreach (var category in categoryType.Categories)
                {
                    category.CategoryType = categoryType;
                    category.PropertyChanged += OnCategoryPropertyChanged;
                }
            }
            
            return categoryTypes;
    }
    
    private void OnCategoryPropertyChanged(object? sender, PropertyChangedEventArgs args)
    {
        if (args.PropertyName != nameof(ObservableCategoryDescriptor.IsChecked)) return;
        if (_isInternalCheckboxUpdate) return;
        
        var category = (ObservableCategoryDescriptor)sender!;
        if (category.IsChecked)
        {
            SelectedCategories.Add(category);
            OnSelectedCategoriesChanged(SelectedCategories);
            return;
        }
        
        SelectedCategories.Remove(category);
        OnSelectedCategoriesChanged(SelectedCategories);
    }

    async partial void OnSelectedCategoriesChanged(List<ObservableCategoryDescriptor> value)
    {
        if (SelectedCategories.Count == 0)
        {
            ElementCount = 0;
            return;
        }
        
        ElementCount = await Task.Run(() =>
        {
            var categories = SelectedCategories
                .Select(cat =>
                    Category.GetCategory(Context.ActiveDocument, cat.Id.ToElementId()).BuiltInCategory)
                .ToArray();
            
            return new FilteredElementCollector(Context.ActiveDocument)
                .WherePasses(new ElementMulticategoryFilter(categories))
                .GetElementCount();
        });
    }

    partial void OnElementCountChanged(int value)
    {
        if (value == 0)
        {
            MainButtonText = SelectedCategories.Count == 0
                ? "Select category to translate"
                : "No elements in selected categories";
            
            return;
        }
        
        MainButtonText = $"Translate {_elementCount} elements";
    }

    // TODO: Fix filtering, atm it causes animation crashes
    // async partial void OnSearchTextChanged(string value)
    // {
    //     if (value == string.Empty)
    //     {
    //         FilteredCategoryTypes = CategoryTypes.ToList();
    //         _previousSearch = string.Empty;
    //         return;
    //     }
    //     
    //     var searchSource = value.Contains(_previousSearch)
    //         ? FilteredCategoryTypes.ToArray()
    //         : CategoryTypes;
    //     
    //     _previousSearch = value;
    //     IsLoading = true;
    //     
    //     FilteredCategoryTypes = await Task.Run(() =>
    //     {
    //         var filteredTypes = new List<ObservableCategoryType>();
    //         foreach (var categoryType in searchSource)
    //         {
    //             var filteredCategories = new List<ObservableCategoryDescriptor>();
    //             var validCategoryType = false;
    //             foreach (var category in categoryType.Categories)
    //             {
    //                 var contains = category.Name.Contains(value, StringComparison.OrdinalIgnoreCase);
    //                 if (!contains) continue;
    //
    //                 validCategoryType = true;
    //                 filteredCategories.Add(category);
    //             }
    //             if (!validCategoryType) continue;
    //
    //             categoryType.FilteredCategories = filteredCategories;
    //             filteredTypes.Add(categoryType);
    //         }
    //
    //         IsLoading = false;
    //         return filteredTypes;
    //     });
    // }

    [RelayCommand]
    private void CategoryTypeChecked(ObservableCategoryType categoryType)
    {
        var value = categoryType.IsChecked;
        var tempCategories = SelectedCategories.GetRange(0, SelectedCategories.Count);
        _isInternalCheckboxUpdate = true;

        switch (value)
        {
            case true:
            {
                foreach (var category in categoryType.Categories)
                {
                    category.IsChecked = true;
                    tempCategories.Add(category);
                }
                break;
            }
            case false:
            {
                foreach (var category in categoryType.Categories)
                {
                    category.IsChecked = false;
                    tempCategories.Remove(category);
                }
                
                break;
            }
        }
        
        SelectedCategories = tempCategories;
        _isInternalCheckboxUpdate = false;
    }

    [RelayCommand(CanExecute = nameof(CanTranslate))] 
    private void Translate()
    {
        var elements = new FilteredElementCollector(Context.ActiveDocument)
            .WherePasses(
                new ElementMulticategoryFilter(SelectedCategories
                    .Select(category => category.Id.ToElementId())
                    .ToArray()))
            .ToArray();

        _baseTranslationService.SelectedElements = elements;
        _baseTranslationService.Execute();
    }

    public void OnCloseRequested()
    {
        foreach (var categoryType in CategoryTypes)
        {
            foreach (var category in categoryType.Categories)
            {
                category.PropertyChanged -= OnCategoryPropertyChanged;
            }
        }
    }

    private bool CanTranslate()
    {
        return !HasErrors;
    }
}

