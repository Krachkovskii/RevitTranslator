using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using RevitTranslator.Common.Models;
using RevitTranslator.Extensions;
using RevitTranslator.UI.Contracts;
using RevitTranslator.Utils.Revit;

namespace RevitTranslator.ViewModels;
public partial class CategoriesViewModel : ObservableValidator, ICategoriesWindowViewModel
{
    [ObservableProperty] private string _mainButtonText = "Select elements to translate";
    [ObservableProperty] private string _searchText = string.Empty;
    [ObservableProperty] private List<ObservableCategoryType> _filteredCategoryTypes = [];
    [ObservableProperty] private bool _isLoading;
    
    [ObservableProperty] 
    [Required]
    [NotifyDataErrorInfo]
    [MinLength(1)]
    [NotifyCanExecuteChangedFor(nameof(TranslateCommand))]
    private List<ObservableCategoryDescriptor> _selectedCategories = [];

    private int _elementCount;
    private string _previousSearch = string.Empty;

    public ObservableCategoryType[] CategoryTypes { get; set; } = null!;
    
    public CategoriesViewModel()
    {
        IsLoading = true;
        Task.Run(async () =>
        {
            MainButtonText = "Translate";
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
            await UpdateElementCounter(category, true);
            MainButtonText = _elementCount == 0 
                ? "No elements in selected categories" 
                : $"Translate {_elementCount} elements";
            return;
        }
        
        SelectedCategories.Remove(category);
        await UpdateElementCounter(category, false);
        
        MainButtonText = _elementCount == 0 
            ? "Select category to translate" 
            : $"Translate {_elementCount} elements";
    }

    private async Task UpdateElementCounter(ObservableCategoryDescriptor category, bool add)
    {
        await Task.Run(() =>
        {
            var elementCount = new FilteredElementCollector(Context.ActiveDocument)
                .OfCategory(Category.GetCategory(Context.ActiveDocument, category.Id.ToElementId()).BuiltInCategory)
                .GetElementCount();
            
            // var elementCount = Context.ActiveDocument.EnumerateInstances(category.Id.ToElementId()).Count();
            if (add)
            {
                _elementCount += elementCount;
                return;
            }

            _elementCount -= elementCount;
        });
    }

    async partial void OnSearchTextChanged(string value)
    {
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

                filteredTypes.Add(categoryType);
                categoryType.FilteredCategories = filteredCategories;
            }

            IsLoading = false;
            return filteredTypes;
        });
    }

    [RelayCommand(CanExecute = nameof(CanTranslate))] 
    private void Translate()
    {
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

