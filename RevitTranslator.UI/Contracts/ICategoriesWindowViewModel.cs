using CommunityToolkit.Mvvm.Input;
using RevitTranslator.Common.Models;

namespace RevitTranslator.UI.Contracts;

public interface ICategoriesWindowViewModel
{
    public string MainButtonText { get; }
    public string SearchText { get; set; }
    public bool IsLoading { get; }
    
    public ObservableCategoryType[] CategoryTypes { get; }
    public List<ObservableCategoryType> FilteredCategoryTypes { get; set; }
    public List<ObservableCategoryDescriptor> SelectedCategories { get; }

    void OnCloseRequested();
    
    IRelayCommand TranslateCommand { get; }
}