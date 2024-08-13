namespace RevitTranslatorAddin.Utils.App;
/// <summary>
/// Represents a report for terminated translation process
/// </summary>
internal class TranslationProcessResult
{
    /// <summary>
    /// Enum of cancellation reasons
    /// </summary>
    internal enum AbortReasons
    {
        None,
        Canceled,
        ServerError,
        Other
    }

    /// <summary>
    /// Denotes whether all translations have been completed successfully
    /// </summary>
    internal bool Completed
    {
        get; 
        set; 
    } = false;

    /// <summary>
    /// Reason for cancellation
    /// </summary>
    internal AbortReasons AbortReasonResult
    {
        get; 
        set; 
    } = AbortReasons.None;

    /// <summary>
    /// Error message, if applicable
    /// </summary>
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
