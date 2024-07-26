using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Autodesk.Revit.UI;
using RevitTranslatorAddin.Commands;
using RevitTranslatorAddin.Models;
using RevitTranslatorAddin.Utils.DeepL;
using RevitTranslatorAddin.Utils.Revit;

namespace RevitTranslatorAddin.ViewModels;
public class TranslateCategoriesViewModel : INotifyPropertyChanged
{

    private List<ListItem> _selectedCategories = [];
    private readonly TranslationUtils _utils = null;
    public ObservableCollection<ListItem> Categories { get; } = [];

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

    public ICommand SelectAllInGroupCommand => new RelayCommand<Tuple<bool, string>>(ExecuteSelectAllInGroup);
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

    public ICommand TranslateSelectedCommand => new RelayCommand(TranslateSelected);
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

        ProgressWindowUtils.Start();

        var finishedTask = Task.Run(() => _utils.StartTranslationAsync(elements));
        var finished = finishedTask.GetAwaiter().GetResult();
        //var finished = _utils.StartTranslation(elements);

        if (TranslationUtils.Translations.Count > 0)
        {
            if (!finished)
            {
                var proceed = TranslationUtils.ProceedWithUpdate();
                if (!proceed)
                {
                    return;
                }
            }

            //TranslateCategoriesCommand.TranslateCategoriesExternalEvent.Raise();
            RevitUtils.ExEvent.Raise();
            RevitUtils.SetTemporaryFocus();
        //}
        //else
        //{
            // shutting down the window ONLY in case if there are no translations, i.e. event is not triggered
            ProgressWindowUtils.End();
        }
    }

    public TranslateCategoriesViewModel(TranslationUtils utils)
    {

        foreach (Category c in CategoriesModel.AllCategories)
        {
            Categories.Add(new ListItem(c, this));
        }

        _utils = utils;
        TranslateButtonText = "Translate";
    }

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

    public event PropertyChangedEventHandler PropertyChanged;
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

