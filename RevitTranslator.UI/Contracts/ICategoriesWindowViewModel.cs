using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using RevitTranslator.Common.Models;
using RevitTranslator.Common.Models.Categories;

namespace RevitTranslator.UI.Contracts;

public interface ICategoriesWindowViewModel
{
    public string MainButtonText { get; }
    public string SearchText { get; set; }
    public bool IsLoading { get; }
    
    public ObservableCollection<CategoryTypeViewModel> CategoryTypes { get; set; }
    public List<CategoryViewModel> SelectedCategories { get; }
    
    IRelayCommand TranslateCommand { get; }
    IRelayCommand LoadedCommand { get; }
}