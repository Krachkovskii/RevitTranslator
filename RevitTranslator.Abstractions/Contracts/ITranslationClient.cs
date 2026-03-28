namespace RevitTranslator.Abstractions.Contracts;

public interface ITranslationClient
{
    Task<string?[]?> TranslateTextsAsync(string[] texts, CancellationToken token);
}