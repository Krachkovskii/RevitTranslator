using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using RevitTranslator.Common.App.Models;

namespace RevitTranslator.UI.Contracts;

public interface ICategoriesWindowViewModel
{
    public string MainButtonText { get; }
    public string SearchText { get; set; }
    public bool IsLoading { get; }
    public ObservableCategoryType[] CategoryTypes { get; }
    public ObservableCollection<ObservableCategoryType> FilteredCategoryTypes { get; set; }
    public List<ObservableCategoryDescriptor> SelectedCategories { get; }
    IRelayCommand TranslateCommand { get; }
}