using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitTranslatorAddin.Utils.DeepL;
internal class DeeplLanguageCodes
{
    // Source: https://developers.deepl.com/docs/v/en/resources/supported-languages
    /// <summary>
    /// Language codes for DeepL. First item is readable language name, second item is language code.
    /// </summary>
    internal static SortedList<string, string> LanguageCodes = new()
    {
        {"Bulgarian", "bg"},
        {"Chinese (simplified)", "zh"},
        {"Czech", "cs"},
        {"Danish", "da"},
        {"Dutch", "nl"},
        {"English (American)", "en-us"},
        {"English (British)", "en-gb"},
        {"Estonian", "et"},
        {"Finnish", "fi"},
        {"French", "fr"},
        {"German", "de"},
        {"Greek", "el"},
        {"Hungarian", "hu"},
        {"Indonesian", "id"},
        {"Italian", "it"},
        {"Japanese", "ja"},
        {"Korean", "ko"},
        {"Latvian", "lv"},
        {"Lithuanian", "lt"},
        {"Norwegian (Bokmål)", "nb"},
        {"Polish", "pl"},
        {"Portuguese (Brazilian)", "pt-br"},
        {"Portuguese (European)", "pt-pt"},
        {"Romanian", "ro"},
        {"Russian", "ru"},
        {"Slovak", "sk"},
        {"Slovenian", "sl"},
        {"Spanish", "es"},
        {"Swedish", "sv"},
        {"Turkish", "tr"},
        {"Ukrainian", "uk"}
    };
}
