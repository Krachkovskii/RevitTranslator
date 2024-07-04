using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Input;
using RevitTranslatorAddin.Commands;
using RevitTranslatorAddin.Models;
using RevitTranslatorAddin.Utils;

namespace RevitTranslatorAddin.ViewModels;
public class TranslateCategoriesViewModel : ObservableObject
{

    public ObservableCollection<ListItem> Categories { get; } = [];
    private List<ListItem> _selectedCategories = [];
    private TranslationUtils _utils = null;

    private int _elementCount
    {
        set
        {
            if (value == 0)
            {
                if (Categories.Where(item => item.IsSelected).Any())
                {
                    TranslateButtonText = "No elements of selected categories";
                }
                else
                {
                    TranslateButtonText = "Select Categories to translate";
                }
            }
            else
            {
                TranslateButtonText = $"Translate {value} elements";
            }
        } 
    }

    private string _translateButtonText;
    public string TranslateButtonText
    {
        get => _translateButtonText;
        set
        {
            _translateButtonText = value;
            OnPropertyChanged(nameof(TranslateButtonText));
        }
    }

    public ICommand SelectAllCommand { get; }
    public ICommand SelectNoneCommand { get; }
    public ICommand InvertSelectionCommand { get; }
    public ICommand TranslateSelectedCommand { get; }

    private void SelectAll() => SetAllCategoriesSelection(true);
    private void SelectNone() => SetAllCategoriesSelection(false);
    private void InvertSelection() => Categories.ToList().ForEach(item => item.IsSelected = !item.IsSelected);

    public TranslateCategoriesViewModel(TranslationUtils utils)
    {
        foreach (Category c in CategoriesModel.AllCategories)
        {
            Categories.Add(new ListItem(c, this));
        }

        _utils = utils;
        TranslateButtonText = "Translate";

        SelectAllCommand = new RelayCommand(SelectAll);
        SelectNoneCommand = new RelayCommand(SelectNone);
        InvertSelectionCommand = new RelayCommand(InvertSelection);
        TranslateSelectedCommand = new RelayCommand(TranslateSelected);
    }

    private void SetAllCategoriesSelection(bool isSelected)
    {
        foreach (var item in Categories)
        {
            item.IsSelected = isSelected;
        }

        //if (!isSelected)
        //{
        //    TranslateButtonText = "Select Categories to translate";
        //}
    }

    private void TranslateSelected()
    {
        var categories = Categories.Where(item => item.IsSelected).Select(c => c.Category.BuiltInCategory).ToList();
        if (categories.Count == 0) { return; }
        var elements = TranslateCategoriesCommand.GetElementsFromCategories(categories);

        ProgressWindowUtils.Start();

        _utils.StartTranslation(elements);
        if (TranslationUtils.Translations.Count > 0)
        {
            TranslateCategoriesCommand.TranslateCategoriesExtrnalEvent.Raise();
        }
        ProgressWindowUtils.End();
    }

    internal void CountElements()
    {
        // TODO: Deal with updating UI on the main thread
        Task.Run(() =>
        {
            var categories = Categories.Where(item => item.IsSelected).Select(c => c.Category.BuiltInCategory).ToList();
            var count = TranslateCategoriesCommand.GetElementsFromCategories(categories).Count;

            _elementCount = count;
        });
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class ListItem : INotifyPropertyChanged
{
    private readonly TranslateCategoriesViewModel _viewModel;
    
    public string Name
    {
        get; private set;
    }

    public string Type
    {
        get; private set; 
    }

    private Category _category;
    public Category Category
    {
        get => _category;
        set
        {
            _category = value;
            Name = _category.Name;
            if (_category.CategoryType == CategoryType.AnalyticalModel)
            {
                Type = "Analytical Model Categories";
            }
            else
            {
                Type = _category.CategoryType.ToString() + " Categories";
            }
        }
    }

    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected != value)
            {
                _isSelected = value;
                OnPropertyChanged();
            }

            // TODO: Implement asynchronous element count
            //if (_isSelected)
            //{
            //    _viewModel.CountElements();
            //}
        }
    }
    public ListItem(Category category, TranslateCategoriesViewModel viewModel)
    {
        _viewModel = viewModel;
        Category = category;
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
