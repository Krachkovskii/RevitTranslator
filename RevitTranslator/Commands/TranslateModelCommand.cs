using System.Windows;
using Nice3point.Revit.Toolkit.External;
using RevitTranslator.Services;

namespace RevitTranslator.Commands;

//TODO: Move to separate service
[UsedImplicitly]
public class TranslateModelCommand : ExternalCommand
{
    public override void Execute()
    {
        var instances = Document.EnumerateInstances().ToArray();
        
        var service = new BaseTranslationService();
        service.SelectedElements = instances;
        service.Execute();
    }

    /// <summary>
    /// Shows the warning that contains approximate number of elements. Allows to cancel the operation.
    /// </summary>
    /// <param name="count"></param>
    /// <returns></returns>
    private static bool ShowElementCountWarning(int count)
    {
        var result = MessageBox.Show($"You're about to translate all text properties of {count}+ elements. " +
                $"It can be time-consuming and you might hit translation limits.\n\n" +
                $"It will also translate elements such as view names or any annotations.\n" +
                $"Consider selecting only necessary categories.\n\n" +
                $"Are you sure you want to translate the whole model?",
                "Large number of translations!",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes) { return true; }
        else { return false; }
    }
}
