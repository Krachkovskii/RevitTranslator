using RevitTranslatorAddin.Utils.App;
using RevitTranslatorAddin.Utils.Revit;

namespace RevitTranslatorAddin.Utils.ElementTextRetrievers;
internal class ProjectParameterTextRetriever : BaseParameterTextRetriever
{
    internal ProjectParameterTextRetriever()
    {
        Process(RevitUtils.Doc);
    }

    /// <summary>
    /// Creates and adds a new Translation Unit with corresponding Translation Details
    /// </summary>
    /// <param name="propertyText"></param>
    /// <param name="details"></param>
    private void ProcessProjectInfoProperty(string propertyText, TranslationDetails details)
    {
        if (!ValidationUtils.HasText(propertyText))
        {
            return;
        }

        var unit = new TranslationUnit(RevitUtils.Doc, propertyText, details);
        AddUnitToList(unit);
    }

    /// <summary>
    /// Processes all Document-related properties
    /// </summary>
    /// <param name="Object">
    /// Document object to process
    /// </param>
    protected override void Process(object Object)
    {
        if (Object is not Document document) 
        {
            return; 
        }

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
}
