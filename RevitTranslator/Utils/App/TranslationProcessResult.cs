namespace RevitTranslator.Utils.App;
/// <summary>
/// Represents a report for terminated translation process
/// </summary>
public class TranslationProcessResult(
    bool result,
    TranslationProcessResult.AbortReasons abortReasonResult,
    string errorMessage)
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
    public bool Completed { get; set; } = result;

    /// <summary>
    /// Reason for cancellation
    /// </summary>
    public AbortReasons AbortReasonResult { get; set; } = abortReasonResult;

    /// <summary>
    /// Error message, if applicable
    /// </summary>
    public string ErrorMessage { get; set; } = errorMessage;
}
