using RevitTranslatorAddin.Utils.DeepL;

namespace RevitTranslatorAddin.Utils.App;
/// <summary>
/// This class handles creation and management of concurrent translation tasks.
/// </summary>
internal class MultiTaskTranslationHandler
{
    private List<TranslationUnitGroup> _unitGroups { get; set; } = null;
    internal int TotalTranslationCount { get; private set; } = 0;

    private readonly List<Task> _translationTasks = [];
    private readonly TranslationUtils _translationUtils = null;
    private readonly ProgressWindowUtils _progressWindowUtils = null;
    private TranslationProcessResult _processResult { get; set; } = new(false, TranslationProcessResult.AbortReasons.None, string.Empty);
    
    /// <summary>
    /// Token handler for this instance
    /// </summary>
    internal CancellationTokenHandler TokenHandler { get; private set; } = null;

    internal MultiTaskTranslationHandler(TranslationUtils translationUtils, 
                                            List<TranslationUnitGroup> unitGroups, 
                                            ProgressWindowUtils progressWindowUtils)
    {
        _translationUtils = translationUtils;
        _progressWindowUtils = progressWindowUtils;
        _unitGroups = unitGroups;
        TotalTranslationCount = CalculateTotalTranslations(_unitGroups);
    }

    /// <summary>
    /// Calculates total number of units across all unit groups.
    /// </summary>
    /// <param name="unitGroups"></param>
    /// <returns>
    /// Total number of translation units.
    /// </returns>
    private int CalculateTotalTranslations(List<TranslationUnitGroup> unitGroups)
    {
        return unitGroups.Sum(x => x.TranslationUnits.Count);
    }

    /// <summary>
    /// Perform translation of all available translation units
    /// </summary>
    /// <returns>
    /// State of translation process via TranslationProcessResult object.
    /// </returns>
    internal TranslationProcessResult TranslateUnits()
    {
        CreateTranslationTasks(_unitGroups);

        // this is executed after all tasks have finished
        TokenHandler.Dispose();

        return _processResult;
    }

    /// <summary>
    /// Creates translation tasks and handles cancellation exceptions
    /// </summary>
    private void CreateTranslationTasks(List<TranslationUnitGroup> unitGroups)
    {
        SetupTokenHandler();
        _progressWindowUtils.StartTranslationStatus();

        try
        {
            _progressWindowUtils.UpdateTotal(TotalTranslationCount);

            foreach (var group in unitGroups)
            {
                foreach (var unit in group.TranslationUnits)
                {
                    TokenHandler.Cts.Token.ThrowIfCancellationRequested();
                    AddTranslationTask(_translationTasks, unit);
                } 
            }

            Task.WaitAll(_translationTasks.ToArray());
            _processResult.Completed = true;
        }

        catch (OperationCanceledException)
        {
            HandleCanceledOperation();
        }

        catch (AggregateException ae)
        {
            HandleAggregateException(ae);
        }

        catch (Exception ex)
        {
            HandleOtherExceptions(ex);
        }
    }

    /// <summary>
    /// Starts translation task and adds it to the list of tasks
    /// </summary>
    /// <param name="unit"></param>
    private void AddTranslationTask(List<Task> translationTasks, TranslationUnit unit)
    {
        translationTasks.Add(Task.Run(async () =>
        {
            TokenHandler.Cts.Token.ThrowIfCancellationRequested();
            var translated = await _translationUtils.TranslateTextAsync(unit.OriginalText, TokenHandler.Cts.Token);
            unit.TranslatedText = translated;
        }, TokenHandler.Cts.Token));
    }

    private void HandleCanceledOperation()
    {
        _processResult.Completed = false;
        _processResult.AbortReasonResult = TranslationProcessResult.AbortReasons.Canceled;
        _processResult.ErrorMessage = "Translation process was cancelled by user";
    }

    private void HandleAggregateException(AggregateException ae)
    {
        _processResult.Completed = false;

        if (ae.InnerExceptions.Any(e => e is OperationCanceledException))
        {
            _processResult.AbortReasonResult = TranslationProcessResult.AbortReasons.Canceled;
            _processResult.ErrorMessage = "Translation process was cancelled by user";
        }

        else 
        { 
            _processResult.AbortReasonResult = TranslationProcessResult.AbortReasons.Other;
            _processResult.ErrorMessage = ae.Message;
        }
    }

    private void HandleOtherExceptions(Exception ex)
    {
        _processResult.Completed = false;
        _processResult.AbortReasonResult = TranslationProcessResult.AbortReasons.Other;
        _processResult.ErrorMessage = ex.Message;
    }

    /// <summary>
    /// Sets up all necessary properties for cancellation tokens across the project.
    /// </summary>
    private void SetupTokenHandler()
    {
        TokenHandler = new CancellationTokenHandler();
        TokenHandler.Create();
        _progressWindowUtils.TokenHandler = TokenHandler;
        _progressWindowUtils.VM.Cts = TokenHandler.Cts;
    }
}
