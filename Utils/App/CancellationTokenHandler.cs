namespace RevitTranslatorAddin.Utils.App;
/// <summary>
/// This class handles cancellation token, its creation and disposal
/// </summary>
public class CancellationTokenHandler
{
    /// <summary>
    /// Active CancellationTokenSource
    /// </summary>
    public CancellationTokenSource Cts { get; private set; } = null;

    /// <summary>
    /// Creates and sets a new CancellationTokenSource instance
    /// </summary>
    public void Create()
    {
        Cts = new CancellationTokenSource();
    }

    /// <summary>
    /// Disposes and resets the CancellationTokenSource
    /// </summary>
    public void Clear()
    {
        Cts.Dispose();
        Cts = null;
    }
}
