using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using RevitTranslatorAddin.Utils.DeepL;
using RevitTranslatorAddin.ViewModels;

namespace RevitTranslatorAddin.Utils.App;
internal class MultiTaskTranslationHandler
{
    private readonly List<Task> _translationTasks = [];
    private readonly List<TranslationUnit> _translationUnits = [];
    private readonly TranslationUtils _translationUtils = null;
    internal CancellationTokenHandler TokenHandler { get; private set; } = null;
    private readonly ProgressWindowUtils _progressWindowUtils = null;
    private TranslationProcessResult _processResult { get; set; } = new(false, TranslationProcessResult.AbortReasons.None, string.Empty);
    internal MultiTaskTranslationHandler(TranslationUtils translationUtils, 
        List<TranslationUnit> units, 
        ProgressWindowUtils progressWindowUtils)
    {
        _translationUtils = translationUtils;
        _translationUnits = units;
        _progressWindowUtils = progressWindowUtils;
    }

    internal TranslationProcessResult StartTranslation()
    {
        CreateTranslationTasks();

        // this is executed after all tasks have finished
        TokenHandler.Clear();

        return _processResult;
    }

    private void CreateTranslationTasks()
    {
        SetupTokenHandler();
        _progressWindowUtils.UpdateTotal(_translationUnits.Count);

        try
        {
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

    private void SetupTokenHandler()
    {
        TokenHandler = new CancellationTokenHandler();
        TokenHandler.Create();
        _progressWindowUtils.TokenHandler = TokenHandler;
        _progressWindowUtils.VM.Cts = TokenHandler.Cts;
    }
}
