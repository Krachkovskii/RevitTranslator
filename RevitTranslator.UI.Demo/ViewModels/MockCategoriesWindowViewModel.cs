#nullable enable
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Bogus;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RevitTranslator.Common.Models;
using RevitTranslator.UI.Contracts;

namespace RevitTranslator.Demo.ViewModels;

public partial class MockCategoriesWindowViewModel : ObservableValidator, ICategoriesWindowViewModel
{
    [ObservableProperty] private string _mainButtonText = "Select elements to translate";
    [ObservableProperty] private string _searchText = string.Empty;
    [ObservableProperty] private List<ObservableCategoryType> _filteredCategoryTypes = [];
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private int _elementCount;
    
    [ObservableProperty] 
    [Required]
    [NotifyDataErrorInfo]
    [MinLength(1)]
    [NotifyCanExecuteChangedFor(nameof(TranslateCommand))]
    private List<ObservableCategoryDescriptor> _selectedCategories = [];

    private string _previousSearch = string.Empty;
    private Dictionary<ObservableCategoryDescriptor, int> _elementCountDict = new();

    public ObservableCategoryType[] CategoryTypes { get; private set; } = [];

    public MockCategoriesWindowViewModel()
    {
        IsLoading = true;
        Task.Run(() =>
        {
            CategoryTypes = new Faker<ObservableCategoryType>()
                .RuleFor(type => type.Name, faker => faker.Lorem.Word())
                .GenerateBetween(3, 10)
                .ToArray();

            foreach (var categoryType in CategoryTypes)
            {
                categoryType.Categories = new Faker<ObservableCategoryDescriptor>()
                    .RuleFor(category => category.Name, faker => faker.Lorem.Word())
                    .RuleFor(category => category.Id, faker => faker.Random.Int(100, 100000))
                    .RuleFor(category => category.IsBuiltInCategory, faker => faker.Random.Bool())
                    .RuleFor(category => category.CategoryType, categoryType)
                    .GenerateBetween(5, 50)
                    .ToArray();
                categoryType.FilteredCategories = categoryType.Categories.ToList();

                // todo: refactor faker invocation, one is more than enough
                foreach (var category in categoryType.Categories)
                {
                    category.PropertyChanged += OnCategoryPropertyChanged;
                    _elementCountDict[category] = new Faker().Random.Int(0, 100);
                    category.IsChecked = new Faker().Random.Bool();
                }
            }

            ElementCount = SelectedCategories.Sum(category => _elementCountDict[category]);
            FilteredCategoryTypes = CategoryTypes.ToList();
            IsLoading = false;
        }).GetAwaiter().GetResult();
    }

    private void OnCategoryPropertyChanged(object? sender, PropertyChangedEventArgs args)
    {
        if (args.PropertyName != nameof(ObservableCategoryDescriptor.IsChecked)) return;
        
        var tempCategories = SelectedCategories.GetRange(0, SelectedCategories.Count);
        var category = (ObservableCategoryDescriptor)sender!;
        
        if (category.IsChecked)
        {
            tempCategories.Add(category);
            SelectedCategories = tempCategories;
            UpdateCategoryTypeCheckbox(category.CategoryType);
            return;
        }
        
        tempCategories.Remove(category);
        SelectedCategories = tempCategories;
        UpdateCategoryTypeCheckbox(category.CategoryType);
    }

    private void UpdateCategoryTypeCheckbox(ObservableCategoryType categoryType)
    {
        if (categoryType.Categories.All(category => category.IsChecked))
        {
            categoryType.IsChecked = true;
        }
        else if (categoryType.Categories.Any(category => category.IsChecked))
        {
            categoryType.IsChecked = null;
        }
        else
        {
            categoryType.IsChecked = false;
        }
    }

    partial void OnSelectedCategoriesChanged(List<ObservableCategoryDescriptor> value)
    {
        ElementCount = value.Sum(category => _elementCountDict[category]);
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

    // TODO: Fix search logic; at the moment, updating the list with expander throws animation error
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
    //         ? FilteredCategoryTypes
    //         : CategoryTypes.ToList();
    //     
    //     _previousSearch = value;
    //     IsLoading = true;
    //     
    //     FilteredCategoryTypes = await Task.Run(() =>
    //     {
    //         var categoryTypes = searchSource.GetRange(0, searchSource.Count);
    //         List<ObservableCategoryType> filteredCategoryTypes = [];
    //         foreach (var categoryType in categoryTypes)
    //         {
    //             categoryType.FilteredCategories.Clear();
    //             var validCategory = false;
    //             foreach (var category in categoryType.Categories)
    //             {
    //                 var contains = category.Name.Contains(value, StringComparison.OrdinalIgnoreCase);
    //                 if (!contains) continue;
    //
    //                 validCategory = true;
    //                 categoryType.FilteredCategories.Add(category);
    //             }
    //
    //             if (validCategory) filteredCategoryTypes.Add(categoryType);
    //         }
    //
    //         IsLoading = false;
    //         return filteredCategoryTypes.ToList();
    //     });
    // }

    [RelayCommand(CanExecute = nameof(CanTranslate))] 
    private void Translate()
    {
    }

    private bool CanTranslate()
    {
        return !HasErrors;
    }

    public void OnCloseRequested()
    {
        
    }

    [RelayCommand]
    private void CategoryTypeChecked(ObservableCategoryType categoryType)
    {
        if (categoryType.IsChecked == null) return;
        
        var isChecked = (bool)categoryType.IsChecked;
        categoryType.Categories.ToList().ForEach(category => category.IsChecked = isChecked);
    }
}