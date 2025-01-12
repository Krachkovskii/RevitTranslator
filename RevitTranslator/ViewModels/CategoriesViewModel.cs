using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using RevitTranslator.Common.Models;
using RevitTranslator.Extensions;
using RevitTranslator.Services;
using RevitTranslator.UI.Contracts;
using RevitTranslator.Utils.Revit;

namespace RevitTranslator.ViewModels;
public partial class CategoriesViewModel : ObservableValidator, ICategoriesWindowViewModel
{
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

    private string _previousSearch = string.Empty;

    public ObservableCategoryType[] CategoryTypes { get; private set; } = null!;
    
    public CategoriesViewModel()
    {
        IsLoading = true;
        Task.Run(async () =>
        {
            await CreateCategoryTypes();
            FilteredCategoryTypes = CategoryTypes.ToList();
            
            IsLoading = false;
        });
    }

    private async Task CreateCategoryTypes()
    {
        await Task.Run(() =>
        {
            var categories = CategoryFilter.ValidCategories;
            CategoryTypes = categories.Select(category => category.CategoryType)
                .Distinct()
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
                    Name = type.ToString()
                })
                .ToArray();

            foreach (var categoryType in CategoryTypes)
            {
                categoryType.FilteredCategories = categoryType.Categories.ToList();
                foreach (var category in categoryType.Categories)
                {
                    category.CategoryType = categoryType;
                    category.PropertyChanged += OnCategoryPropertyChanged;
                }
            }
        });
    }

    private async void OnCategoryPropertyChanged(object? sender, PropertyChangedEventArgs args)
    {
        if (args.PropertyName != nameof(ObservableCategoryDescriptor.IsChecked)) return;
        
        var category = (ObservableCategoryDescriptor)sender!;
        if (category.IsChecked)
        {
            SelectedCategories.Add(category);
            await UpdateElementCounter();
            return;
        }
        
        SelectedCategories.Remove(category);
        await UpdateElementCounter();
    }

    private async Task UpdateElementCounter()
    {
        await Task.Run(() =>
        {
            if (SelectedCategories.Count == 0)
            {
                ElementCount = 0;
                return;
            }

            var categories = SelectedCategories
                .Select(cat =>
                    Category.GetCategory(Context.ActiveDocument, cat.Id.ToElementId()).BuiltInCategory)
                .ToArray();
            
            var elementCount = new FilteredElementCollector(Context.ActiveDocument)
                .WherePasses(new ElementMulticategoryFilter(categories))
                .GetElementCount();

            ElementCount = elementCount;
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

    async partial void OnSearchTextChanged(string value)
    {
        if (value == string.Empty)
        {
            FilteredCategoryTypes = CategoryTypes.ToList();
            _previousSearch = string.Empty;
            return;
        }
        
        var searchSource = value.Contains(_previousSearch)
            ? FilteredCategoryTypes.ToArray()
            : CategoryTypes;
        
        _previousSearch = value;
        IsLoading = true;
        
        FilteredCategoryTypes = await Task.Run(() =>
        {
            var filteredTypes = new List<ObservableCategoryType>();
            foreach (var categoryType in searchSource)
            {
                var filteredCategories = new List<ObservableCategoryDescriptor>();
                var validCategoryType = false;
                foreach (var category in categoryType.Categories)
                {
                    var contains = category.Name.Contains(value, StringComparison.OrdinalIgnoreCase);
                    if (!contains) continue;

                    validCategoryType = true;
                    filteredCategories.Add(category);
                }
                if (!validCategoryType) continue;

                categoryType.FilteredCategories = filteredCategories;
                filteredTypes.Add(categoryType);
            }

            IsLoading = false;
            return filteredTypes;
        });
    }

    [RelayCommand(CanExecute = nameof(CanTranslate))] 
    private void Translate()
    {
        var elements = new FilteredElementCollector(Context.ActiveDocument)
            .WherePasses(
                new ElementMulticategoryFilter(SelectedCategories
                    .Select(category => new ElementId(category.Id))
                    .ToArray()))
            .ToArray();

        var service = new BaseTranslationService();
        service.SelectedElements = elements;
        
        service.Execute();
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

