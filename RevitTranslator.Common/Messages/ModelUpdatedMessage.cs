namespace RevitTranslator.Common.Messages;

public record ModelUpdatedMessage(int NonUpdatedEntitiesCount, int UpdatedInModelCount, int UpdatedFamiliesCount);