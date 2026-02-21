using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using RevitTranslator.Common.Contracts;
using RevitTranslator.Common.Extensions;
using RevitTranslator.Common.Messages;
using RevitTranslator.Common.Models.Views;
using RevitTranslator.UI.Extensions;

namespace RevitTranslator.UI.ViewModels;

public sealed partial class SheetsViewModel : ObservableObject, IDisposable
{
    [ObservableProperty] private string _searchText = "";
    [ObservableProperty] private string _mainButtonText = "";
    [ObservableProperty] private ObservableCollection<ViewGroupViewModel> _viewGroups = [];

    [NotifyCanExecuteChangedFor(nameof(TranslateCommand))]
    [ObservableProperty]
    private int _elementCount;

    private CancellationTokenSource? _searchCancellationTokenSource;

    public SheetsViewModel(IRevitViewProvider viewProvider)
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

        var sheetCollections = viewProvider.GetAllSheetCollections();

        if (sheetCollections.Count == 0)
        {
            sheetGroup.IsExpanded = true;
            ViewGroups = new ObservableCollection<ViewGroupViewModel>([sheetGroup]);
        }
        else
        {
            ViewGroups = sheetCollections
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

        foreach (var group in ViewGroups)
        {
            group.PropertyChanged += OnViewGroupChanged;
        }

        OnElementCountChanged(0);
    }

    private void OnViewGroupChanged(object? sender, PropertyChangedEventArgs args)
    {
        if (args.PropertyName != nameof(ViewGroupViewModel.SelectedElementCount)) return;

        ElementCount = ViewGroups.Sum(group => group.SelectedElementCount);
    }

    partial void OnElementCountChanged(int value)
    {
        var suffix = value != 1 ? "s" : "";
        MainButtonText = value > 0
            ? $"Translate {value} element{suffix}"
            : "Select views to translate";
    }

    [RelayCommand(CanExecute = nameof(CanTranslate))]
    private void Translate()
    {
        var selectedViews = ViewGroups
            .SelectMany(group => group.Views)
            .Where(view => view.IsChecked)
            .Select(view => view.Model)
            .ToArray();

        WeakReferenceMessenger.Default.Send(new ViewsSelectedMessage(selectedViews));
    }

    private bool CanTranslate() => ElementCount > 0;

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
        catch (Exception)
        {
            // do nothing for now
        }
    }

    private void FilterCategories(string text)
    {
        foreach (var viewType in ViewGroups)
        {
            var typeHasMatches = false;
            foreach (var view in viewType.Views)
            {
                view.IsVisible = view.Name.Contains(text, StringComparison.InvariantCultureIgnoreCase);
                if (!view.IsVisible) continue;

                typeHasMatches = true;
            }

            viewType.IsVisible = typeHasMatches;
            if (!typeHasMatches) continue;

            viewType.IsExpanded = true;
        }
    }

    private void ResetCategoriesVisibility()
    {
        foreach (var viewType in ViewGroups)
        {
            viewType.IsVisible = true;
            viewType.IsExpanded = false;
            foreach (var view in viewType.Views)
            {
                view.IsVisible = true;
            }
        }
    }

    public void Dispose()
    {
        foreach (var group in ViewGroups)
        {
            group.PropertyChanged -= OnViewGroupChanged;
        }
    }
}
