using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitTranslatorAddin.Utils.App;
internal class TranslationProcessResult
{
    internal enum AbortReasons
    {
        None,
        Canceled,
        ServerError,
        Other
    }
    internal bool Completed
    {
        get; 
        set; 
    } = false;
    internal AbortReasons AbortReasonResult
    {
        get; 
        set; 
    } = AbortReasons.None;
    internal string ErrorMessage 
    { 
        get; set; 
    } = string.Empty;
    internal TranslationProcessResult(bool result, AbortReasons abortReasonResult, string errorMessage)
    {
        Completed = result;
        AbortReasonResult = abortReasonResult;
        ErrorMessage = errorMessage;
    }
}
