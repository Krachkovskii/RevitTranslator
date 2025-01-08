using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Bogus;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RevitTranslator.Common.App.Models;
using RevitTranslator.UI.Contracts;

// using RevitTranslator.Models;

namespace RevitTranslator.Demo.ViewModels;

public partial class MockCategoriesWindowViewModel : ObservableValidator, ICategoriesWindowViewModel
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
    
    public ObservableCategoryType[] CategoryTypes { get; private set; }

    public MockCategoriesWindowViewModel()
    {
        IsLoading = true;
        Task.Run(async () =>
        {
            await Task.Delay(1000);
            
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

                foreach (var category in categoryType.Categories)
                {
                    category.PropertyChanged += OnCategoryPropertyChanged;
                    category.IsChecked = new Faker().Random.Bool();
                }
            }

            FilteredCategoryTypes = CategoryTypes.ToList();
            IsLoading = false;
        });
    }

    private void OnCategoryPropertyChanged(object? sender, PropertyChangedEventArgs args)
    {
        if (args.PropertyName != nameof(ObservableCategoryDescriptor.IsChecked)) return;
        var category = (ObservableCategoryDescriptor)sender!;

        if (category.IsChecked)
        {
            SelectedCategories.Add(category);
            _elementCount += new Faker().Random.Int(0, 50);
            MainButtonText = $"Translate {_elementCount} elements";
            return;
        }
        
        SelectedCategories.Remove(category);
        _elementCount -= new Faker().Random.Int(0, 50);
        
        if (_elementCount == 0) MainButtonText = "Select category to translate";
    }

    partial void OnSearchTextChanged(string value)
    {
        IsLoading = true;
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

            IsLoading = false;
            FilteredCategoryTypes = filteredCategories.ToList();
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

    public void OnCloseRequested()
    {
        
    }
}