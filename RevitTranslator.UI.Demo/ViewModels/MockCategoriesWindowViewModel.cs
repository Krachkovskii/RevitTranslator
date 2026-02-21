#nullable enable
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Bogus;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RevitTranslator.Common.Extensions;
using RevitTranslator.Common.Models.Categories;
using RevitTranslator.UI.Contracts;

namespace RevitTranslator.UI.Demo.ViewModels;

public partial class MockCategoriesWindowViewModel : ObservableValidator, ICategoriesWindowViewModel
{
    private Dictionary<CategoryViewModel, int> _elementCountDict = new();
    
    [ObservableProperty] private string _mainButtonText = "Select elements to translate";
    [ObservableProperty] private string _searchText = string.Empty;
    [ObservableProperty] private List<CategoryTypeViewModel> _filteredCategoryTypes = [];
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private int _elementCount;
    [ObservableProperty] private ObservableCollection<CategoryTypeViewModel> _categoryTypes = [];
    
    [ObservableProperty] 
    [Required]
    [NotifyDataErrorInfo]
    [MinLength(1)]
    [NotifyCanExecuteChangedFor(nameof(TranslateCommand))]
    private List<CategoryViewModel> _selectedCategories = [];


    public MockCategoriesWindowViewModel()
    {
        IsLoading = true;
        Task.Run(() =>
        {
            CategoryTypes = new Faker<CategoryTypeViewModel>()
                .RuleFor(type => type.Name, faker => faker.Lorem.Word())
                .GenerateBetween(3, 10)
                .ToObservableCollection();

            foreach (var categoryType in CategoryTypes)
            {
                categoryType.Categories = new Faker<CategoryViewModel>()
                    .RuleFor(category => category.Name, faker => faker.Lorem.Word())
                    .RuleFor(category => category.Id, faker => faker.Random.Int(100, 100000))
                    .RuleFor(category => category.IsBuiltInCategory, faker => faker.Random.Bool())
                    .GenerateBetween(5, 50)
                    .ToObservableCollection();

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
        if (args.PropertyName != nameof(CategoryViewModel.IsChecked)) return;
        
        var tempCategories = SelectedCategories.GetRange(0, SelectedCategories.Count);
        var category = (CategoryViewModel)sender!;
        
        if (category.IsChecked)
        {
            tempCategories.Add(category);
            SelectedCategories = tempCategories;
            return;
        }
        
        tempCategories.Remove(category);
        SelectedCategories = tempCategories;
    }

    private void UpdateCategoryTypeCheckbox(CategoryTypeViewModel categoryType)
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

    partial void OnSelectedCategoriesChanged(List<CategoryViewModel> value)
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
    private void CategoryTypeChecked(CategoryTypeViewModel categoryType)
    {
        if (categoryType.IsChecked == null) return;
        
        var isChecked = (bool)categoryType.IsChecked;
        categoryType.Categories.ToList().ForEach(category => category.IsChecked = isChecked);
    }

    public IRelayCommand LoadedCommand { get; }
}