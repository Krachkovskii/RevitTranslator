using RevitTranslator.Enums;
using RevitTranslator.Models;
using RevitTranslator.Utils.App;

namespace RevitTranslator.ElementTextRetrievers;

public class ProjectParameterTextRetriever : BaseElementTextRetriever
{
    public ProjectParameterTextRetriever()
    {
        Process(Context.ActiveDocument!);
    }
    
    protected override sealed void Process(object Object)
    {
        if (Object is not Document document) return;

        var projectInfo = document.ProjectInformation;
        
        ProcessProjectInfoProperty(projectInfo.Address, TranslationDetails.ProjectInfoAddress);
        ProcessProjectInfoProperty(projectInfo.Author, TranslationDetails.ProjectInfoAuthor);
        ProcessProjectInfoProperty(projectInfo.BuildingName, TranslationDetails.ProjectInfoBuildingName);
        ProcessProjectInfoProperty(projectInfo.ClientName, TranslationDetails.ProjectInfoClientName);
        ProcessProjectInfoProperty(projectInfo.IssueDate, TranslationDetails.ProjectInfoIssueDate);
        ProcessProjectInfoProperty(projectInfo.Name, TranslationDetails.ProjectInfoName);
        ProcessProjectInfoProperty(projectInfo.Number, TranslationDetails.ProjectInfoNumber);
        ProcessProjectInfoProperty(projectInfo.OrganizationDescription, TranslationDetails.ProjectInfoOrganizationDescription);
        ProcessProjectInfoProperty(projectInfo.OrganizationName, TranslationDetails.ProjectInfoOrganizationName);
        ProcessProjectInfoProperty(projectInfo.Status, TranslationDetails.ProjectInfoStatus);
    }
    
    private void ProcessProjectInfoProperty(string propertyText, TranslationDetails details)
    {
        if (!propertyText.HasText()) return;

        var unit = new TranslationEntity
        {
            Element = Context.ActiveDocument!,
            OriginalText = propertyText,
            TranslationDetails = details
        };

        AddUnitToList(unit);
    }
}
