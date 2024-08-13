namespace RevitTranslatorAddin.Utils.App;
/// <summary>
/// This class handles cancellation token, its creation and disposal
/// </summary>
internal class CancellationTokenHandler
{
    /// <summary>
    /// Active CancellationTokenSource
    /// </summary>
    internal CancellationTokenSource Cts { get; private set; } = null;

    /// <summary>
    /// Creates and sets a new CancellationTokenSource instance
    /// </summary>
    internal void Create()
    {
        Cts = new CancellationTokenSource();
    }

    /// <summary>
    /// Disposes and resets the CancellationTokenSource
    /// </summary>
    internal void Clear()
    {
        Cts.Dispose();
        Cts = null;
    }
}
