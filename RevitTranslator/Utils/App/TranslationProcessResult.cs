namespace RevitTranslator.Utils.App;
/// <summary>
/// Represents a report for terminated translation process
/// </summary>
public class TranslationProcessResult
{
    /// <summary>
    /// Enum of cancellation reasons
    /// </summary>
    public enum AbortReasons
    {
        None,
        Canceled,
        ServerError,
        Other
    }

    /// <summary>
    /// Denotes whether all translations have been completed successfully
    /// </summary>
    public bool Completed
    {
        get; 
        set; 
    } = false;

    /// <summary>
    /// Reason for cancellation
    /// </summary>
    public AbortReasons AbortReasonResult
    {
        get; 
        set; 
    } = AbortReasons.None;

    /// <summary>
    /// Error message, if applicable
    /// </summary>
    public string ErrorMessage 
    { 
        get; set; 
    } = string.Empty;

    public TranslationProcessResult(bool result, AbortReasons abortReasonResult, string errorMessage)
    {
        Completed = result;
        AbortReasonResult = abortReasonResult;
        ErrorMessage = errorMessage;
    }
}
