namespace TranslationService.Models;
//TODO: Remove Source/Target properties; Leave only LanguageCode property
public record LanguageDescriptor(string VisibleName, string SourceLanguageCode, string TargetLanguageCode);