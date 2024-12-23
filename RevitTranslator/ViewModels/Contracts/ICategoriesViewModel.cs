using System.Collections.ObjectModel;
using RevitTranslator.Models;

namespace RevitTranslatorAddin.ViewModels.Contracts;

public interface ICategoriesViewModel
{
    public CategoryDescriptor[] AllCategories { get; }
    public string MainButtonText { get; }
    public ObservableCollection<CategoryDescriptor> SelectedCategories { get; }
    IRelayCommand SelectAllInGroupCommand { get; }
    IRelayCommand TranslateSelectedCommand { get; }
}