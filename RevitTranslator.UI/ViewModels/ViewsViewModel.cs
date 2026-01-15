using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using RevitTranslator.Common.Contracts;
using RevitTranslator.Common.Extensions;
using RevitTranslator.UI.Extensions;
using RevitTranslator.Common.Models.Views;

namespace RevitTranslator.UI.ViewModels;

public sealed partial class ViewsViewModel : ObservableObject
{
    [ObservableProperty] private string _searchText = "";
    [ObservableProperty] private ObservableCollection<ViewGroupViewModel> _viewTypes = [];

    private CancellationTokenSource? _searchCancellationTokenSource;
    
    public ViewsViewModel(IRevitViewProvider viewProvider)
    {
        var sheets = viewProvider.GetAllSheets().ToArray();

        var sheetGroup = new ViewGroupViewModel
        {
            Views = sheets
                .OrderBy(view => view.Name)
                .Select(view => new ViewViewModel { Model = view })
                .ToObservableCollection(),
            Name = "All Sheets",
        };

        var sheetCollections = viewProvider
            .GetAllSheetCollections();

        if (sheetCollections.Count == 0)
        {
            sheetGroup.IsExpanded = true;
            ViewTypes = new ObservableCollection<ViewGroupViewModel>([sheetGroup]);
            return;
        }
        
        ViewTypes = sheetCollections
            .Select(collection => new ViewGroupViewModel
            {
                Views = collection
                    .Views
                    .OrderBy(view => view.Name)
                    .Select(view => new ViewViewModel { Model = view })
                    .ToObservableCollection(),
                Name = collection.Name
            })
            .Concat([sheetGroup])
            .ToObservableCollection();
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
                _searchCancellationTokenSource.Cancel();
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
        foreach (var categoryType in ViewTypes)
        {
            var typeHasMatches = false;
            foreach (var category in categoryType.Views)
            {
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
        foreach (var categoryType in ViewTypes)
        {
            categoryType.IsVisible = true;
            categoryType.IsExpanded = false;
            foreach (var category in categoryType.Views)
            {
                category.IsVisible = true;
            }
        }
    }
}