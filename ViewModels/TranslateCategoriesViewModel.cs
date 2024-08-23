using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using RevitTranslatorAddin.Commands;
using RevitTranslatorAddin.Models;
using RevitTranslatorAddin.Utils.App;
using RevitTranslatorAddin.Utils.DeepL;
using RevitTranslatorAddin.Utils.Revit;

namespace RevitTranslatorAddin.ViewModels;
public class TranslateCategoriesViewModel : INotifyPropertyChanged
{
    private readonly TranslationUtils _translationUtils = null;
    private string _mainButtonText;
    public TranslateCategoriesViewModel(TranslationUtils utils, ProgressWindowUtils windowUtils)
    {

        foreach (Category c in CategoriesModel.AllValidCategories)
        {
            Categories.Add(new ListItem(c, this));
        }

        _translationUtils = utils;
        _progressWindowUtils = windowUtils;
        MainButtonText = "Translate";
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public ObservableCollection<ListItem> Categories { get; } = [];
    public ICommand SelectAllInGroupCommand => new RelayCommand<Tuple<bool, string>>(ExecuteSelectAllInGroup);
    
    public string MainButtonText
    {
        get => _mainButtonText;
        set
        {
            _mainButtonText = value;
            OnPropertyChanged(nameof(MainButtonText));
        }
    }

    public ICommand TranslateSelectedCommand => new RelayCommand(TranslateSelected);
    
    /// <summary>
    /// Counts the number of elements in all selected categories
    /// </summary>
    private int _elementCount
    {
        set
        {
            if (value == 0)
            {
                if (Categories.Where(item => item.IsSelected).Any())
                {
                    MainButtonText = "No elements of selected categories";
                }
                else
                {
                    MainButtonText = "Select Categories to translate";
                }
            }
            else
            {
                MainButtonText = $"Translate {value} elements";
            }
        }
    }

    private ProgressWindowUtils _progressWindowUtils { get; set; } = null;
    /// <summary>
    /// Counts the number of elements in selected categories and asynchronously updates the value.
    /// </summary>
    internal void CountElements()
    {
        Task.Run(() =>
        {
            var categories = Categories.Where(item => item.IsSelected).Select(c => c.Category.BuiltInCategory).ToList();
            var count = TranslateCategoriesCommand.GetElementsFromCategories(categories).Count;

            _elementCount = count;
        });
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Checks or unchecks all categories in selected Category Type.
    /// </summary>
    /// <param name="parameter">
    /// Passed by GroupCheckboxStateConverter.
    /// </param>
    private void ExecuteSelectAllInGroup(Tuple<bool, string> parameter)
    {
        if (parameter != null)
        {
            bool isChecked = parameter.Item1;
            string type = parameter.Item2;

            // Example: Update all items in the group
            var itemsInGroup = Categories.Where(item => item.Type == type);
            foreach (var item in itemsInGroup)
            {
                item.IsSelected = isChecked;
            }
        }
    }
    /// <summary>
    /// Command for translation of all elements in selected categories. 
    /// Starts translation process and calls the raise of ExternalEvent to update Revit model.
    /// </summary>
    private void TranslateSelected()
    {
        var categories = Categories.Where(item => item.IsSelected).Select(c => c.Category.BuiltInCategory).ToList();
        if (categories.Count == 0) { return; }
        var elements = TranslateCategoriesCommand.GetElementsFromCategories(categories);

        TranslateCategoriesCommand.Window.Close();
        TranslateCategoriesCommand.Window = null;

        BaseTranslationCommand.StartCommandTranslation(elements, _progressWindowUtils, _translationUtils, false, false);
    }
}

