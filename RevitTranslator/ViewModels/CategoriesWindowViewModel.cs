using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Timers;
using RevitTranslator.Common.Extensions;
using RevitTranslator.Common.Models;
using RevitTranslator.Common.Models.Categories;
using RevitTranslator.Extensions;
using RevitTranslator.UI.Contracts;
using RevitTranslator.Utils;
using Timer = System.Timers.Timer;

namespace RevitTranslator.ViewModels;

public partial class CategoriesWindowViewModel
    : ObservableValidator, ICategoriesWindowViewModel, IDisposable
{
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private string _mainButtonText = "";
    [ObservableProperty] private string _searchText = string.Empty;
    [ObservableProperty] private ObservableCollection<CategoryTypeViewModel> _categoryTypes = [];
    [ObservableProperty] private List<CategoryViewModel> _selectedCategories = [];

    [Required]
    [NotifyDataErrorInfo]
    [Range(1, int.MaxValue)]
    [NotifyCanExecuteChangedFor(nameof(TranslateCommand))]
    [ObservableProperty]
    private int _elementCount;

    private CancellationTokenSource? _searchCancellationTokenSource;

    [RelayCommand]
    private void OnLoaded()
    {
        OnElementCountChanged(0);
        CategoryTypes = CreateCategoryTypesAsync();
        foreach (var type in CategoryTypes)
        {
            type.PropertyChanged += OnCategoryTypeChanged;
        }
    }

    private void OnCategoryTypeChanged(object? sender, PropertyChangedEventArgs args)
    {
        if (args.PropertyName != nameof(CategoryTypeViewModel.SelectedElementCount)) return;

        ElementCount = CategoryTypes.Sum(type => type.Categories
            .Where(cat => cat.IsChecked)
            .Sum(cat => cat.ElementCount));
    }

    private ObservableCollection<CategoryTypeViewModel> CreateCategoryTypesAsync()
    {
        var doc = Context.ActiveDocument;
        if (doc == null) return [];

        return CategoryManager.ValidCategories
            .Select(cat => new
            {
                category = cat,
                count = new FilteredElementCollector(doc)
                    .OfCategory(cat.BuiltInCategory)
                    .GetElementCount()
            })
            .Where(cat => cat.count != 0)
            .Select(cat => new CategoryViewModel
            {
                Name = cat.category.Name,
                ElementCount = cat.count,
                CategoryTypeEnum = (int)cat.category.CategoryType,
                IsBuiltInCategory = cat.category.BuiltInCategory != BuiltInCategory.INVALID,
                Id = cat.category.Id.ToLong()
            })
            .GroupBy(cat => cat.CategoryTypeEnum)
            .Select(group => new CategoryTypeViewModel
            {
                Categories = group
                    .OrderBy(cat => cat.Name)
                    .ToObservableCollection(),
                Name = group
                    .First()
                    .CategoryTypeEnum
                    .Cast<CategoryType>()
                    .ToString()
            })
            .OrderBy(group => group.Name)
            .ToObservableCollection();
    }

    partial void OnSelectedCategoriesChanged(List<CategoryViewModel> value)
    {
        if (SelectedCategories.Count == 0)
        {
            ElementCount = 0;
            return;
        }

        ElementCount = CategoryTypes.Sum(type => type.Categories
            .Where(cat => cat.IsChecked)
            .Sum(cat => cat.ElementCount));
    }

    partial void OnElementCountChanged(int value)
    {
        var suffix = value > 1 ? "s" : "";
        
        MainButtonText = value > 0
                ? $"Translate {ElementCount} element{suffix}"
                : "Select category to translate";
    }

    async partial void OnSearchTextChanged(string value)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                ResetCategoriesVisibility();
                return;
            }

            if (_searchCancellationTokenSource is not null)
            {
#if NETFRAMEWORK
                _searchCancellationTokenSource.Cancel();
#else
                await _searchCancellationTokenSource.CancelAsync();
#endif
                _searchCancellationTokenSource.Dispose();
            }

            _searchCancellationTokenSource = new CancellationTokenSource();
            var token = _searchCancellationTokenSource.Token;

            await Task.Delay(300, token).ConfigureAwait(true);

            FilterCategories(value);
        }
        catch (TaskCanceledException)
        {
            // The user typed again before 300ms passed; do nothing.
        }
        catch (Exception ex)
        {
            // do nothing for now
        }
    }

    private void FilterCategories(string text)
    {
        foreach (var categoryType in CategoryTypes)
        {
            var typeHasMatches = false;
            foreach (var category in categoryType.Categories)
            {
#if NETFRAMEWORK
                
#endif
                category.IsVisible = category.Name.Contains(text, StringComparison.InvariantCultureIgnoreCase);
                if (!category.IsVisible) continue;

                typeHasMatches = true;
            }

            categoryType.IsVisible = typeHasMatches;
            if (!typeHasMatches) continue;

            categoryType.IsExpanded = true;
        }
    }

    private void ResetCategoriesVisibility()
    {
        foreach (var categoryType in CategoryTypes)
        {
            categoryType.IsVisible = true;
            categoryType.IsExpanded = false;
            foreach (var category in categoryType.Categories)
            {
                category.IsVisible = true;
            }
        }
    }

    [RelayCommand(CanExecute = nameof(CanTranslate))]
    private void Translate() { }

    private bool CanTranslate() => ElementCount > 0;

    public void Dispose()
    {
        foreach (var type in CategoryTypes)
        {
            type.PropertyChanged -= OnCategoryTypeChanged;
        }
    }
}