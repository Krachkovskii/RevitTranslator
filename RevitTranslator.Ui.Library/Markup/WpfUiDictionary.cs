using System.Windows.Markup;
using RevitTranslator.Ui.Library.Appearance;

namespace RevitTranslator.Ui.Library.Markup;

[Localizability(LocalizationCategory.Ignore)]
[Ambient]
[UsableDuringInitialization(true)]
public class WpfUiDictionary : ResourceDictionary
{
    private static readonly ThemesDictionary ThemesDictionary = new();
    private static readonly ControlsDictionary ControlsDictionary = new();

    public ApplicationTheme Theme
    {
        set => ChangeTheme(value);
    }

    public WpfUiDictionary()
    {
        MergedDictionaries.Add(ThemesDictionary);
        MergedDictionaries.Add(ControlsDictionary);
    }

    private void ChangeTheme(ApplicationTheme selectedApplicationTheme)
    {
        // if (selectedApplicationTheme != ThemesDictionary.Theme)
        // {
        //     ThemesDictionary.Theme = selectedApplicationTheme;
        // }
    }
}