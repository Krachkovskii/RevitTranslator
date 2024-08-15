using RevitTranslatorAddin.Utils.DeepL;

namespace RevitTranslatorAddin.Utils.App;
/// <summary>
/// This class handles creation and management of concurrent translation tasks.
/// </summary>
internal class MultiTaskTranslationHandler
{
    private List<TranslationUnitGroup> _unitGroups { get; set; }
    private int _totalTranslationCount = 0;
    private bool _test = true;


    private readonly List<Task> _translationTasks = [];
    private readonly List<TranslationUnit> _translationUnits = [];
    private readonly TranslationUtils _translationUtils = null;
    private readonly ProgressWindowUtils _progressWindowUtils = null;
    private TranslationProcessResult _processResult { get; set; } = new(false, TranslationProcessResult.AbortReasons.None, string.Empty);
    
    /// <summary>
    /// Token handler for this instance
    /// </summary>
    internal CancellationTokenHandler TokenHandler { get; private set; } = null;


    internal MultiTaskTranslationHandler(TranslationUtils translationUtils, 
                                            List<TranslationUnit> units, 
                                            ProgressWindowUtils progressWindowUtils)
    {
        _translationUtils = translationUtils;
        _translationUnits = units;
        _progressWindowUtils = progressWindowUtils;
    }

    internal MultiTaskTranslationHandler(TranslationUtils translationUtils, 
                                            List<TranslationUnitGroup> unitGroups, 
                                            ProgressWindowUtils progressWindowUtils)
    {
        _translationUtils = translationUtils;
        _progressWindowUtils = progressWindowUtils;
        _unitGroups = unitGroups;
        _totalTranslationCount = CalculateTotalTranslations(_unitGroups);
    }

    private int CalculateTotalTranslations(List<TranslationUnitGroup> unitGroups)
    {
        var i = 0;
        foreach (var group in unitGroups)
        {
            i += group.TranslationUnits.Count;
        }
        return i;
    }

    /// <summary>
    /// Perform translation of all available translation units
    /// </summary>
    /// <returns></returns>
    internal TranslationProcessResult PerformTranslation()
    {
        CreateTranslationTasks();

        // this is executed after all tasks have finished
        TokenHandler.Clear();

        return _processResult;
    }

    /// <summary>
    /// Creates translation tasks and handles cancellation exceptions
    /// </summary>
    private void CreateTranslationTasks()
    {
        SetupTokenHandler();
        _progressWindowUtils.StartTranslationStatus();

        
        

        try
        {
            //test
            if (_test)
            {
                _progressWindowUtils.UpdateTotal(_totalTranslationCount);

                foreach (var group in _unitGroups)
                {
                    foreach (var unit in group.TranslationUnits)
                    {
                        if (TokenHandler.Cts.Token.IsCancellationRequested)
                        {
                            throw new OperationCanceledException();
                        }
                        AddTranslationTask(unit);
                    } 
                }

                Task.WaitAll(_translationTasks.ToArray());
                _processResult.Completed = true;
            }

            else
            {
                _progressWindowUtils.UpdateTotal(_translationUnits.Count);

                foreach (var unit in _translationUnits)
                {
                    if (TokenHandler.Cts.Token.IsCancellationRequested)
                    {
                        throw new OperationCanceledException();
                    }
                    AddTranslationTask(unit);
                }

                Task.WaitAll(_translationTasks.ToArray());
                _processResult.Completed = true;
            }
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
    private void AddTranslationTask(TranslationUnit unit)
    {
        _translationTasks.Add(Task.Run(async () =>
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
        if (ae.InnerExceptions.Any(e => e is OperationCanceledException))
        {
            _processResult.Completed = false;
            _processResult.AbortReasonResult = TranslationProcessResult.AbortReasons.Canceled;
            _processResult.ErrorMessage = "Translation process was cancelled by user";
        }

        else { 
            _processResult.Completed = false;
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
